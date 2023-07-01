using System.Collections;
using UnityEngine;
using Photon.Pun;

public class CarsHandler : MonoBehaviour
{
    [SerializeField] private Car _car;
    private PhotonView _photonView;

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();

        if (PhotonNetwork.IsMasterClient)
        {
            if(GameSettings.OfflineMode)
            {
                StartCarWithDelay(60);
            }
            else
            {
                _photonView.RPC(nameof(StartCarWithDelay), RpcTarget.All, 60f);
            }
        }
    }

    [PunRPC]
    private void StartCarWithDelay(float delay)
    {
        StartCoroutine(StartCarWithDelayAsync(delay));
    }

    private IEnumerator StartCarWithDelayAsync(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCar();
    }

    private void StartCar()
    {
        _car.transform.localScale = Vector3.zero;
        _car.gameObject.SetActive(true);
        _car.Go();
    }
}
