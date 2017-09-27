using Lib.Util;
using UnityEngine;

public class CellData : MonoBehaviour, IPoolItem
{

    public bool InUse { get; set; }

    public SpriteRenderer Render;

    private char _character;
    private Sprite _sprite;
    private Sprite _activeSprite;
    private bool _isSelected;

    public void Setup(char character, Sprite sprite, Sprite activeSprite)
    {
        _character = character;
        _sprite = sprite;
        _activeSprite = activeSprite;
        Render.sprite = sprite;
    }

    public void Select()
    {
        _isSelected = true;
        Render.sprite = _activeSprite;
        GameController.Instance.SelectCell.Invoke(this);
    }

    public char GetChar()
    {
        return _character;
    }

    public void Unselect(bool invoke = true)
    {
        _isSelected = false;
        Render.sprite = _sprite;
        if (invoke)
        {
            GameController.Instance.UnselectCell.Invoke(this);
        }
    }

    public void OnRemove()
    {
        gameObject.SetActive(false);
        transform.parent = null;
        _sprite = null;
        _activeSprite = null;
        _character = ' ';
        _isSelected = false;
    }

    public void OnCreate()
    {
        gameObject.SetActive(true);
    }

    private void OnMouseDown()
    {
        if (_isSelected)
        {
            Unselect();
        }
        else
        {
            Select();
        }
    }

}
