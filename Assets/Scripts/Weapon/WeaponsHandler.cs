using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using DG.Tweening;

[RequireComponent(typeof(HoneycombHandler), typeof(PhotonView), typeof(ServerHandler))]
public class WeaponsHandler : MonoBehaviour
{
    [SerializeField] private Transform _weaponsParent;

    private const int MAX_WEAPON_TYPE = 2;
    [SerializeField] private GameObject[] _weaponPrefabs = new GameObject[MAX_WEAPON_TYPE];
    [SerializeField] private int[] _weaponsCount = new int[MAX_WEAPON_TYPE];
    private readonly Vector3[] _weaponsRotation = 
    { 
        new Vector3(0, 0, -130f), 
        new Vector3(24.28f, -36.44f, 20.89f), 
        new Vector3(-12.87f, 12.19f, 26.32f) 
    };

    public enum WeaponType
    {
        Grenade,
        RootsMine,
        Punch
    }

    private HoneycombHandler _honeycombHandler;
    private PhotonView _photonView;
    private List<Honeycomb> _honeycombs = new List<Honeycomb>();
    private List<GameObject> _pool = new List<GameObject>();

    private Coroutine _timer;
    private readonly float _timeForSpawnNewWeapon = 2f;

    #region MonoBehaviour

    private void Awake()
    {
        _honeycombHandler = GetComponent<HoneycombHandler>();
        _photonView = PhotonView.Get(this);
    }
    #endregion

    private IEnumerator SpawnTimer()
    {
        yield return new WaitForSeconds(_timeForSpawnNewWeapon);
        SpawnRandomWeapon(GetComponent<ServerHandler>().CurrentRound);
    }

    public void CreateWeapons(int round)
    {
        for(int s = 0; s < MAX_WEAPON_TYPE; s++)
        {
            GetHoneycombsInCircle(round + s + 1);

            for (int i = 0; i < _weaponsCount[s]; i++)
            {
                int randomPos = Random.Range(0, _honeycombs.Count);

                GameObject weaponObj = PhotonNetwork.InstantiateRoomObject(_weaponPrefabs[s].name, 
                    _honeycombs[randomPos].transform.position, Quaternion.Euler(_weaponsRotation[s]));

                _photonView.RPC(nameof(UpdateWeaponInfoForAll), RpcTarget.All, 
                    weaponObj.GetComponent<Weapon>().PhotonViewObject.ViewID, round + s + 1, s, randomPos);
            }
        }
        StartTimerForWeaponSpawn();
    }

    public void StartTimerForWeaponSpawn()
    {
        if (_timer != null) StopCoroutine(_timer);
        _timer = StartCoroutine(SpawnTimer());
    }

    [PunRPC]
    public void UpdateWeaponInfoForAll(int weaponViewID, int currentRound, int weaponType, int randomPos)
    {
        //GetHoneycombsInCircle(currentRound);
        if (randomPos < 0 || randomPos > _honeycombs.Count) return; 
        Weapon weapon = PhotonView.Find(weaponViewID).GetComponent<Weapon>();

        weapon.transform.parent = _weaponsParent;
        weapon.CurrentRoundLayerWhereImStay = currentRound;
        weapon.CurrentHoneycombWhereImStay = _honeycombs[randomPos];
        weapon.WeaponType = (WeaponType)weaponType;

        _honeycombs[randomPos].IsTiedWeapon = true;
        _honeycombs.Remove(_honeycombs[randomPos]);

        _pool.Add(weapon.gameObject);
    }

