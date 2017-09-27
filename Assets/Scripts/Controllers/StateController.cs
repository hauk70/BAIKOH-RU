using Lib.Util;
using UnityEngine;
using Event = Lib.Util.Event;

public class StateController : MonoSingleton<StateController>
{

    public enum State { MainMenu, PrepareRound, Game, EndRound }

    public Event OnMainMenuStart = new Event();
    public Event OnMainMenuEnd = new Event();
    public Event OnPrepareRoundStart = new Event();
    public Event OnPrepareRoundEnd = new Event();
    public Event OnGameStart = new Event();
    public Event OnGameEnd = new Event();
    public Event<int, int> OnEndRoundStart = new Event<int, int>();
    public Event OnEndRoundEnd = new Event();

    [SerializeField]
    private State _currentState = State.MainMenu;

    public void MainMenuState()
    {
        SetState(State.MainMenu);
        OnMainMenuStart.Invoke();
    }


    public void PrepareRoundState()
    {
        SetState(State.PrepareRound);
        OnPrepareRoundStart.Invoke();
    }

    public void GameState()
    {
        SetState(State.Game);
        OnGameStart.Invoke();
    }


    public void EndRoundState(int collected, int record)
    {
        SetState(State.EndRound);
        OnEndRoundStart.Invoke(collected, record);
    }

    protected void SetState(State newState)
    {
        switch (_currentState)
        {
            case State.MainMenu:
                OnMainMenuEnd.Invoke();
                break;
            case State.PrepareRound:
                OnPrepareRoundEnd.Invoke();
                break;
            case State.Game:
                OnGameEnd.Invoke();
                break;
            case State.EndRound:
                OnEndRoundEnd.Invoke();
                break;
        }

        _currentState = newState;
    }

}
