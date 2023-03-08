using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
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

        StringBus stringBus = new();
        if (www.result != UnityWebRequest.Result.Success)
        {
            if (PlayerPrefs.GetInt(stringBus.AccStatus) == 2)
            {
                Show();
            }
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
        else
        {
            bool successful = bool.Parse(www.downloadHandler.text);
            if (successful)
            {
                LoadData.Instance.LoadAccount(email, password, _rememberMe.isOn);
                Hide();
            }
            else
            {
                if(PlayerPrefs.GetInt(stringBus.AccStatus) == 2)
                {
                    Show();
                }
                print("neverno");
            }
        }
    }

    /*private string GetIPAddress()
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
    }*/

    public void Show() => _authPanel.SetActive(true);
    public void Hide() => _authPanel.SetActive(false);
}
