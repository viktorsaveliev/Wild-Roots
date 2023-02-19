using Photon.Pun;
using UnityEngine.SceneManagement;

public class BackToLobby : MonoBehaviourPunCallbacks, INoticeAction
{
    public void ActionOnClickNotice(int button)
    {
        if (button == 0) LoadLobby();
        else Notice.HideDialog();
    }

    public void ExitButton(int buttonID)
    {
        if (buttonID == 0)
        {
            LoadLobby();
        }
        else
        {
            Notice.ShowDialog(NoticeDialog.Message.BackToLobby, this, "NoticeButton_Yes", "Notice_Close");
        }
    }

    private void LoadLobby()
    {
        PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);

        LoadingUI.Show(LoadingShower.Type.Progress);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        LoadingUI.UpdateProgress(0.5f);
        SceneManager.LoadScene(0);
    }
}
