using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LeverArmController : MonoBehaviour, IDragHandler, IEndDragHandler
{

    public UnityEvent OnPullDone;

    [Tooltip("Дистанция на которую можно вытянуть объект в процентах относительно высоты экрана")]
    public float DistanceToPull;

    private Vector3 _startPosition;
    private Vector3 _startPositionLocal;
    private bool _isPulledBack;
    private bool _isLocked;

    private CanvasGroup _canvasGroup;

    void Start()
    {
        _startPosition = transform.position;
        _startPositionLocal = transform.localPosition;

        _canvasGroup = GetComponent<CanvasGroup>();

        StateController.Instance.OnPrepareRoundStart.AddListener(Reset);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("LeverArmController OnDrag");
        if (_isPulledBack || _isLocked)
        {
            return;
        }

        Vector2 pointer = new Vector2(_startPosition.x, Input.mousePosition.y);
        float coveredDistInPercent = Vector2.Distance(_startPosition, pointer) * 100 / Screen.height;
        if (coveredDistInPercent > DistanceToPull && GameController.Instance.GetEmptyCellPosition() != -Vector2.one)
        {

            Debug.Log("LeverArmController OnPullDone.Invoke");
            OnPullDone.Invoke();
            _isLocked = true;

            DisplacementController.Instance.Move(transform, transform.localPosition, new Vector2(transform.localPosition.x, -Screen.height / 3), .0012f,
                () => gameObject.SetActive(false),
                coveredDistance => _canvasGroup.alpha = 1 - coveredDistance);
            return;
        }

        if (_startPosition.y < Input.mousePosition.y || coveredDistInPercent > DistanceToPull)
        {
            return;
        }

        transform.position = pointer;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("LeverArmController OnEndDrag");
        if (_isPulledBack || _isLocked)
        {
            return;
        }
        PullBack();
    }

    private void PullBack()
    {
        _isPulledBack = true;
        DisplacementController.Instance.Move(transform, transform.localPosition, _startPositionLocal, .001f,
            () => _isPulledBack = false);
    }

    private void Reset()
    {
        gameObject.SetActive(true);
        transform.localPosition = _startPositionLocal;
        _canvasGroup.alpha = 1;
        _isLocked = false;
        _isPulledBack = false;
    }
}
