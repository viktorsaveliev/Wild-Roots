using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayAsGuest : MonoBehaviour
{
    public void OnClickButton()
    {
        LoadingUI.Show(LoadingShower.Type.Simple);

        EventBus.OnPlayerClickUI?.Invoke(0);

        StringBus stringBus = new();

        /*_reg.Hide();
        _auth.Hide();
        gameObject.SetActive(false);*/

        PlayerPrefs.SetInt(stringBus.IsGuest, 1);
        PlayerPrefs.Save();

        PlayerData.Update(string.Empty, 1, 0, 0, 0, 0, 1);

        ConnectDatabase.IsUserEnter = true;
        DOTween.Clear();
        SceneManager.LoadSceneAsync((int)GameSettings.Scene.Lobby);

        
    }
}
