using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

public class ConfirmMenu : MonoBehaviour
{
    [SerializeField] private Text _name;
    [SerializeField] private Text _price;
    [SerializeField] private Text _rarity;
    [SerializeField] private GameObject _skin;
    [SerializeField] private GameObject _dialog;
    [SerializeField] private Image _item2D;

    [SerializeField] private GameObject[] _buttonType;

    private ItemData _itemData;
    private GameObject _skinModel;
    private LocalizeStringEvent _rarityKey;
    private IConfirmMenuAction _action;

    private enum Type
    {
        Buy,
        Watch
    }

    private void Start()
    {
        _rarityKey = _rarity.GetComponent<LocalizeStringEvent>();
        _itemData = FindObjectOfType<ItemData>();
    }

    public void Show3D(ShopItem item)
    {
        _action = item;

        _name.text = item.GetName;
        _price.text = item.GetPrice.ToString();

        _rarityKey.enabled = true;
        _rarityKey.SetEntry(_itemData.RarityStringKey[(int)item.GetRarity]);
        _rarity.color = _itemData.RarityTextColor[(int)item.GetRarity];

        if (item.GetItem != null)
        {
            if (_skinModel != null)
            {
                Destroy(_skinModel);
            }
            _skinModel = Instantiate(item.GetItem, _skin.transform);
            _skinModel.name = "Model";
        }

        _buttonType[(int)Type.Buy].SetActive(true);
        _skin.SetActive(true);
        _dialog.SetActive(true);

        EventBus.OnPlayerClickUI?.Invoke(2);
    }

    public void Show2D(IConfirmMenuAction action, string name, Sprite sprite)
    {
        _action = action;

        _name.text = name;
        _rarityKey.enabled = false;
        _rarity.text = "+100";
        _rarity.color = _itemData.RarityTextColor[(int)ItemData.Rarity.Legendary];

        _buttonType[(int)Type.Watch].SetActive(true);
        _dialog.SetActive(true);

        _item2D.sprite = sprite;
        _item2D.gameObject.SetActive(true);

        EventBus.OnPlayerClickUI?.Invoke(2);
    }

    public void Buy()
    {
        _action.Action();
    }

    public void Hide()
    {
        EventBus.OnPlayerClickUI?.Invoke(2);

        _buttonType[(int)Type.Watch].SetActive(false);
        _buttonType[(int)Type.Buy].SetActive(false);

        _item2D.gameObject.SetActive(false);
        _skin.SetActive(false);
        _dialog.SetActive(false);
        Destroy(_skinModel);
    }
}
