using System.Collections;
using UnityEngine;
using Photon.Pun;

public class CarsHandler : MonoBehaviour
{
    private const int MAX_ROADS = 2;

    [SerializeField] private GameObject[] _carPrefabs;
    [SerializeField] private Transform[] _startPos = new Transform[MAX_ROADS];
    [SerializeField] private Transform[] _endPos = new Transform[MAX_ROADS];
    [SerializeField] private Transform _parent;

    private bool[] _isRoadUsed = new bool[MAX_ROADS];
    private Car[] _cars = new Car[MAX_ROADS];
    private PhotonView _photonView;

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();

        for (int i = 0; i < MAX_ROADS; i++)
        {
            GameObject car = Instantiate(_carPrefabs[Random.Range(0, _carPrefabs.Length)], _parent);
            _cars[i] = car.GetComponent<Car>();

            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(StartCarWithDelay), RpcTarget.All, Random.Range(3f, 15f), i, 3f);
            }
        }
    }

    [PunRPC]
    private void StartCarWithDelay(float delay, int carID, float speed)
    {
        StartCoroutine(StartCarWithDelayAsync(delay, carID, speed));
    }

    private IEnumerator StartCar(int carID, float speed)
    {
        _cars[carID].gameObject.SetActive(true);
        _cars[carID].Go(_startPos[carID].position, _endPos[carID].position, speed);
        _isRoadUsed[carID] = true;
        yield return new WaitForSeconds(speed);
        _isRoadUsed[carID] = false;

        if (PhotonNetwork.IsMasterClient)
        {
            _photonView.RPC(nameof(StartCarWithDelay), RpcTarget.All, Random.Range(3f, 15f), carID, 3f);
        }
    }

    private IEnumerator StartCarWithDelayAsync(float delay, int carID, float speed)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(StartCar(carID, speed));
    }
}
