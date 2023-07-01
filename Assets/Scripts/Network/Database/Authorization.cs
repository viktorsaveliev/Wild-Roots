using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class Authorization : MonoBehaviour
{
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private Toggle _rememberMe;
    [SerializeField] private GameObject _authPanel;

    [SerializeField] private TMP_Text _emailError;
    [SerializeField] private TMP_Text _passwordError;

    private bool _isLoginButtonPressed;

    private void Start()
    {
        _isLoginButtonPressed = false;
    }

    public void ShowKeyboard(int inputIndex)
    {
        Keyboard.Show(inputIndex == 0 ? _email : _password);
    }

    public void AutoInputData(string email, string password)
    {
        _email.text = email;
        _password.text = password;
        _rememberMe.isOn = true;
    }

    public void CheckLogin()
    {
        if (_isLoginButtonPressed) return;
        ResetErrorText();

        if (_email.text == string.Empty || _password.text == string.Empty)
        {
            _emailError.text = "Fill in all the fields";
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        if (_password.text.Length < 6)
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

        _isLoginButtonPressed = true;
        StartCoroutine(GetPlayerLogin(_email.text, _password.text));
        EventBus.OnPlayerClickUI?.Invoke(0);
    }

    public IEnumerator GetPlayerLogin(string email, string password)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("email", email);
        form.AddField("password", password);

        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "authorization.php", form);
        yield return www.SendWebRequest();

        bool isRemember = PlayerPrefs.GetInt(stringBus.AccStatus) == 2;

        if (www.result != UnityWebRequest.Result.Success)
        {
            if (isRemember)
            {
                PlayerPrefs.DeleteKey(stringBus.AccStatus);
                //Show();
            }
            _isLoginButtonPressed = false;
            Notice.Dialog(www.error);
        }
        else
        {
            if (www.downloadHandler.text == "false")
            {
                if (isRemember)
                {
                    PlayerPrefs.DeleteKey(stringBus.AccStatus);
                    //Show();
                }
                _isLoginButtonPressed = false;
                _emailError.text = "Invalid e-mail or password";
                EventBus.OnPlayerClickUI?.Invoke(3);
            }
            else
            {
                LoadData.Instance.UpdateUser(www.downloadHandler.text);

                SpawnPlayer(email, password, isRemember || _rememberMe.isOn);
            }
        }
    }

    private void SpawnPlayer(string email, string password, bool remember)
    {
        LoadingUI.Show(LoadingShower.Type.Simple);

        StringBus stringBus = new();

        if (remember)
        {
            PlayerPrefs.SetInt(stringBus.AccStatus, 2);
            PlayerPrefs.SetString(stringBus.Email, email);
            PlayerPrefs.SetString(stringBus.Password, password);
        }
        else
        {
            PlayerPrefs.SetInt(stringBus.AccStatus, 1);
            PlayerPrefs.DeleteKey(stringBus.Email);
            PlayerPrefs.DeleteKey(stringBus.Password);
        }

        PlayerPrefs.Save();
        SaveData.Instance.Stats(SaveData.Statistics.Login);

        DOTween.Clear();
        ConnectDatabase.IsUserEnter = true;
        SceneManager.LoadSceneAsync((int)GameSettings.Scene.Lobby);
    }

    private void ResetErrorText()
    {
        _emailError.text = string.Empty;
        _passwordError.text = string.Empty;
    }

    public void Show()
    {
        _authPanel.transform.localPosition = new Vector2(-2000f, 0);
        _authPanel.SetActive(true);
        _authPanel.transform.DOLocalMoveX(0f, 0.3f);

        EventBus.OnPlayerClickUI?.Invoke(2);
    }

    public void Hide()
    {
        _authPanel.transform.DOScale(0.1f, 0.2f).OnComplete(() =>
        {
            _authPanel.SetActive(false);
            _authPanel.transform.localScale = new Vector2(0.8f, 0.8f);
        });
    }
}
