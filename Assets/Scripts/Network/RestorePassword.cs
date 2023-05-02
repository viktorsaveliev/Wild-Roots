using CrazyGames;
using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class RestorePassword : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private TMP_Text _headEmail;
    [SerializeField] private TMP_Text _header;
    [SerializeField] private TMP_Text _errorText;

    [SerializeField] private GameObject _resetPass;
    [SerializeField] private ChangePassword _changePass;

    private bool _pincodeSended;
    private string _savedEmail;

    public void SendPincode()
    {
        ResetErrorText();

        EventBus.OnPlayerClickUI?.Invoke(0);

        if (_inputField.text == string.Empty)
        {
            _errorText.text = "Fill in all the fields";
            return;
        }
        if (_pincodeSended == false)
        {
            if (_inputField.text.Contains('@') == false)
            {
                _errorText.text = "Invalid e-mail format";
                return;
            }
            StartCoroutine(GetPasswordAndSendToEmail(_inputField.text));
        }
        else
        {
            if (_inputField.text.Length != 4 || _savedEmail == string.Empty)
            {
                _errorText.text = "Invalid PIN-code format";
                return;
            }
            StartCoroutine(CheckPincode(int.Parse(_inputField.text)));
        }
    }

    private IEnumerator CheckPincode(int pincode)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("email", _savedEmail);
        form.AddField("pincode", pincode);
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "check_pincode.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = bool.Parse(www.downloadHandler.text);
            if (success)
            {
                _changePass.Show();
                Hide();
            }
            else
            {
                _errorText.text = "Wrong PIN-code";
            }
        }
        else
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
    }

    private IEnumerator GetPasswordAndSendToEmail(string email)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("email", email);
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "restore_password.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = bool.Parse(www.downloadHandler.text);
            if(success)
            {
                _savedEmail = email;
                PlayerPrefs.SetString("email", email);

                _inputField.text = string.Empty;
                _inputField.placeholder.GetComponent<TMP_Text>().text = "enter your pincode";
                _header.text = "New password";
                _headEmail.text = "PIN-code:";

                _pincodeSended = true;
                Notice.Dialog(NoticeDialog.Message.PincodeSended);
            }
            else
            {
                Notice.Dialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
    }

    private void ResetErrorText()
    {
        _errorText.text = string.Empty;
    }

    public void ShowKeyboard()
    {
        Keyboard.Show(_inputField);
    }

    public void Show()
    {
        _resetPass.transform.localPosition = new Vector2(-2000f, 0);
        _resetPass.SetActive(true);
        _resetPass.transform.DOLocalMoveX(0f, 0.3f);

        EventBus.OnPlayerClickUI?.Invoke(2);
    }

    public void Hide()
    {
        _resetPass.transform.DOScale(0.1f, 0.2f).OnComplete(() =>
        {
            _resetPass.SetActive(false);
            _resetPass.transform.localScale = new Vector2(0.8f, 0.8f);
        });
    }
}