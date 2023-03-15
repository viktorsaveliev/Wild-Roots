using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _priceText;
    [SerializeField] private LocalizeStringEvent _rarityText;
    [SerializeField] private Image _itemImage;
    [SerializeField] private Image _rarityBG;

    private ItemData _itemData;

    private string _name;
    private int _price;
    private ItemData.Rarity _rarity;

    public void SetItemInfo(string name, int price, ItemData.Rarity rarity, Sprite sprite)
    {
        _name = name;
        _price = price;
        _rarity = rarity;

        UpdateItemInfo(sprite);
    }

    public void SetItemData(ItemData itemData)
    {
        _itemData = itemData;
    }

    private void UpdateItemInfo(Sprite sprite)
    {
        _nameText.text = _name;
        _priceText.text = _price.ToString();

        _rarityText.SetEntry(_itemData.RarityStringKey[(int)_rarity]);
        _rarityText.GetComponent<Text>().color = _itemData.RarityTextColor[(int)_rarity];

        _rarityBG.sprite = _itemData.GetRarityBackground[(int)_rarity];
        if(sprite != null) _itemImage.sprite = sprite;
    }

    public void SelectItem()
    {
        EventBus.OnPlayerClickUI?.Invoke(2);
    }
}
