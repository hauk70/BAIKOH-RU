using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Lib.Util;
using UniRx;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoSingleton<GameController>
{
    /*
     TODO 
     Canvas Group
     Aspect Scaling
     anchors
    */

    public const int ROWS_STARTED = 6;
    public const int ROWS = 8;
    public const int COLUMNS = 8;
    public const int ROUND_DURATION = 60;
    public const int TIMER_TICK_INTERVAL = 1000;
    public const int ADDITIONAL_CHARACTERS_TIMER_INTERVAL = 4000;
    public const float FALL_SPEED = .25f;

    public Event<int> TimerCounterChangedEvent = new Event<int>();
    public Event<int> CollectedScoreChangedEvent = new Event<int>();
    public Event<string> WordChangedEvent = new Event<string>();

    public Event<CellData> SelectCell = new Event<CellData>();
    public Event<CellData> UnselectCell = new Event<CellData>();

    public GameObject CellPrefab;
    public GameObject Field;

    private CellData[,] _field;

    private HashSet<string> _words;
    private Dictionary<char, CharacterRule> _rules;
    private Dictionary<char, Sprite> _charactersSprites;
    private Dictionary<char, Sprite> _charactersActiveSprites;

    private int _collectedScoreInRound;
    private int _maxCollectedScore;

    private List<CellData> _selectedCells;
    private List<CellData> _cells;

    private bool _isMoreCharactersUsed;

    private IDisposable _roundTimer;
    private IDisposable _characterFallTimer;
    private int _ticksOfRound;

    public override void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        
        LoadRules();
        LoadWords();
        LoadSprites();

        SelectCell.AddListener(OnSelectedCell);
        UnselectCell.AddListener(OnUnselectedCell);

        StateController.Instance.OnPrepareRoundStart.AddListener(PrepareGame);

        StateController.Instance.OnGameStart.AddListener(StartGame);
        StateController.Instance.OnGameEnd.AddListener(ClearField);
    }

    public void ApplyWord()
    {
        string word = GetSelectedWord();

        if (word.Length == 0)
        {
            return;
        }

        if (!_words.Contains(word.ToLower()))
        {
            return;
        }

        foreach (var cell in _selectedCells)
        {
            _field[(int)-cell.transform.localPosition.y, (int)cell.transform.localPosition.x] = null;
            _cells.Remove(cell);
            ObjectPoolManager.Instance.Push(CellPrefab, cell);
        }
        _selectedCells = new List<CellData>();

        MoveCells();

        if (word.Length < 6)
        {
            _collectedScoreInRound += word.Length;
        }
        else
        {
            _collectedScoreInRound += word.Length * 2;
        }

        CollectedScoreChangedEvent.Invoke(_collectedScoreInRound);
        WordChangedEvent.Invoke("");
    }

    public void UnselectAll()
    {
        foreach (var cell in _selectedCells)
        {
            cell.Unselect(false);
        }
        _selectedCells.Clear();
        WordChangedEvent.Invoke("");
    }

    public void SpawnMoreCharacters()
    {
        if (_isMoreCharactersUsed)
        {
            return;
        }

        var charsCountToSpawn = Math.Min(_cells.Count - ROWS - COLUMNS, 6);
        if (charsCountToSpawn == 0)
        {
            return;
        }

        _isMoreCharactersUsed = true;

        var usedChars = GenerateUsedChars();

        var keys = usedChars.Keys.ToArray();
        foreach (var key in keys)
        {
            if (usedChars[key] >= 2)
            {
                usedChars.Remove(key);
            }
        }

        var vowelsCount = Mathf.Clamp(Random.Range(2, 4), Mathf.Min(2, charsCountToSpawn), Mathf.Min(4, charsCountToSpawn));
        var consonantsCount = charsCountToSpawn - vowelsCount;

        var charsToSpawn = new List<char>();
        var vowels = new List<char>();
        var consonants = new List<char>();

        foreach (var c in usedChars)
        {
            if (_rules[c.Key].IsVowel)
            {
                vowels.Add(c.Key);
            }
            else
            {
                consonants.Add(c.Key);
            }
        }

        while (vowelsCount > 0)
        {
            var index = Random.Range(0, vowels.Count);
            if (usedChars[vowels[index]] + 1 > 2)
            {
                continue;
            }
            charsToSpawn.Add(vowels[index]);
            usedChars[vowels[index]]++;
            vowelsCount--;
        }

        while (consonantsCount > 0)
        {
            var index = Random.Range(0, consonants.Count);
            if (usedChars[consonants[index]] + 1 > 2)
            {
                continue;
            }
            charsToSpawn.Add(consonants[index]);
            usedChars[consonants[index]]++;
            consonantsCount--;
        }

        foreach (var charToSpawn in charsToSpawn)
        {
            SpawnCharacter(GetEmptyCellPosition(), charToSpawn);
        }
    }

    private void LoadRules()
    {
        _rules = new Dictionary<char, CharacterRule>();
        var asset = Resources.Load("rules_for_letters") as TextAsset;
        var rules = JsonHelper.GetJsonArray<CharacterRule>(asset.text);

        for (int i = 0; i < rules.Length; i++)
        {
            _rules.Add(rules[i].Character[0], rules[i]);
        }
    }

    private void LoadWords()
    {
        var asset = Resources.Load("rus_words") as TextAsset;

        _words = new HashSet<string>(Regex.Split(asset.text, "\r\n"));
    }

    private void LoadSprites()
    {
        _charactersSprites = new Dictionary<char, Sprite>();
        _charactersActiveSprites = new Dictionary<char, Sprite>();

        Sprite[] spritesActive = Resources.LoadAll<Sprite>("Characters-active");
        Sprite[] sprites = Resources.LoadAll<Sprite>("Characters");

        foreach (var sprite in spritesActive)
        {
            _charactersActiveSprites.Add(sprite.name.ToCharArray()[sprite.name.Length - 1], sprite);
        }

        foreach (var sprite in sprites)
        {
            _charactersSprites.Add(sprite.name.ToCharArray()[sprite.name.Length - 1], sprite);
        }

    }

    private void PrepareGame()
    {
        _collectedScoreInRound = 0;
        _ticksOfRound = 0;
        _selectedCells = new List<CellData>();

        char[] characters = GenerateCharactersArray();

        _field = new CellData[ROWS, COLUMNS];
        _cells = new List<CellData>();

        foreach (char c in characters)
        {
            var cellData = ObjectPoolManager.Instance.Get(CellPrefab) as CellData;
            cellData.Setup(c, _charactersSprites[c], _charactersActiveSprites[c]);
            cellData.transform.SetParent(Field.transform);
            _cells.Add(cellData);
        }

        Shaffle();

        for (int i = 0; i < ROWS_STARTED; i++)
        {
            for (int j = 0; j < COLUMNS; j++)
            {
                _cells[i * COLUMNS + j].transform.localPosition = new Vector3(j, -i - (ROWS - ROWS_STARTED), 0);
                _field[i + (ROWS - ROWS_STARTED), j] = _cells[i * COLUMNS + j];
            }
        }

        StateController.Instance.GameState();
        WordChangedEvent.Invoke("");
    }

    private char[] GenerateCharactersArray()
    {
        Dictionary<char, int> usedChars = new Dictionary<char, int>();
        char[] characters = new char[ROWS_STARTED * COLUMNS];

        int characterIndex = 0;

        foreach (var pair in _rules)
        {
            if (pair.Value.NormalRange.Min > 0)
            {
                for (int i = 0; i < pair.Value.NormalRange.Min; i++)
                {
                    characters[characterIndex] = pair.Key;
                    characterIndex++;

                    if (!usedChars.ContainsKey(pair.Key))
                    {
                        usedChars.Add(pair.Key, 0);
                    }
                    usedChars[pair.Key]++;

                    if (characterIndex - 1 >= ROWS_STARTED * COLUMNS)
                    {
                        return characters;
                    }
                }
            }
        }

        for (int i = 0; i < characterIndex; i++)
        {
            if (characterIndex >= ROWS_STARTED * COLUMNS)
            {
                return characters;
            }

            char randomChar = _rules.ElementAt(Random.Range(0, _rules.Count)).Key;
            if (usedChars.ContainsKey(randomChar) && usedChars[randomChar] >= _rules[randomChar].NormalRange.Max)
            {
                continue;
            }

            characters[characterIndex] = randomChar;
            characterIndex++;

            if (!usedChars.ContainsKey(randomChar))
            {
                usedChars.Add(randomChar, 0);
            }
            usedChars[randomChar]++;
        }
        return characters;
    }

    private char GenerateCharacter()
    {
        var usedChars = GenerateUsedChars();

        var keys = usedChars.Keys.ToArray();
        foreach (var key in keys)
        {
            if (usedChars[key] >= _rules[key].AdditionalFallRange.Max)
            {
                usedChars.Remove(key);
            }
        }

        return usedChars.Keys.ElementAt(Random.Range(0, usedChars.Count));
    }

    private Dictionary<char, int> GenerateUsedChars()
    {
        var usedChars = new Dictionary<char, int>();

        foreach (var cell in _cells)
        {
            if (!usedChars.ContainsKey(cell.GetChar()))
            {
                usedChars.Add(cell.GetChar(), 0);
            }
            else
            {
                usedChars[cell.GetChar()]++;
            }
        }
        return usedChars;
    }

    private void Shaffle()
    {
        System.Random rng = new System.Random();
        int n = _cells.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            CellData value = _cells[k];
            _cells[k] = _cells[n];
            _cells[n] = value;
        }
    }

    private void OnSelectedCell(CellData cellData)
    {
        _selectedCells.Add(cellData);
        WordChangedEvent.Invoke(GetSelectedWord());
    }

    private void OnUnselectedCell(CellData cellData)
    {
        _selectedCells.Remove(cellData);
        WordChangedEvent.Invoke(GetSelectedWord());
    }

    private string GetSelectedWord()
    {
        string word = "";
        foreach (var cell in _selectedCells)
        {
            word += cell.GetChar().ToString();
        }
        return word;
    }

    private void StartGame()
    {
        _roundTimer = Observable.Interval(TimeSpan.FromMilliseconds(TIMER_TICK_INTERVAL))
            .Subscribe(_ => { OnTimerTick(); });

        _characterFallTimer = Observable.Interval(TimeSpan.FromMilliseconds(ADDITIONAL_CHARACTERS_TIMER_INTERVAL))
            .Subscribe(_ => { SpawnAdditionalCharacter(); });

        Field.SetActive(true);
    }

    private void OnTimerTick()
    {
        _ticksOfRound++;

        TimerCounterChangedEvent.Invoke(ROUND_DURATION - _ticksOfRound);

        if (_ticksOfRound >= ROUND_DURATION)
        {
            _roundTimer.Dispose();
            _characterFallTimer.Dispose();

            if (_collectedScoreInRound > _maxCollectedScore)
            {
                _maxCollectedScore = _collectedScoreInRound;
            }
            StateController.Instance.EndRoundState(_collectedScoreInRound, _maxCollectedScore);
        }
    }

    private void SpawnAdditionalCharacter()
    {
        if (_cells.Count == ROWS * COLUMNS)
        {
            return;
        }

        SpawnCharacter(GetEmptyCellPosition(), GenerateCharacter());
    }

    private void SpawnCharacter(Vector2 position, char c)
    {
        var character = ObjectPoolManager.Instance.Get(CellPrefab) as CellData;
        character.Setup(c, _charactersSprites[c], _charactersActiveSprites[c]);
        character.transform.SetParent(Field.transform);
        character.transform.localPosition = new Vector2(position.x, 2);

        _cells.Add(character);
        _field[(int)-position.y, (int)position.x] = character;

        DisplacementController.Instance.Move(character.transform, character.transform.localPosition, new Vector2(position.x, position.y), FALL_SPEED);
    }

    private Vector2 GetEmptyCellPosition()
    {
        int higgestEmptyRowIndex = ROWS - 1;

        for (int i = ROWS - 1; i >= 0; i--)
        {
            for (int j = 0; j < COLUMNS; j++)
            {
                if (_field[i, j] == null && higgestEmptyRowIndex > i)
                {
                    higgestEmptyRowIndex = i;
                    goto endSycle;
                }
            }
        }
        endSycle:

        List<int> columnIndexes = new List<int>();

        for (int i = 0; i < COLUMNS; i++)
        {
            if (_field[higgestEmptyRowIndex, i] == null)
            {
                columnIndexes.Add(i);
            }
        }

        return new Vector2(columnIndexes[Random.Range(0, columnIndexes.Count)], -higgestEmptyRowIndex);
    }

    private void ClearField()
    {
        var children = Field.GetComponentsInChildren<CellData>();
        foreach (CellData child in children)
        {
            ObjectPoolManager.Instance.Push(CellPrefab, child);
        }

        DisplacementController.Instance.StopAll();

        _field = null;
        _cells = null;

        Field.SetActive(false);
    }

    private void MoveCells()
    {
        for (int j = 0; j < COLUMNS; j++)
        {
            int emptyRowIndex = -1;
            for (int i = ROWS - 1; i >= 0; i--)
            {
                if (_field[i, j] == null)
                {
                    if (emptyRowIndex == -1)
                    {
                        emptyRowIndex = i;
                    }
                    continue;
                }

                if (emptyRowIndex == -1)
                {
                    continue;
                }

                var cell = _field[i, j];
                _field[emptyRowIndex, j] = cell;
                _field[i, j] = null;

                DisplacementController.Instance.Move(cell.transform, cell.transform.localPosition, new Vector2(cell.transform.localPosition.x, -emptyRowIndex), FALL_SPEED);
                i = Mathf.Clamp(emptyRowIndex, 0, ROWS - 1);
                emptyRowIndex = -1;
            }
        }
    }

    [Serializable]
    private struct Range
    {
        public int Min;
        public int Max;

        public Range(int min, int max)
        {
            Min = min;
            Max = max;
        }
    }

    [Serializable]
    private struct CharacterRule
    {
        public string Character;
        public Range NormalRange;
        public Range AdditionalFallRange;
        public bool IsVowel;

        public CharacterRule(string character, Range normalRange, Range additionalFallRange, bool isVowel)
        {
            Character = character;
            NormalRange = normalRange;
            AdditionalFallRange = additionalFallRange;
            IsVowel = isVowel;
        }
    }
}
