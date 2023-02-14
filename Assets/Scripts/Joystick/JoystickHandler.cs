using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class JoystickHandler : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image _joystickBG;
    [SerializeField] private Image _joystick;
    [SerializeField] private Image _joystickArea;

    private Vector2 _joystickBGStartPosition;

    protected Vector2 InputVector;

    [SerializeField] private Color _inactiveJoystickColor;
    [SerializeField] private Color _activeJoystickColor;

    private bool _joystickIsActive;

    private void Start()
    {
        ClickEffect();
        _joystickBGStartPosition = _joystickBG.rectTransform.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_joystickBG.rectTransform, eventData.position, null, out Vector2 joystickPosition))
        {
            joystickPosition.x = (joystickPosition.x * 2 / _joystickBG.rectTransform.sizeDelta.x);
            joystickPosition.y = (joystickPosition.y * 2 / _joystickBG.rectTransform.sizeDelta.y);

            InputVector = joystickPosition;
            InputVector = (InputVector.magnitude > 1 ? InputVector.normalized : InputVector);

            _joystick.rectTransform.anchoredPosition = new Vector2(InputVector.x * (_joystickBG.rectTransform.sizeDelta.x / 2), InputVector.y * (_joystickBG.rectTransform.sizeDelta.y / 2));

            OnPlayerMouseDrag();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPlayerMouseDowm();

        ClickEffect();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_joystickArea.rectTransform, eventData.position, null, out Vector2 joystickBGPosition))
        {
            _joystickBG.rectTransform.anchoredPosition = joystickBGPosition;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        OnPlayerMouseUp();

        ClickEffect();
        _joystickBG.rectTransform.anchoredPosition = _joystickBGStartPosition;
        InputVector = Vector2.zero;
        _joystick.rectTransform.anchoredPosition = Vector2.zero;
        
    }

    protected virtual void OnPlayerMouseUp()
    {

    }

    protected virtual void OnPlayerMouseDowm()
    {

    }

    protected virtual void OnPlayerMouseDrag()
    {

    }

    private void ClickEffect()
    {
        if (!_joystickIsActive)
        {
            _joystick.color = _activeJoystickColor;
            _joystickIsActive = true;
        }
        else
        {
            _joystick.color = _inactiveJoystickColor;
            _joystickIsActive = false;
        }
    }
}
