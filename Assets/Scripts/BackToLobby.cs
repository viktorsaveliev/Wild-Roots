using Photon.Pun;
using UnityEngine.SceneManagement;
using CrazyGames;
using DG.Tweening;

public class BackToLobby : MonoBehaviourPunCallbacks, INoticeAction
{
    public void ActionOnClickNotice(int button)
    {
        if (button == 0) OnClick();
        Notice.HideDialog();
    }

    public void ExitButton(int buttonID)
    {
        if (buttonID == 0)
        {
            OnClick();
        }
        else
        {
            Notice.Dialog(NoticeDialog.Message.BackToLobby, this, "NoticeButton_Yes", "Notice_Close");
        }
    }

    private void OnClick()
    {
        LoadingUI.Show(LoadingShower.Type.Progress);
        if (PhotonNetwork.OfflineMode == false)
        {
            PhotonNetwork.DestroyPlayerObjects(PhotonNetwork.LocalPlayer);
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            LoadLobbby();
        }
    }

    private void LoadLobbby()
    {
        SaveData.Instance.Stats(SaveData.Statistics.LeftTheMatch);

        DOTween.Clear();
        CrazyAds.Instance.beginAdBreak(AdsSucces, AdsError);

        LoadingUI.UpdateProgress(0.5f);
        SceneManager.LoadScene((int)GameSettings.Scene.Lobby);
    }

    public override void OnLeftRoom()
    {
        if (GameSettings.OfflineMode) return;
        base.OnLeftRoom();
        LoadLobbby();
    }

    private void AdsSucces()
    {
        PlayerData.ViewedAds();
        SaveData.Instance.Stats(SaveData.Statistics.MidgameAds);
    }

    private void AdsError()
    {
        print("[Ads error] Some problem #036");
    }
}
