using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DG.Tweening;

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

    public IEnumerator BuyNewSkin(int userID, int skinID, int price, bool forAds)
    {
        WWWForm form = new();
        form.AddField("userID", userID);
        form.AddField("skinID", skinID);

        StringBus stringBus = new();
        using UnityWebRequest request = UnityWebRequest.Post(stringBus.GameDomain + "do_you_have_this_skin.php", form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
        else
        {
            bool isHave = bool.Parse(request.downloadHandler.text);
            if (isHave)
            {
                Notice.Simple(NoticeDialog.Message.Simple_YouHaveThisSkin, false);
            }
            else
            {
                StartCoroutine(SavePurchasedSkin(userID, skinID, price, forAds));
            }
        }
    }

    public IEnumerator SavePurchasedSkin(int userID, int skinID, int price, bool forAds)
    {
        WWWForm form = new();
        form.AddField("userID", userID);
        form.AddField("skinID", skinID);

        StringBus stringBus = new();
        using UnityWebRequest request = UnityWebRequest.Post(stringBus.GameDomain + "buy_skin.php", form);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
        else
        {
            bool success = bool.Parse(request.downloadHandler.text);
            if(success)
            {
                if (forAds)
                {
                    PlayerData.PayAds(price);
                }
                else
                {
                    Coins.Pay(price);
                }
                PlayerPrefs.SetInt(stringBus.NeedUpdateWardrobe, 1);
                EventBus.OnPlayerClickUI?.Invoke(4);
                EventBus.OnPlayerBuyNewSkin?.Invoke();
                Notice.Simple(NoticeDialog.Message.Simple_Purchase, true);
                SaveData.Instance.Stats(SaveData.Statistics.BuyNewSkin);
            }
            else
            {
                Notice.Dialog(NoticeDialog.Message.ConnectionError);
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
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
        else
        {
            string jsonString = request.downloadHandler.text;
            List<SkinData> skins = JsonConvert.DeserializeObject<List<SkinData>>(jsonString);
            skins.Sort((a, b) => a.rarity.CompareTo(b.rarity));

            foreach (SkinData skin in skins)
            {
                ShopItem shopItem = Instantiate(_itemPrefab, _content).GetComponent<ShopItem>();
                shopItem.SetLinks(this, _confirmMenu);
                shopItem.SetInfo(skin.id, skin.name, (ItemData.Rarity)skin.rarity);

                if(skin.price_ads > 0)
                {
                    shopItem.SetPrice(skin.price_ads, true);
                }
                else
                {
                    shopItem.SetPrice(skin.price, false);
                }

                yield return _loadAssets.DownloadSkin(skin.id, skin.url_fbx);

                GameObject prefab = _loadAssets.GetLoadedSkin[skin.id];
                shopItem.SetObject(prefab);

                if (Assets.GetLoadedSkinIcon.ContainsKey(skin.id))
                {
                    shopItem.SetIcon(Assets.GetLoadedSkinIcon[skin.id]);
                }

                shopItem.transform.DOScale(1, 0.5f);
            }
        }
        _loading.SetActive(false);
    }
}
