using UnityEngine;
using UnityEngine.UI;

public class EndRoundResultPanel : MonoBehaviour
{

    public Text CollectedValue;
    public Text RecordValue;

    void Start()
    {
        StateController.Instance.OnEndRoundStart.AddListener((collected, record) =>
        {
            gameObject.SetActive(true);
            CollectedValue.text = collected.ToString();
            RecordValue.text = record.ToString();
        });

        StateController.Instance.OnEndRoundEnd.AddListener(() => gameObject.SetActive(false));

        gameObject.SetActive(false);
    }

    public void TryAgainHandler()
    {
        StateController.Instance.PrepareRoundState();
    }

}
