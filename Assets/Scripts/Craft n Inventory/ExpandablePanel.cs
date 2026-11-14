using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ExpandablePanel : MonoBehaviour
{
    [SerializeField] Button _headerButton; //clicking the header opens/closes the panel
    [SerializeField] GameObject _content; //the part that gets hidden
    [SerializeField] bool _startOpen = true;

    [Header("Arrow")]
    [SerializeField] Image _arrowImage;  //the arrow icon in the header
    [SerializeField] Sprite _openSprite;//arrow pointing down
    [SerializeField] Sprite _closedSprite;//arrow point right 

    [Header("Optional key")]
    [SerializeField] bool _useKey = false; //missions use a key because the cursor is locked while playing
    [SerializeField] Key _toggleKey = Key.M;

    private bool _isOpen = false;

    private void Start()
    {
        if(_headerButton != null)
        {
            _headerButton.onClick.AddListener(Toggle);
        }
        SetOpen(_startOpen);
    }
    private void Update()
    {
        if (!_useKey || Keyboard.current == null) return;

        if (Keyboard.current[_toggleKey].wasPressedThisFrame)
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        SetOpen(!_isOpen);
    }

    public void SetOpen(bool open)
    {
        _isOpen = open;

        if(_content!= null)
        {
            _content.SetActive(open);
        }

        if(_arrowImage!= null)
        {
            _arrowImage.sprite = open ? _openSprite : _closedSprite;
        }
    }
}
