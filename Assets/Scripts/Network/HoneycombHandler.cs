using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(ServerHandler), typeof(WeaponsHandler))]
public class HoneycombHandler : MonoBehaviour
{
    [SerializeField] private GameObject[] _honeycombCircles;

    public GameObject[] GetHoneycombCircles => _honeycombCircles;
    private ServerHandler _server;
    private readonly float _timeToShake = 5f;

    public void Init()
    {
        _server = GetComponent<ServerHandler>();
        //_server.SetTimeToNextRound(90);
    }

    public void ReInit(int timeToNextRound) // when switched MasterClient
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _server = GetComponent<ServerHandler>();
            _server.ServerPhotonView = PhotonView.Get(this);
        }
        //_server.SetTimeToNextRound(timeToNextRound);
    }

    [PunRPC]
    public void DestroyHoneycombs(int round)
    {
        Honeycomb[] currentHoneycombs = _honeycombCircles[round].GetComponentsInChildren<Honeycomb>();

        for(int i = 0; i < currentHoneycombs.Length; i++)
        {
            currentHoneycombs[i].DestroyHoneycomb(1 + i, _timeToShake);
        }
    }
}
