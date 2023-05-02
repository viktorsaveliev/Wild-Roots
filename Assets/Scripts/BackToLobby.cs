using Photon.Pun;
using UnityEngine.SceneManagement;
using CrazyGames;
using DG.Tweening;

public class BackToLobby : MonoBehaviourPunCallbacks, INoticeAction
{
    public void ActionOnClickNotice(int button)
    {
        if (button == 0) LoadLobby();
        Notice.HideDialog();
    }

    public void ExitButton(int buttonID)
    {
        if (buttonID == 0)
        {
            LoadLobby();
        }
        else
        {
            Notice.Dialog(NoticeDialog.Message.BackToLobby, this, "NoticeButton_Yes", "Notice_Close");
        }
    }

    private void LoadLobby()
    {
        if (PhotonNetwork.OfflineMode == false) PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);

        LoadingUI.Show(LoadingShower.Type.Progress);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        DOTween.Clear();
        CrazyAds.Instance.beginAdBreak(AdsSucces, AdsError);

        LoadingUI.UpdateProgress(0.5f);
        SceneManager.LoadScene((int)GameSettings.Scene.Lobby);
    }

    private void AdsSucces()
    {
        PlayerData.ViewedAds();
    }
    private void AdsError()
    {
        print("[Ads error] Some problem #036");
    }
}
