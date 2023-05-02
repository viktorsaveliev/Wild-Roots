using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Localization.Components;
using System.Collections;

public class NoticeDialog : MonoBehaviour
{
    [Header("Notice")]
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

    [Header("Simple Notice")]
    [SerializeField] private GameObject _simpleNotice;
    [SerializeField] private Text _simpleNoticeStatus;
    [SerializeField] private Text _simpleNoticeText;
    [SerializeField] private Image _icon;
    [SerializeField] private Sprite[] _iconSprite;

    private LocalizeStringEvent _simpleTextEvent;
    private LocalizeStringEvent _simpleStatusEvent;
    private Coroutine _timerSimpleNotice;

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
        InvalidNickname,
        PasswordChanged,
        PincodeSended,

        Simple_Success,
        Simple_UnSuccess,
        Simple_Purchase,
        Simple_NotMoney,
        Simple_YouHaveThisSkin,
        Simple_NeedLogin,
        Simple_CopyToClipboard
    }

    public Message CurrentMessage { get; private set; }

    private void Start()
    {
        _taskTextEvent = _text.GetComponent<LocalizeStringEvent>();
        _leftButtonTextEvent = _buttonText[(int)Buttons.Left].GetComponent<LocalizeStringEvent>();
        _rightButtonTextEvent = _buttonText[(int)Buttons.Right].GetComponent<LocalizeStringEvent>();

        _simpleTextEvent = _simpleNoticeText.GetComponent<LocalizeStringEvent>();
        _simpleStatusEvent = _simpleNoticeStatus.GetComponent<LocalizeStringEvent>();
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
        "Notice_InvalidNickname",
        "Notice_PasswordChanged",
        "Notice_PincodeSended",
        
        "SimpleNotice_Success",
        "SimpleNotice_UnSuccess",
        "SimpleNotice_Purchase",
        "SimpleNotice_NotEnoughtMoney",
        "SimpleNotice_YouHaveThisSkin",
        "SimpleNotice_NeedLogin",
        "SimpleNotice_CopyToClipboard"
    };

    public void Simple(Message message, bool success)
    {
        _simpleNotice.SetActive(true);
        _simpleNotice.transform.DOMoveY(50f, 0.5f);

        _simpleTextEvent.SetEntry(_noticeKey[(int)message]);

        if(success)
        {
            _icon.sprite = _iconSprite[0];
            _simpleStatusEvent.SetEntry("SimpleNotice_Success");
        }
        else
        {
            _icon.sprite = _iconSprite[1];
            _simpleStatusEvent.SetEntry("SimpleNotice_UnSuccess");
        }

        if(_timerSimpleNotice != null)
        {
            StopCoroutine(_timerSimpleNotice);
            _timerSimpleNotice = null;
        }
        _timerSimpleNotice = StartCoroutine(SimpleNoticeTimer());
    }

    public void HideSimple()
    {
        _timerSimpleNotice = null;
        _simpleNotice.transform.DOMoveY(-200f, 0.5f).OnComplete(() =>
        {
            _simpleNotice.SetActive(false);
        });
    }

    private IEnumerator SimpleNoticeTimer()
    {
        yield return new WaitForSeconds(2f);
        HideSimple();
    }

    public void Dialog(Message message, INoticeAction action = null, string leftButtonTextKey = "Notice_Close", string rightButtonTextKey = null)
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

    public void Dialog(string message, INoticeAction action = null, string leftButtonTextKey = "Notice_Close")
    {
        if (!_isShowed)
        {
            _noticeBG.SetActive(true);
            _notice.SetActive(true);
            _isShowed = true;
            _notice.transform.DOScale(1, 0.3f).SetEase(Ease.OutExpo);
        }

        _leftButtonTextEvent.SetEntry(leftButtonTextKey);

        
        _buttons[(int)Buttons.Right].gameObject.SetActive(false);
        _buttons[(int)Buttons.Left].rectTransform.anchoredPosition = new Vector2(0, -151.52f);

        SetAction(action);

        _text.text = message;
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