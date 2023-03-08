using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Guide : MonoBehaviour, INoticeAction
{
    private void Start()
    {
        /*StringBus stringBus = new();
        int level = PlayerPrefs.GetInt(stringBus.PlayerLevel);
        if(level == 0)
        {
            Invoke(nameof(LoadTutorial), 1f);

            PlayerPrefs.SetInt(stringBus.PlayerLevel, 1);
            PlayerPrefs.Save();
        }*/
    }

    public void ActionOnClickNotice(int button)
    {
        if(button == 0)
        {
            StartCoroutine(StartTutorial());
        }
        Notice.HideDialog();
    }

    public void LoadTutorial()
    {
        Notice.ShowDialog(NoticeDialog.Message.StartTutorial, this, "NoticeButton_Yes", "Notice_Close");
        EventBus.OnPlayerClickUI?.Invoke(0);
    }

    public IEnumerator StartTutorial()
    {
        LoadingUI.Show(LoadingShower.Type.Progress);
        AsyncOperation percentload = SceneManager.LoadSceneAsync(2);
        while (!percentload.isDone)
        {
            yield return new WaitForEndOfFrame();
            LoadingUI.UpdateProgress(percentload.progress);
        }
    }
}
