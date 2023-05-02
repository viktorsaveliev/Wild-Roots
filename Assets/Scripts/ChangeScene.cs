using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [SerializeField] private GameSettings.Scene _scene;
    [SerializeField] private bool _isUserExit;

    public void GoToScene()
    {
        if(_isUserExit)
        {
            ConnectDatabase.IsUserEnter = false;
            StringBus stringBus = new();
            PlayerPrefs.SetInt(stringBus.IsGuest, 0);
            PlayerPrefs.Save();
        }

        DOTween.Clear();
        SceneManager.LoadSceneAsync((int)_scene);
    }
}