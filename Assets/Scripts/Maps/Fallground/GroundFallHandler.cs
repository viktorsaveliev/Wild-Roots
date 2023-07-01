using Photon.Pun;
using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

public class GroundFallHandler : MonoBehaviour
{
    [SerializeField] private GroundBlock[] _groundBlocks;
    [SerializeField] private NavMeshSurface _navMesh;

    private int _fallGroundCount;
    private const float _delayForFall = 5f;
    private PhotonView _photonView;

    private void Start()
    {
        _photonView = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode)
        {
            StartCoroutine(TimerForFallGround());
        }
    }

    private IEnumerator TimerForFallGround()
    {
        yield return new WaitForSeconds(45f);
        if (_fallGroundCount >= _groundBlocks.Length) yield break;

        GameData game = new();
        game.CallMethod<GroundFallHandler>(nameof(FallGround), _photonView, RpcTarget.All);

        StartCoroutine(TimerForFallGround());
    }

    [PunRPC]
    private void FallGround()
    {
        _groundBlocks[_fallGroundCount].gameObject.isStatic = false;
        /*GameObject newBlock = Instantiate(_groundBlocks[_fallGroundCount].gameObject, _groundBlocks[_fallGroundCount].transform.position, 
            _groundBlocks[_fallGroundCount].transform.rotation);

        Destroy(_groundBlocks[_fallGroundCount].gameObject);
        _groundBlocks[_fallGroundCount] = newBlock.GetComponent<GroundBlock>();*/
        _groundBlocks[_fallGroundCount].gameObject.layer = 0;
        _groundBlocks[_fallGroundCount].Fall(_delayForFall);

        _navMesh.BuildNavMesh();
        _fallGroundCount++;
    }
}
