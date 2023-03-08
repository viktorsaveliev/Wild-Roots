using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SaveData : MonoBehaviour
{
    [SerializeField] private GameObject _saveDataIndicator;
    public static SaveData Instance;

    private void Start()
    {
        Instance = this;
    }

    public void SaveLevelData(int playerID, Character player)
    {
        StartCoroutine(IEUpdateLevel(playerID, player));
    }

    private IEnumerator IEUpdateLevel(int playerID, Character player)
    {
        if(_saveDataIndicator != null) _saveDataIndicator.SetActive(true);
        WWWForm form = new();
        form.AddField("ID", playerID);
        form.AddField("level", player.Level);
        form.AddField("exp", player.Exp);
        form.AddField("wins", player.Wins);

        using UnityWebRequest www = UnityWebRequest.Post("https://www.wildroots.fun/public/update_level.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = bool.Parse(www.downloadHandler.text);
            if (success == false)
            {
                Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
            }
            else print("yes level");
        }
        else
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
        if (_saveDataIndicator != null) _saveDataIndicator.SetActive(false);
    }
}
