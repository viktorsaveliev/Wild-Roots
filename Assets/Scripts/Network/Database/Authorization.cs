using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Net;
using TMPro;
using UnityEngine.UI;

public class Authorization : MonoBehaviour
{
    [SerializeField] private TMP_InputField _email;
    [SerializeField] private TMP_InputField _password;
    [SerializeField] private Toggle _rememberMe;
    [SerializeField] private GameObject _authPanel;

    public void CheckLogin()
    {
        if (_email.text == string.Empty || _password.text == string.Empty)
        {
            print("Заполните все поля");
            return;
        }

        if (_password.text.Length < 6)
        {
            print("Длина пароля должна быть минимум 6 символов");
            return;
        }

        if (_email.text.Contains('@') == false)
        {
            print("Неверный формат ввода");
            return;
        }

        StartCoroutine(GetPlayerLogin(_email.text, _password.text));
    }

    public IEnumerator GetPlayerLogin(string email, string password)
    {
        WWWForm form = new();
        form.AddField("email", email);
        form.AddField("password", password);

        using UnityWebRequest www = UnityWebRequest.Post("https://www.wildroots.fun/public/authorization.php", form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error while checking user: " + www.error);
        }
        else
        {
            bool successful = bool.Parse(www.downloadHandler.text);
            if(successful)
            {
                StringBus stringBus = new();
                if (_rememberMe.isOn)
                {
                    PlayerPrefs.SetInt(stringBus.AccStatus, 2);
                    PlayerPrefs.SetString(stringBus.Email, email);
                    PlayerPrefs.SetString(stringBus.Password, password);

                    print("remembered");
                }
                else
                {
                    PlayerPrefs.SetInt(stringBus.AccStatus, 1);
                    PlayerPrefs.DeleteKey(stringBus.Email);
                    PlayerPrefs.DeleteKey(stringBus.Password);
                }
                PlayerPrefs.Save();
                print("autorizatcisz");
            }
            else
            {
                print("neverno");
            }
        }
    }

    private string GetIPAddress()
    {
        string ipAddress = "";
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ipAddress = ip.ToString();
                break;
            }
        }

        return ipAddress;
    }

    public void Show() => _authPanel.SetActive(true);
    public void Hide() => _authPanel.SetActive(false);
}
