using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class LeverArmController : MonoBehaviour, IDragHandler, IEndDragHandler
{

    public UnityEvent OnPullDone;

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
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isPulledBack || _isLocked)
        {
            return;
        }

        Vector2 pointer = new Vector2(_startPosition.x, Input.mousePosition.y);
        if (Vector2.Distance(_startPosition, pointer) > DistanceToPull)
        {
            OnPullDone.Invoke();
            _isLocked = true;

            DisplacementController.Instance.Move(transform, transform.localPosition, new Vector2(transform.localPosition.x, transform.localPosition.y - 500), .0015f,
                () => gameObject.SetActive(false),
                coveredDistance => _canvasGroup.alpha = 1 - coveredDistance);
            return;
        }

        if (Input.mousePosition.y >= _startPosition.y)
        {
            return;
        }

        transform.position = pointer;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isPulledBack || _isLocked)
        {
            return;
        }
        _isPulledBack = true;
        DisplacementController.Instance.Move(transform, transform.localPosition, _startPositionLocal, .001f,
            () => _isPulledBack = false);
    }

}
