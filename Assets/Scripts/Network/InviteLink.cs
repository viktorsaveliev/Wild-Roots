using UnityEngine;
using CrazyGames;
using System.Collections.Generic;
using Photon.Pun;

public class InviteLink : MonoBehaviour
{
    public void OnClickButton()
    {
        Dictionary<string, string> parameters = new()
        {
            { "roomId", PhotonNetwork.CurrentRoom.Name }
        };

        string inviteLink = CrazyEvents.Instance.InviteLink(parameters);
        CrazyEvents.Instance.CopyToClipboard(inviteLink);
    }
}
