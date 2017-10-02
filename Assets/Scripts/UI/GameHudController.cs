using UnityEngine;
using UnityEngine.UI;

public class GameHudController : MonoBehaviour
{

    public Text TimerCounter;
    public Text CollectedCounter;
    public Text Word;

    void Awake()
    {
        GameController.Instance.TimerCounterChangedEvent.AddListener(OnTimerCounterChanged);
        GameController.Instance.CollectedScoreChangedEvent.AddListener(OnCollectedCounterChanged);
        GameController.Instance.WordChangedEvent.AddListener(OnWordChanged);

        StateController.Instance.OnGameStart.AddListener(() => gameObject.SetActive(true));
        StateController.Instance.OnGameEnd.AddListener(() => gameObject.SetActive(false));

        gameObject.SetActive(false);
    }

    public void ApplyWord()
    {
        GameController.Instance.ApplyWord();
    }

    public void ClearSelected()
    {
        GameController.Instance.UnselectAll();
    }

    public void MoreCharacters()
    {
        Debug.Log("GameHudController MoreCharacters");
        GameController.Instance.SpawnMoreCharacters();
    }

    private void OnTimerCounterChanged(int value)
    {
        TimerCounter.text = value.ToString();
    }

    private void OnCollectedCounterChanged(int value)
    {
        CollectedCounter.text = value.ToString();
    }

    private void OnWordChanged(string word)
    {
        Word.text = word;
    }
}
