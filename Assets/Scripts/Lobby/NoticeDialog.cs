using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Localization.Components;

public class NoticeDialog : MonoBehaviour
{
    [SerializeField] private GameObject _notice;
    [SerializeField] private GameObject _noticeBG;
    [SerializeField] private Text _text;
    [SerializeField] private Image[] _buttons;
    [SerializeField] private Text[] _buttonText;

    private readonly float[] _buttonPosX =
    {
        -193f,
        193f
    };

    private LocalizeStringEvent _taskTextEvent;
    private LocalizeStringEvent _leftButtonTextEvent;
    private LocalizeStringEvent _rightButtonTextEvent;

    private bool _isShowed;

    private INoticeAction _action;

    private enum Buttons
    {
        Left,
        Right
    }

    public enum Message
    {
        ConnectionError,
        ItShop,
        Wardrobe,
        Rating,
        StartTutorial,
        EndTutorial,
        ServerFull,
        EmptyQueue,
        BackToLobby,
        InvalidNickname
    }

    public Message CurrentMessage { get; private set; }

    private void Start()
    {
        _taskTextEvent = _text.GetComponent<LocalizeStringEvent>();
        _leftButtonTextEvent = _buttonText[(int)Buttons.Left].GetComponent<LocalizeStringEvent>();
        _rightButtonTextEvent = _buttonText[(int)Buttons.Right].GetComponent<LocalizeStringEvent>();
    }

    private readonly string[] _noticeKey =
    {
        "Notice_WaitingForNetwork",
        "Notice_Shop",
        "Notice_Wardrobe",
        "Notice_Rating",
        "Notice_StartTutorial",
        "Notice_EndTutorial",
        "Notice_ServerFull",
        "Notice_EmptyQueue",
        "Notice_BackToLobby",
        "Notice_InvalidNickname"
    };

    public void ShowDialog(Message message, INoticeAction action = null, string leftButtonTextKey = "Notice_Close", string rightButtonTextKey = null)
    {
        if(!_isShowed)
        {
            _noticeBG.SetActive(true);
            _notice.SetActive(true);
            _isShowed = true;
            _notice.transform.DOScale(1, 0.3f).SetEase(Ease.OutExpo);
        }

        _leftButtonTextEvent.SetEntry(leftButtonTextKey);

        if(rightButtonTextKey != null) 
        {
            _buttons[(int)Buttons.Right].gameObject.SetActive(true);

            _buttons[(int)Buttons.Left].rectTransform.anchoredPosition = 
                new Vector2(_buttonPosX[(int)Buttons.Left], -151.52f);

            _buttons[(int)Buttons.Right].rectTransform.anchoredPosition = 
                new Vector2(_buttonPosX[(int)Buttons.Right], -151.52f);

            _rightButtonTextEvent.SetEntry(rightButtonTextKey);
        }
        else
        {
            _buttons[(int)Buttons.Right].gameObject.SetActive(false);
            _buttons[(int)Buttons.Left].rectTransform.anchoredPosition = new Vector2(0, -151.52f);
        }
        SetAction(action);

        _taskTextEvent.SetEntry(_noticeKey[(int)message]);
    }

    public void HideDialog()
    {
        if (!_isShowed) return;
        _noticeBG.SetActive(false);
        _notice.transform.DOScale(0.1f, 0.1f).OnComplete(() =>
        {
            _action = null;
            _isShowed = false;
            _notice.SetActive(false);
        });
        EventBus.OnPlayerClickUI?.Invoke(1);
    }

    public void OnClickButtonEvent(int button)
    {
        if (!_isShowed) return;
        if(_action != null) _action.ActionOnClickNotice(button);
        else HideDialog();
    }

    private void SetAction(INoticeAction action) => _action = action;
}