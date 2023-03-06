using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class Registration : MonoBehaviour
{
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private TMP_InputField _repeatPassword;
    [SerializeField] private GameObject _regPanel;

    public void Show() => _regPanel.SetActive(true);
    public void Hide() => _regPanel.SetActive(false);

    public void RegisterButton()
    {
        if(_email.text == string.Empty || _password.text == string.Empty || _repeatPassword.text == string.Empty)
        {
            print("Заполните все поля");
            return;
        }

        if(_password.text != _repeatPassword.text)
        {
            print("Пароли не совпадают");
            return;
        }

        if(_password.text.Length < 6)
        {
            print("Длина пароля должна быть минимум 6 символов");
            return;
        }

        if (_email.text.Contains('@') == false)
        {
            print("Неверный формат ввода");
            return;
        }
        StartCoroutine(CheckIfUserExists(_email.text));
    }

    private IEnumerator CheckIfUserExists(string email)
    {
        WWWForm form = new();
        form.AddField("email", email);
        using UnityWebRequest www = UnityWebRequest.Post("https://www.wildroots.fun/public/get_mail.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error while checking user email: " + www.error);
            yield break;
        }

        bool userExists = bool.Parse(www.downloadHandler.text);
        if (userExists)
        {
            Debug.Log("User with email " + email + " already exists in the database.");
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

        using UnityWebRequest www = UnityWebRequest.Post("https://www.wildroots.fun/public/register.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("User registered successfully");
        }
        else
        {
            Debug.Log("Error registering user: " + www.error);
        }
    }
}