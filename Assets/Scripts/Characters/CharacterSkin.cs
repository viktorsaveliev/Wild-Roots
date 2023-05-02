using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterSkin : MonoBehaviour
{
    [SerializeField] private GameObject _currentModel;

    private Animator _animator;
    private PhotonView _photonView;

    private int _skinID;
    public int GetSkinID => _skinID;

    //private int _randomSkinID = -1;
    [HideInInspector] public bool IsABot;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _photonView = GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        EventBus.OnPlayerLogged += SetSkinForGuest;
        //EventBus.OnPlayerStartSearchMatch += UpdateForOthers;
        EventBus.OnPlayerNeedChangeSkin += Change;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerLogged -= SetSkinForGuest;
        //EventBus.OnPlayerStartSearchMatch -= UpdateForOthers;
        EventBus.OnPlayerNeedChangeSkin -= Change;
    }

    public void UpdateForAll()
    {
        if (_photonView == null)
        {
            _photonView = GetComponent<PhotonView>();
        }
        if (_photonView.IsMine == false) return;

        _photonView.RPC(nameof(UpdateMySkinForPlayers), RpcTarget.All, PlayerData.GetSkinID());
    }

    public void UpdateForOthers()
    {
        if (_photonView == null)
        {
            _photonView = GetComponent<PhotonView>();
        }
        if (_photonView.IsMine == false) return;

        StringBus stringBus = new();
        _photonView.RPC(nameof(UpdateMySkinForPlayers), RpcTarget.Others, PlayerData.GetSkinID());
    }

    [PunRPC]
    public void UpdateMySkinForPlayers(int skinID)
    {
        Change(skinID, true);
    }

    public IEnumerator ChangeToRandom()
    {
        //yield return StartCoroutine(Assets.GetRandomSkin(SetValue));
        int randomSkin = Random.Range(1, 8);
        //if (_randomSkinID != -1)
        //{
            yield return ChangeSkin(randomSkin, true);
            //_randomSkinID = -1;
        //}
        //else
        //{
           // Notice.Dialog(NoticeDialog.Message.ConnectionError);
        //}
    }
    
    /*private void SetValue(int value)
    {
        _randomSkinID = value;
    }*/

    private void SetSkinForGuest()
    {
        StringBus stringBus = new();
        bool isGuest = PlayerPrefs.GetInt(stringBus.IsGuest) == 1;

        if(isGuest)
        {
            Change(1, true); // default skin
        }
    }

    private IEnumerator Load(int id)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("id", id);
        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + "get_skin_url.php", form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string url = www.downloadHandler.text;
            yield return StartCoroutine(Assets.DownloadSkin(id, url));
        }
        else
        {
            Notice.Dialog(www.error);
        }
    }

    public void Change(int id) => StartCoroutine(ChangeSkin(id));
    public void Change(int id, bool updateAnim) => StartCoroutine(ChangeSkin(id, updateAnim));

    private IEnumerator ChangeSkin(int id, bool updateAnim = false)
    {
        if (id == 0)
        {
            id = 1;
            Notice.Dialog("Load data error #021");
        }

        if (Assets.GetLoadedSkin.ContainsKey(id) == false) yield return Load(id);
        
        if (_currentModel != null)
        {
            Destroy(_currentModel);
        }

        _skinID = id;
        _currentModel = Instantiate(Assets.GetLoadedSkin[id], transform);
        _currentModel.name = "Model";

        if (_photonView == null)
        {
            _photonView = GetComponent<PhotonView>();
        }

        if (IsABot == false && _photonView.IsMine)
        {
            PlayerData.UpdateSkinID(_skinID);
        }
        
        if (updateAnim)
        {
            Invoke(nameof(UpdateAnimation), 0.3f);
            gameObject.SetActive(false);
        }
    }

    private void UpdateAnimation()
    {
        gameObject.SetActive(true);
        LoadingUI.Hide();
    }
}
