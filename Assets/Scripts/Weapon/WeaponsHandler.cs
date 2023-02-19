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
    private List<Honeycomb> _honeycombs = new();
    public List<GameObject> Pool = new();

    private Coroutine _timer = null;
    private readonly float _timeForRespawnWeapon = 1.5f;

    public bool IsNeedUpdateHoneycomb;
    
    #region MonoBehaviour

    private void Awake()
    {
        _honeycombHandler = GetComponent<HoneycombHandler>();
        _photonView = PhotonView.Get(this);
    }

    private void Start()
    {
        if(!PhotonNetwork.IsMasterClient && GameSettings.GameMode == GameModeSelector.GameMode.Deathmatch)
        {
            StartCoroutine(LoadDeathmatchMode());
        }
    }

    private void OnApplicationFocus(bool focus)
    {
        if(focus)
        {
            _photonView.RPC(nameof(GlobalUpdateWeapons), RpcTarget.MasterClient);
        }
    }

    #endregion

    private IEnumerator LoadDeathmatchMode()
    {
        yield return new WaitForSeconds(0.5f);
        UpdateWeaponsPool();
        yield return new WaitForEndOfFrame();
        _photonView.RPC(nameof(GlobalUpdateWeapons), RpcTarget.MasterClient);
    }

    private IEnumerator SpawnTimer()
    {
        yield return new WaitForSeconds(_timeForRespawnWeapon);
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

    public void UpdateWeaponsPool()
    {
        Pool.Clear();
        Weapon[] weapons = FindObjectsOfType<Weapon>(true);
        foreach (Weapon weapon in weapons)
        {
            if (weapon is Punch) continue;
            Pool.Add(weapon.gameObject);
        }
    }

    public void StartTimerForWeaponSpawn()
    {
        if (_timer != null)
        {
            StopCoroutine(_timer);
            _timer = null;
        }
        _timer = StartCoroutine(SpawnTimer());
    }

    [PunRPC]
    public void UpdateWeaponInfoForAll(int weaponViewID, int currentRound, int weaponType, int randomPos)
    {
        if (IsNeedUpdateHoneycomb)
        {
            GetHoneycombsInCircle(currentRound);
            IsNeedUpdateHoneycomb = false;
        }

        if (randomPos < 0 || randomPos >= _honeycombs.Count) return; 
        Weapon weapon = PhotonView.Find(weaponViewID).GetComponent<Weapon>();

        weapon.transform.parent = _weaponsParent;
        weapon.CurrentLayerWhereImStay = currentRound;
        weapon.CurrentHoneycombWhereImStay = _honeycombs[randomPos];
        weapon.WeaponType = (WeaponType)weaponType;

        _honeycombs[randomPos].IsTiedWeapon = true;
        _honeycombs.Remove(_honeycombs[randomPos]);

        Pool.Add(weapon.gameObject);
    }

    [PunRPC]
    public void GlobalUpdateWeapons()
    {
        int[] viewID = new int[Pool.Count];
        Vector3[] weaponPosition = new Vector3[Pool.Count];
        Vector3[] weaponScale = new Vector3[Pool.Count];
        bool[] weaponActiveStatus = new bool[Pool.Count];
        
        for (int i = 0; i < Pool.Count; i++)
        {
            if (Pool[i] == null) continue;
            viewID[i] = Pool[i].GetComponent<Weapon>().PhotonViewObject.ViewID;
            weaponPosition[i] = Pool[i].transform.position;
            weaponScale[i] = Pool[i].transform.lossyScale;
            weaponActiveStatus[i] = Pool[i].activeSelf;
        }
        _photonView.RPC(nameof(UpdateWeaponsPositionForAll), RpcTarget.Others, viewID, weaponPosition, weaponScale, weaponActiveStatus);
    }

    [PunRPC]
    public void UpdateWeaponsPositionForAll(int[] viewID, Vector3[] weaponPos, Vector3[] weaponScale, bool[] weaponActive)
    {
        for (int s = 0; s < viewID.Length; s++)
        {
            for (int i = 0; i < Pool.Count; i++)
            {
                Weapon weapon = Pool[i].GetComponent<Weapon>();
                if (!weapon || weapon.PhotonViewObject.ViewID != viewID[s]) continue;

                Pool[i].transform.position = weaponPos[s];
                Pool[i].transform.DOScale(weaponScale[s], 0.5f);
                Pool[i].SetActive(weaponActive[s]);
            }
        }
    }

    public void SpawnRandomWeapon(int round)
    {
        if (PhotonNetwork.IsMasterClient)
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
            StartTimerForWeaponSpawn();
        }
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
        weapon.CurrentLayerWhereImStay = roundLayer;
        weapon.CurrentHoneycombWhereImStay = (Honeycomb)phView.Owner.TagObject;

        weaponObject.transform.position = new Vector3(honeycombPos.x, 0.85f, honeycombPos.z);
        weaponObject.transform.rotation = Quaternion.Euler(_weaponsRotation[(int)weapon.WeaponType]);
        weaponObject.transform.parent = _weaponsParent;

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
        result = Pool.FirstOrDefault(p => p.activeSelf == false);
        return result != null;
    }

    public void DeleteWeaponInRoundLayer(int round)
    {
        for (int i = 0; i < Pool.Count; i++)
        {
            Weapon weapon = Pool[i].GetComponent<Weapon>();
            if (weapon.CurrentLayerWhereImStay != round) continue;
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
