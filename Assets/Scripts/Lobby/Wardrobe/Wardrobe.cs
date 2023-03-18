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

    private LoadAssets _loadAssets;

    private void Start()
    {
        _loading.SetActive(true);
        _loadAssets = FindObjectOfType<LoadAssets>();
        StartCoroutine(LoadMySkins());
    }

    private IEnumerator LoadMySkins()
    {
        StartCoroutine(AddStandartSkin());

        StringBus stringBus = new();
        WWWForm form = new();
        form.AddField("ID", PlayerPrefs.GetInt(stringBus.UserID));
        using UnityWebRequest request = UnityWebRequest.Post(stringBus.GameDomain + "get_my_skins_id.php", form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
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
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
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
                yield return _loadAssets.DownloadSkin(skin.id, skin.url_fbx);

                GameObject prefab = _loadAssets.GetLoadedSkin[skin.id];
                skinItem.SetObject(prefab);
            }
        }
    }

    private IEnumerator AddStandartSkin()
    {
        int skinID = 1;
        string skinName = "Loafer";
        string url = "https://www.wildroots.fun/public/skins/firstskin.unity3d";

        SkinItem skinItem = Instantiate(_skinItemPrefab, _content).GetComponent<SkinItem>();
        skinItem.SetInfo(skinID, skinName, ItemData.Rarity.Regular);
        skinItem.UpdateUI();

        yield return StartCoroutine(_loadAssets.DownloadSkin(skinID, url));

        GameObject prefab = _loadAssets.GetLoadedSkin[skinID];
        skinItem.SetObject(prefab);
    }
}
