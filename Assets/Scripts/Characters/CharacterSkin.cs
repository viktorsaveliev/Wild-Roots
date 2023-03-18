using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CharacterSkin : MonoBehaviour
{
    [SerializeField] private GameObject _currentModel;

    private LoadAssets _loader;
    private Animator _animator;
    private PhotonView _photonView;

    private int _skinID;
    public int GetSkinID => _skinID;

    private void Awake()
    {
        _loader = FindObjectOfType<LoadAssets>();
        _animator = GetComponent<Animator>();
        _photonView = GetComponent<PhotonView>();
    }

    private void OnEnable()
    {
        EventBus.OnPlayerGetUserIDFromDB += SetSkinForGuest;
        EventBus.OnPlayerStartSearchMatch += UpdateForOthers;
        EventBus.OnPlayerChangeSkin += Change;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerGetUserIDFromDB -= SetSkinForGuest;
        EventBus.OnPlayerStartSearchMatch -= UpdateForOthers;
        EventBus.OnPlayerChangeSkin -= Change;
    }

    public void UpdateForAll()
    {
        StringBus stringBus = new();
        _photonView.RPC(nameof(UpdateMySkinForAllPlayers), RpcTarget.All, PlayerPrefs.GetInt(stringBus.SkinID));
    }

    public void UpdateForOthers()
    {
        StringBus stringBus = new();
        _photonView.RPC(nameof(UpdateMySkinForAllPlayers), RpcTarget.Others, PlayerPrefs.GetInt(stringBus.SkinID));
    }

    [PunRPC]
    public void UpdateMySkinForAllPlayers(int skinID)
    {
        Change(skinID);
    }

    private void SetSkinForGuest()
    {
        StringBus stringBus = new();
        bool isGuest = PlayerPrefs.GetInt(stringBus.IsGuest) == 1;

        if(isGuest)
        {
            Change(1); // default skin
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
            yield return StartCoroutine(_loader.DownloadSkin(id, url));
        }
        else
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
    }

    public void Change(int id) => StartCoroutine(ChangeSkin(id));

    private IEnumerator ChangeSkin(int id)
    {
        if (_loader.GetLoadedSkin.ContainsKey(id) == false) yield return Load(id);
        
        if (_currentModel != null)
        {
            Destroy(_currentModel);
        }

        _skinID = id;
        _currentModel = Instantiate(_loader.GetLoadedSkin[id], transform);
        _currentModel.name = "Model";

        StringBus stringBus = new();
        PlayerPrefs.SetInt(stringBus.SkinID, _skinID);
        PlayerPrefs.Save();

        //Invoke(nameof(UpdateAnimation), 0.3f);
        //gameObject.SetActive(false);
    }

    private void UpdateAnimation()
    {
        gameObject.SetActive(true);
        LoadingUI.Hide();
    }
}
