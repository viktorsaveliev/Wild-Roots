using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class CustomizeItem : MonoBehaviour
{
    [SerializeField] private Text _nameText;
    [SerializeField] private LocalizeStringEvent _rarityText;
    [SerializeField] private Image _itemIcon;
    [SerializeField] private Image _rarityBG;
    [SerializeField] private GameObject _item;
    public GameObject GetItem => _item;

    private int _itemID;
    public int GetItemID => _itemID;

    private string _name;
    public string GetName => _name;

    private ItemData.Rarity _rarity;
    public ItemData.Rarity GetRarity => _rarity;

    private ItemData _itemData;

    private void Awake()
    {
        _itemData = FindObjectOfType<ItemData>();
    }

    public virtual void SetInfo(int id, string name, ItemData.Rarity rarity)
    {
        _itemID = id;
        _name = name;
        _rarity = rarity;
    }

    public virtual void UpdateUI()
    {
        _nameText.text = _name;
        _rarityText.SetEntry(_itemData.RarityStringKey[(int)_rarity]);
        _rarityText.GetComponent<Text>().color = _itemData.RarityTextColor[(int)_rarity];

        _rarityBG.sprite = _itemData.GetRarityBackground[(int)_rarity];

    }

    public void SetIcon(Sprite sprite)
    {
        if (sprite == null) return;
        _itemIcon.sprite = sprite;
        _itemIcon.SetNativeSize();
    }

    public virtual void SetObject(GameObject item)
    {
        if (item == null) return;
        _item = item;
    }

    public virtual void Select()
    {

    }
}
