using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Wardrobe : MonoBehaviour
{
    [SerializeField] private GameObject _skinItemPrefab;
    [SerializeField] private GameObject _loading;
    [SerializeField] private Transform _content;

    private Coroutine _loadSkins;

    private void Start()
    {
        if (_loadSkins != null) return;

        StringBus stringBus = new();
        bool isNeedUpdate = PlayerPrefs.GetInt(stringBus.NeedUpdateWardrobe) == 1;
        if (isNeedUpdate)
        {
            PlayerPrefs.DeleteKey(stringBus.NeedUpdateWardrobe);
        }

        _loading.SetActive(true);
        _loadSkins = StartCoroutine(LoadMySkins());
    }

    private void OnEnable()
    {
        if (_loadSkins != null) return;

        StringBus stringBus = new();
        bool isNeedUpdate = PlayerPrefs.GetInt(stringBus.NeedUpdateWardrobe) == 1;
        if(isNeedUpdate)
        {
            SkinItem[] skinItems = _content.GetComponentsInChildren<SkinItem>(); // FindObjectsOfType<SkinItem>();
            foreach (SkinItem child in skinItems)
            {
                Destroy(child.gameObject);
            }

            _loading.SetActive(true);
            _loadSkins = StartCoroutine(LoadMySkins());
            PlayerPrefs.DeleteKey(stringBus.NeedUpdateWardrobe);
        }
    }

    private IEnumerator LoadMySkins()
    {
        yield return StartCoroutine(AddStandartSkin());

        StringBus stringBus = new();
        if(PlayerPrefs.GetInt(stringBus.IsGuest) == 1)
        {
            _loading.SetActive(false);
            Notice.Simple(NoticeDialog.Message.Simple_NeedLogin, false);
            yield break;
        }

        WWWForm form = new();
        form.AddField("ID", PlayerPrefs.GetInt(stringBus.UserID));
        using UnityWebRequest request = UnityWebRequest.Post(stringBus.GameDomain + "get_my_skins_id.php", form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
        else
        {
            string response = request.downloadHandler.text;

            if (response.Equals("false") == false)
            {
                int[] skinIDs = JsonConvert.DeserializeObject<int[]>(request.downloadHandler.text);
                foreach (int id in skinIDs)
                {
                    StartCoroutine(GetSkinData(id));
                }
            }
        }
        _loadSkins = null;
        _loading.SetActive(false);
    }

    private IEnumerator GetSkinData(int id)
    {
        StringBus stringBus = new();

        WWWForm form = new();
        form.AddField("skinID", id);
        using UnityWebRequest request = UnityWebRequest.Post(stringBus.GameDomain + "get_skin_data.php", form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
        else
        {
            string jsonString = request.downloadHandler.text;
            List<SkinData> skins = JsonConvert.DeserializeObject<List<SkinData>>(jsonString);

            foreach (SkinData skin in skins)
            {
                SkinItem skinItem = Instantiate(_skinItemPrefab, _content).GetComponent<SkinItem>();
                skinItem.SetInfo(skin.id, skin.name, (ItemData.Rarity)skin.rarity);
                skinItem.UpdateUI();
                yield return Assets.DownloadSkin(skin.id, skin.url_fbx);

                GameObject prefab = Assets.GetLoadedSkin[skin.id];
                skinItem.SetObject(prefab);

                if (Assets.GetLoadedSkinIcon.ContainsKey(skin.id))
                {
                    skinItem.SetIcon(Assets.GetLoadedSkinIcon[skin.id]);
                }

                skinItem.transform.DOScale(1, 0.3f);
            }
        }
    }

    private IEnumerator AddStandartSkin()
    {
        int skinID = 1;
        string skinName = "Loafer";
        string url = "https://www.wildroots.fun/public/skins/loafer";

        SkinItem skinItem = Instantiate(_skinItemPrefab, _content).GetComponent<SkinItem>();
        skinItem.SetInfo(skinID, skinName, ItemData.Rarity.Regular);
        skinItem.UpdateUI();

        yield return StartCoroutine(Assets.DownloadSkin(skinID, url));

        if (Assets.GetLoadedSkinIcon.ContainsKey(skinID))
        {
            skinItem.SetIcon(Assets.GetLoadedSkinIcon[skinID]);
        }

        GameObject prefab = Assets.GetLoadedSkin[skinID];
        skinItem.SetObject(prefab);

        skinItem.transform.DOScale(1, 0.5f);
    }
}