    public void SpawnRandomWeapon(int round)
    {
        int randomRoundLayer = Random.Range(round, 4);
        Honeycomb[] honeycombs = _honeycombHandler.GetHoneycombCircles[randomRoundLayer].GetComponentsInChildren<Honeycomb>();

        if (TryGetObject(out GameObject weapon))
        {
            Honeycomb randomHoneycomb = GetRandomFreeHoneycomb(honeycombs);
            if (randomHoneycomb != null)
            {
                randomHoneycomb.IsTiedWeapon = true;
                weapon.GetPhotonView().Owner.TagObject = randomHoneycomb;
                _photonView.RPC(nameof(SpawnWeaponForAll), RpcTarget.All, 
                    weapon.GetPhotonView().ViewID, randomHoneycomb.transform.position, randomRoundLayer);
            }
        }

        if (_timer != null) StopCoroutine(_timer);
        _timer = StartCoroutine(SpawnTimer());
    }

    private Honeycomb GetRandomFreeHoneycomb(Honeycomb[] honeycombs)
    {
        Honeycomb[] mixedHoneycombs = MixArray(honeycombs);

        Honeycomb forReturn = null;
        for (int i = 0; i < mixedHoneycombs.Length; i++)
        {
            if (mixedHoneycombs[i].IsTiedWeapon) continue;
            forReturn = mixedHoneycombs[i];
            break;
        }

        return forReturn;
    }

    private Honeycomb[] MixArray(Honeycomb[] honeycombs)
    {
        for (int i = 0; i < honeycombs.Length; i++)
        {
            Honeycomb currentValue = honeycombs[i];
            int randomValue = Random.Range(i, honeycombs.Length);

            honeycombs[i] = honeycombs[randomValue];
            honeycombs[randomValue] = currentValue;
        }
        return honeycombs;
    }

    private Honeycomb[] GetHoneycombsInCircle(int round)
    {
        _honeycombs.Clear();
        Honeycomb[] honeycombs = _honeycombHandler.GetHoneycombCircles[round].GetComponentsInChildren<Honeycomb>();
        for (int i = 0; i < honeycombs.Length; i++)
        {
            _honeycombs.Add(honeycombs[i]);
        }
        return honeycombs;
    }

    [PunRPC]
    public void SpawnWeaponForAll(int viewID, Vector3 honeycombPos, int roundLayer)
    {
        PhotonView phView = PhotonView.Find(viewID);
        GameObject weaponObject = phView.gameObject;
        Weapon weapon = weaponObject.GetComponent<Weapon>();
        weapon.CurrentRoundLayerWhereImStay = roundLayer;
        weapon.CurrentHoneycombWhereImStay = (Honeycomb)phView.Owner.TagObject;

        weaponObject.transform.position = new Vector3(honeycombPos.x, 0.85f, honeycombPos.z);
        weaponObject.transform.rotation = Quaternion.Euler(_weaponsRotation[(int)weapon.WeaponType]);

        weaponObject.GetComponent<Collider>().isTrigger = true;

        Rigidbody rigidbody = weaponObject.GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        weaponObject.SetActive(true);
        weapon.SpawnAnimation = weaponObject.transform.DOScale(1, 0.3f).OnComplete(() => weapon.SpawnAnimation = null);

        EventBus.OnWeaponSpawned?.Invoke(weaponObject);
    }

    protected bool TryGetObject(out GameObject result)
    {
        result = _pool.FirstOrDefault(p => p.activeSelf == false);
        return result != null;
    }

    public void DeleteWeaponInRoundLayer(int round)
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            Weapon weapon = _pool[i].GetComponent<Weapon>();
            if (weapon.CurrentRoundLayerWhereImStay != round) continue;
            _photonView.RPC(nameof(DeleteWeaponForAll), RpcTarget.All, weapon.PhotonViewObject.ViewID);
        }
    }

    [PunRPC]
    public void DeleteWeaponForAll(int viewID)
    {
        PhotonView photonView = PhotonView.Find(viewID);
        DeleteWeapon(photonView.GetComponent<Weapon>());
    }

    public void DeleteWeapon(Weapon weapon)
    {
        weapon.transform.DOScale(0.2f, 1f).OnComplete(() =>
        {
            weapon.DisableWeapon();
        });
    }
}
