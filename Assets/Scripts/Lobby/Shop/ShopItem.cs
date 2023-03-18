using UnityEngine;
using UnityEngine.UI;

public class ShopItem : CustomizeItem, IConfirmMenuAction
{
    [SerializeField] private Text _priceText;

    private ShopHandler _shop;
    private ConfirmMenu _confirmMenu;

    private int _price;
    public int GetPrice => _price;

    public void SetLinks(ShopHandler shop, ConfirmMenu confirmMenu)
    {
        _shop = shop;
        _confirmMenu = confirmMenu;
    }

    public void SetPrice(int value)
    {
        _price = value;
        UpdateUI();
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        _priceText.text = _price.ToString();
    }

    public override void SetObject(GameObject skin)
    {
        base.SetObject(skin);
        GetItem.name = "Model";
    }

    public override void Select()
    {
        _confirmMenu.Show3D(this);
    }

    public void Action()
    {
        if(Coins.GetValue() < _price)
        {
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        StringBus stringBus = new();
        int userID = PlayerPrefs.GetInt(stringBus.UserID);
        StartCoroutine(_shop.BuyNewSkin(userID, GetItemID, _price));

        _confirmMenu.Hide();
    }
}
