using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToLobby : MonoBehaviour
{
    public void LoadLobby()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene(0);
    }
}
