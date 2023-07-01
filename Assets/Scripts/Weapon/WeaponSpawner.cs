using DG.Tweening;
using Photon.Pun;
using System.Collections;
using UnityEngine;

public class WeaponSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _weaponPrefabs;
    [SerializeField] private SpawnPoint[] _spawnPoints;
    [SerializeField] private Transform _weaponsContainer;

    private const int MAX_WEAPONS = 5;
    private readonly GameObject[] _weaponsPool = new GameObject[MAX_WEAPONS];

    private readonly Vector3[] _weaponsRotation =
    {
        new Vector3(-143.4f, 26.1f, -190.9f),
        new Vector3(24.28f, -36.44f, 20.89f),
        new Vector3(-12.87f, 12.19f, 26.32f)
    };

    public enum WeaponType
    {
        Grenade,
        RootsMine,
        Punch
    }

    private PhotonView _photonView;

    private Coroutine _respawnTimer = null;
    private readonly float _timeForRespawnWeapon = 1.5f;

    private void OnEnable()
    {
        EventBus.OnCharacterGetWeapon += ResetSpawnPoint;
    }

    private void OnDisable()
    {
        EventBus.OnCharacterGetWeapon -= ResetSpawnPoint;
    }

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus)
        {
            if (PhotonNetwork.IsMasterClient == false)
            {
                _photonView.RPC(nameof(GlobalUpdateWeapons), RpcTarget.MasterClient);
            }
        }
    }

    [PunRPC]
    private void GlobalUpdateWeapons()
    {
        int[] viewID = new int[MAX_WEAPONS];
        Vector3[] weaponPosition = new Vector3[MAX_WEAPONS];
        Vector3[] weaponScale = new Vector3[MAX_WEAPONS];
        bool[] weaponActiveStatus = new bool[MAX_WEAPONS];

        for (int i = 0; i < MAX_WEAPONS; i++)
        {
            if (_weaponsPool[i] == null) continue;
            viewID[i] = _weaponsPool[i].GetComponent<Weapon>().PhotonView.ViewID;
            weaponPosition[i] = _weaponsPool[i].transform.position;
            weaponScale[i] = _weaponsPool[i].transform.lossyScale;
            weaponActiveStatus[i] = _weaponsPool[i].activeSelf;
        }
        _photonView.RPC(nameof(UpdateWeaponsPositionForAll), RpcTarget.Others, viewID, weaponPosition, weaponScale, weaponActiveStatus);
    }

    [PunRPC]
    private void UpdateWeaponsPositionForAll(int[] viewID, Vector3[] weaponPos, Vector3[] weaponScale, bool[] weaponActive)
    {
        for (int i = 0; i < viewID.Length; i++)
        {
            GameObject weapon = PhotonView.Find(viewID[i]).gameObject;
            if (weapon == null) continue;
            weapon.transform.position = weaponPos[i];
            weapon.transform.DOScale(weaponScale[i], 0.5f);
            weapon.SetActive(weaponActive[i]);
        }
    }

    public void CreateWeapons()
    {
        for(int i = 0; i < MAX_WEAPONS; i++)
        {
            GameObject weaponObj;
            if (PhotonNetwork.OfflineMode)
            {
                weaponObj = Instantiate(_weaponPrefabs,
                    _spawnPoints[i].transform.position, Quaternion.Euler(_weaponsRotation[0]));

                UpdateWeaponInfoForAll(weaponObj.GetComponent<Weapon>(), i);
            }
            else
            {
                weaponObj = PhotonNetwork.InstantiateRoomObject(_weaponPrefabs.name,
                    _spawnPoints[i].transform.position, Quaternion.Euler(_weaponsRotation[0]));

                _photonView.RPC(nameof(UpdateWeaponInfoForAll), RpcTarget.All,
                weaponObj.GetComponent<Weapon>().PhotonView.ViewID, i);
            }
        }
        StartTimerForWeaponSpawn();
    }

    [PunRPC]
    private void UpdateWeaponInfoForAll(int viewID, int spawnPointID)
    {
        Weapon weapon = PhotonView.Find(viewID).GetComponent<Weapon>();

        weapon.transform.parent = _weaponsContainer;
        weapon.WeaponType = WeaponType.Grenade;
        weapon.CurrentSpawnPoint = _spawnPoints[spawnPointID];
        weapon.CurrentSpawnPoint.IsUsed = true;

        if (TryPutWeaponInPool(weapon.gameObject) == false)
        {
            print("[ERROR #053] Can't add weapons");
        }
    }

    private void UpdateWeaponInfoForAll(Weapon weapon, int spawnPointID)
    {
        weapon.transform.parent = _weaponsContainer;
        weapon.WeaponType = WeaponType.Grenade;
        weapon.CurrentSpawnPoint = _spawnPoints[spawnPointID];
        weapon.CurrentSpawnPoint.IsUsed = true;

        if (TryPutWeaponInPool(weapon.gameObject) == false)
        {
            print("[ERROR #053] Can't add weapons");
        }
    }

    private bool TryPutWeaponInPool(GameObject weapon)
    {
        for(int i = 0; i < MAX_WEAPONS; i++)
        {
            if (_weaponsPool[i] != null) continue;
            _weaponsPool[i] = weapon;
            return true;
        }
        return false;
    }

    private GameObject GetInactiveWeapon()
    {
        for (int i = 0; i < MAX_WEAPONS; i++)
        {
            if (_weaponsPool[i] == null || _weaponsPool[i].activeSelf) continue;
            return _weaponsPool[i];
        }
        return null;
    }

    private void ResetSpawnPoint(Weapon weapon)
    {
        if(weapon.CurrentSpawnPoint != null)
        {
            weapon.CurrentSpawnPoint.IsUsed = false;
            weapon.CurrentSpawnPoint = null;
        }
    }

    public void StartTimerForWeaponSpawn()
    {
        if (_respawnTimer != null)
        {
            StopCoroutine(_respawnTimer);
        }
        _respawnTimer = StartCoroutine(RespawnWeaponTimer());
    }

    private IEnumerator RespawnWeaponTimer()
    {
        yield return new WaitForSeconds(_timeForRespawnWeapon);
        SpawnRandomWeapon();
        _respawnTimer = null;
    }

    private void SpawnRandomWeapon()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject weapon = GetInactiveWeapon();
            if(weapon != null)
            {
                int spawnPointID = GetRandomFreeSpawnPoint();
                if (spawnPointID != -1)
                {
                    _photonView.RPC(nameof(SpawnWeaponForAll), RpcTarget.All,
                        weapon.GetPhotonView().ViewID, spawnPointID);
                }
            }
            StartTimerForWeaponSpawn();
        }
    }

    [PunRPC]
    public void SpawnWeaponForAll(int viewID, int spawnPoint)
    {
        if (spawnPoint == -1) return;

        PhotonView phView = PhotonView.Find(viewID);
        GameObject weaponObject = phView.gameObject;
        Weapon weapon = weaponObject.GetComponent<Weapon>();
        weapon.CurrentSpawnPoint = _spawnPoints[spawnPoint];
        weapon.CurrentSpawnPoint.IsUsed = true;

        weapon.SetOwnerLocal(-1);

        Vector3 spawnPos = _spawnPoints[spawnPoint].transform.position;
        weaponObject.transform.SetPositionAndRotation(new Vector3(spawnPos.x, 0.85f, spawnPos.z), Quaternion.Euler(_weaponsRotation[(int)weapon.WeaponType]));
        weaponObject.transform.parent = _weaponsContainer;

        weaponObject.GetComponent<Collider>().isTrigger = true;

        Rigidbody rigidbody = weaponObject.GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        weaponObject.SetActive(true);
        weaponObject.transform.DOScale(4, 0.3f);

        EventBus.OnWeaponSpawned?.Invoke(weaponObject);
    }

    private int GetRandomFreeSpawnPoint()
    {
        ArrayHandler arrayHandler = new();
        SpawnPoint[] mixedSpawnPoints = (SpawnPoint[])arrayHandler.MixArray(_spawnPoints);

        int forReturn = -1;
        for (int i = 0; i < mixedSpawnPoints.Length; i++)
        {
            if (mixedSpawnPoints[i].IsUsed || mixedSpawnPoints[i].IsActive == false) continue;
            forReturn = i;
            break;
        }

        return forReturn;
    }
}
