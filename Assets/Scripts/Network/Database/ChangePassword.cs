using UnityEngine;
using TMPro;
using DG.Tweening;

public class ChangePassword : MonoBehaviour
{
    [SerializeField] private TMP_InputField _newPassword;
    [SerializeField] private TMP_InputField _repeatNewPassword;
    [SerializeField] private TMP_Text _errorText;

    [SerializeField] private GameObject _changePassPanel;
    [SerializeField] private Authorization _loginPanel;

    public void ConfirmButton()
    {
        ResetErrorText();

        if (_newPassword.text == string.Empty || _repeatNewPassword.text == string.Empty)
        {
            _errorText.text = "Fill in all the fields";
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        if (_newPassword.text != _repeatNewPassword.text)
        {
            _errorText.text = "Password mismatch";
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        if (_newPassword.text.Length < 6)
        {
            _errorText.text = "Password length must be at least 6 characters";
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        SaveData.Instance.SaveNewPassword(_newPassword.text);
        Hide();
        _loginPanel.Show();
        EventBus.OnPlayerClickUI?.Invoke(0);
    }

    private void ResetErrorText()
    {
        _errorText.text = string.Empty;
    }

    public void Show()
    {
        _changePassPanel.transform.localPosition = new Vector2(-2000f, 0);
        _changePassPanel.SetActive(true);
        _changePassPanel.transform.DOLocalMoveX(0f, 0.3f);

        EventBus.OnPlayerClickUI?.Invoke(2);
    }

    public void ShowKeyboard(int inputID)
    {
        Keyboard.Show(inputID == 0 ? _newPassword : _repeatNewPassword);
    }

    public void Hide()
    {
        _changePassPanel.transform.DOScale(0.1f, 0.2f).OnComplete(() =>
        {
            _changePassPanel.SetActive(false);
            _changePassPanel.transform.localScale = new Vector2(0.8f, 0.8f);
        });
    }
}
