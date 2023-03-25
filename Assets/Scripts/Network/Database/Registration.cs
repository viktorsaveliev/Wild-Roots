using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;
using DG.Tweening;

public class Registration : MonoBehaviour
{
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TMP_InputField _repeatPassword;
    [SerializeField] private GameObject _regPanel;

    [SerializeField] private TMP_Text _emailError;
    [SerializeField] private TMP_Text _passwordError;

    private bool _isMobileDevice;

    public void Show(bool isMobileDevice)
    {
        _isMobileDevice = isMobileDevice;

        _regPanel.transform.localPosition = new Vector2(-2000f, 0);
        _regPanel.SetActive(true);
        _regPanel.transform.DOLocalMoveX(0f, 0.3f);

        EventBus.OnPlayerClickUI?.Invoke(2);
    }

    public void Show()
    {
        _regPanel.transform.localPosition = new Vector2(-2000f, 0);
        _regPanel.SetActive(true);
        _regPanel.transform.DOLocalMoveX(0f, 0.3f);

        EventBus.OnPlayerClickUI?.Invoke(2);
    }

    public void Hide()
    {
        _regPanel.transform.DOScale(0.1f, 0.2f).OnComplete(() =>
        {
            _regPanel.SetActive(false);
            _regPanel.transform.localScale = new Vector2(0.8f, 0.8f);
        });
    }

    public void ShowKeyboard(int inputIndex)
    {
        //if (_isMobileDevice == false) return;

        if(inputIndex == 0) Keyboard.Show(_email);
        else if(inputIndex == 1) Keyboard.Show(_password);
        else Keyboard.Show(_repeatPassword);
    }

    public void RegisterButton()
    {
        ResetErrorText();

        if (_email.text == string.Empty || _password.text == string.Empty || _repeatPassword.text == string.Empty)
        {
            _emailError.text = "Fill in all the fields";
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        if(_password.text != _repeatPassword.text)
        {
            _passwordError.text = "Password mismatch";
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        if(_password.text.Length < 6)
        {
            _passwordError.text = "Password length must be at least 6 characters";
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        if (_email.text.Contains('@') == false)
        {
            _emailError.text = "Invalid e-mail format";
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }
        StartCoroutine(CheckIfUserExists(_email.text));
        EventBus.OnPlayerClickUI?.Invoke(0);
    }

    private IEnumerator CheckIfUserExists(string email)
    {
        WWWForm form = new();
        form.AddField("email", email);
        using UnityWebRequest www = UnityWebRequest.Post("https://www.wildroots.fun/public/get_mail.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
            yield break;
        }

        print(www.downloadHandler.text);

        bool userExists = bool.Parse(www.downloadHandler.text);
        if (userExists)
        {
            _emailError.text = "This account is already registered";
            EventBus.OnPlayerClickUI?.Invoke(3);
        }
        else
        {
            StartCoroutine(RegisterUser(_email.text, _password.text));
        }
    }

    private IEnumerator RegisterUser(string email, string password)
    {
        WWWForm form = new();
        form.AddField("email", email);
        form.AddField("password", password);
        form.AddField("regIP", "Web-server");

        StringBus stringBus = new();
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "register.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            LoadData.Instance.GetUserID(email, password, false);
            Hide();
        }
        else
        {
            EventBus.OnPlayerClickUI?.Invoke(3);
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
    }

    /*public string GetUserIP()
    {
        string externalIP = new WebClient().DownloadString("http://checkip.dyndns.org/");
        externalIP = (new System.Text.RegularExpressions.Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}")).Matches(externalIP)[0].ToString();
        return externalIP;
    }*/

    private void ResetErrorText()
    {
        _emailError.text = string.Empty;
        _passwordError.text = string.Empty;
    }
}