using DG.Tweening;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToLobby : MonoBehaviourPunCallbacks
{
    public void LoadLobby()
    {
        LoadingUI.Show(LoadingShower.Type.Progress);
        DOTween.Clear();
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        LoadingUI.UpdateProgress(0.5f);
        SceneManager.LoadScene(0);
        base.OnLeftRoom();
    }
}
