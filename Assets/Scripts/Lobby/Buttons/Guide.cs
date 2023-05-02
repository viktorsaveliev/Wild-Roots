using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class Guide : MonoBehaviour, INoticeAction
{
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
        Notice.Dialog(NoticeDialog.Message.StartTutorial, this, "NoticeButton_Yes", "Notice_Close");
        EventBus.OnPlayerClickUI?.Invoke(0);
    }

    public IEnumerator StartTutorial()
    {
        LoadingUI.Show(LoadingShower.Type.Progress);
        DOTween.Clear();

        AsyncOperation percentload = SceneManager.LoadSceneAsync((int)GameSettings.Scene.Tutorial);
        while (!percentload.isDone)
        {
            yield return new WaitForEndOfFrame();
            LoadingUI.UpdateProgress(percentload.progress);
        }
    }
}
