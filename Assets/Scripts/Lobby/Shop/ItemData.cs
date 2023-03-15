using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ItemData : MonoBehaviour
{
    //private const int MAX_SKINS_IN_SHOP = 3;

    [SerializeField] private GameObject _itemPrefab;
    [SerializeField] private Transform _content;

    [SerializeField] private Sprite[] _backgroundForRarity;
    public Sprite[] GetRarityBackground => _backgroundForRarity;

    public readonly string[] RarityStringKey =
    {
        "Rarity_Regular",
        "Rarity_Rare",
        "Rarity_Legendary"
    };

    public enum Rarity
    {
        Regular,
        Rare,
        Legendary
    }

    public readonly Color[] RarityTextColor =
    {
        new Color(0.75f, 0.75f, 0.75f),
        new Color(0, 0.79f, 1f),
        new Color(1f, 0.83f, 0f)
    };

    private void Start()
    {
        StartCoroutine(LoadSkinsFromServer());
    }

    private IEnumerator LoadSkinsFromServer()
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
            // Parse the JSON data into a list of skin objects
            string jsonString = request.downloadHandler.text;
            List<SkinData> skins = JsonConvert.DeserializeObject<List<SkinData>>(jsonString);

            // Do something with the skin data, e.g. spawn skin prefabs in the store
            foreach (SkinData skin in skins)
            {
                ShopItem shopItem = Instantiate(_itemPrefab, _content).GetComponent<ShopItem>();
                shopItem.SetItemData(this);
                shopItem.SetItemInfo(skin.name, skin.price, (Rarity)skin.rarity, null);
            }
        }
    }
}

[System.Serializable]
public class SkinData
{
    public int id;
    public string name;
    public string url_fbx;
    public string url_icon;
    public int price;
    public int rarity;
}
