using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyboardHandler : MonoBehaviour
{
    [SerializeField] private Button _uppercaseButton;
    [SerializeField] private GameObject _keyboardObject;
    [SerializeField] private GameObject _keyboardBG;

    private TMP_InputField _currentInputField;
    private bool _uppercasePressed = false;
    private bool _actionAfterHide = false;
    private Image _upperImageButton;

    private void Start()
    {
        _upperImageButton = _uppercaseButton.GetComponent<Image>();

        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortraitUpsideDown = false;
    }

    public void Show(TMP_InputField targetInputField, bool actionAfterHide = false)
    {
        StringBus stringBus = new();
        if (PlayerPrefs.GetInt(stringBus.PlayerDevice) == 1) return;

        _actionAfterHide = actionAfterHide;
        _currentInputField = targetInputField;
        _keyboardBG.SetActive(true);
        _keyboardObject.SetActive(true);
    }

    public void Hide()
    {
        if(_actionAfterHide)
        {
            EventBus.OnPlayerTryChangeNickname?.Invoke(_currentInputField.text);
        }

        _actionAfterHide = false;
        _keyboardBG.SetActive(false);
        _keyboardObject.SetActive(false);
    }

    public void UppercaseButtonClick()
    {
        if (_uppercasePressed)
        {
            _upperImageButton.color = new Color(0.1132075f, 0.1132075f, 0.1132075f);
            _uppercasePressed = false;
        }
        else
        {
            _upperImageButton.color = new Color(0.2169811f, 0.2169811f, 0.2169811f);
            _uppercasePressed = true;
        }
        EventBus.OnPlayerClickUI?.Invoke(1);
    }

    public enum ButtonType
    {
        Symbol,
        Delete,
        Enter,
        Empty,
        ESC
    }

    public void OnKeyboardButtonClick(int type)
    {
        string buttonPressed = EventSystem.current.currentSelectedGameObject.name;

        if (_uppercasePressed)
        {
            buttonPressed = buttonPressed.ToUpper();
        }
        else
        {
            buttonPressed = buttonPressed.ToLower();
        }
        
        switch(type)
        {
            case (int)ButtonType.Delete:
                if (_currentInputField.text.Length == 0) return;
                _currentInputField.text = _currentInputField.text.Remove(_currentInputField.text.Length - 1);
                break;

            case (int)ButtonType.Symbol:
                _currentInputField.text += buttonPressed;
                break;

            case (int)ButtonType.ESC:
            case (int)ButtonType.Enter:
                Hide();
                break;
        }

        EventBus.OnPlayerClickUI?.Invoke(1);
    }
}
