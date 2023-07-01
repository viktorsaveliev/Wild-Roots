using Photon.Pun;
using System.Collections;
using UnityEngine;

public class FanHandler : MonoBehaviour
{
    [SerializeField] private Fan[] _fans;
    private PhotonView _photonView;
    private int _enabledFans;

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();

        if(PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(TimerForEnableFan());
        }
    }

    private IEnumerator TimerForEnableFan()
    {
        while(_enabledFans < _fans.Length)
        {
            yield return new WaitForSeconds(30f);
            int randomID = GetRandomFreeFan();

            GameData gameData = new();
            gameData.CallMethod<FanHandler>(nameof(EnableFan), _photonView, RpcTarget.All, randomID);
        }
    }

    [PunRPC]
    private void EnableFan(int id)
    {
        _fans[id].Enable();
        _enabledFans++;
    }

    private int GetRandomFreeFan()
    {
        ArrayHandler arrayHandler = new();
        Fan[] mixedSpawnPoints = (Fan[])arrayHandler.MixArray(_fans);

        int forReturn = -1;
        for (int i = 0; i < mixedSpawnPoints.Length; i++)
        {
            if (mixedSpawnPoints[i].IsEnable) continue;
            forReturn = i;
            break;
        }

        return forReturn;
    }
}