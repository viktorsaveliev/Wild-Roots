using UnityEngine;
using TMPro;
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
            print("��������� ��� ����");
            return;
        }

        if(_password.text != _repeatPassword.text)
        {
            print("������ �� ���������");
            return;
        }

        if(_password.text.Length < 6)
        {
            print("����� ������ ������ ���� ������� 6 ��������");
            return;
        }

        if (_email.text.Contains('@') == false)
        {
            print("�������� ������ �����");
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
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
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
            LoadData.Instance.LoadAccount(email, password, false);
            Hide();
        }
        else
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
    }
}