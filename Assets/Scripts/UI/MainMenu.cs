using UnityEngine;

public class MainMenu : MonoBehaviour
{

    public void Awake()
    {
        StateController.Instance.OnMainMenuEnd.AddListener(() => { gameObject.SetActive(false); });
    }

    public void StartGame()
    {
        StateController.Instance.PrepareRoundState();
    }

}
