using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ShopHandler : MonoBehaviour
{
    //private const int MAX_SKINS_IN_SHOP = 3;

    [SerializeField] private GameObject _loading;
    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private ConfirmMenu _confirmMenu;
    [SerializeField] private Transform _content;

    private LoadAssets _loadAssets;

    private void Start()
    {
        _loading.SetActive(true);
        _loadAssets = FindObjectOfType<LoadAssets>();

        StartCoroutine(LoadSkinsDataFromServer());
    }

    public IEnumerator BuyNewSkin(int userID, int skinID, int price)
    {
        WWWForm form = new();
        form.AddField("userID", userID);
        form.AddField("skinID", skinID);

        StringBus stringBus = new();
        using UnityWebRequest request = UnityWebRequest.Post(stringBus.GameDomain + "do_you_have_this_skin.php", form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
        else
        {
            bool isHave = bool.Parse(request.downloadHandler.text);
            if (isHave)
            {
                print("you have this skin");
                Notice.ShowDialog(NoticeDialog.Message.BackToLobby);
            }
            else
            {
                StartCoroutine(SavePurchasedSkin(userID, skinID, price));
            }
        }
    }

    public IEnumerator SavePurchasedSkin(int userID, int skinID, int price)
    {
        WWWForm form = new();
        form.AddField("userID", userID);
        form.AddField("skinID", skinID);

        StringBus stringBus = new();
        using UnityWebRequest request = UnityWebRequest.Post(stringBus.GameDomain + "buy_skin.php", form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
        else
        {
            bool success = bool.Parse(request.downloadHandler.text);
            if(success)
            {
                Coins.Pay(price);
                EventBus.OnPlayerClickUI?.Invoke(4);
            }
            else
            {
                Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
                print(request.downloadHandler.text);
            }
        }
    }

    private IEnumerator LoadSkinsDataFromServer()
    {
        StringBus stringBus = new();
        using UnityWebRequest request = UnityWebRequest.Get(stringBus.GameDomain + "get_items_in_shop.php");

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
                ShopItem shopItem = Instantiate(_itemPrefab, _content).GetComponent<ShopItem>();
                shopItem.SetLinks(this, _confirmMenu);
                shopItem.SetInfo(skin.id, skin.name, (ItemData.Rarity)skin.rarity);
                shopItem.SetPrice(skin.price);

                yield return _loadAssets.DownloadSkin(skin.id, skin.url_fbx);

                GameObject prefab = _loadAssets.GetLoadedSkin[skin.id];
                shopItem.SetObject(prefab);
            }
        }
        _loading.SetActive(false);
    }
}
