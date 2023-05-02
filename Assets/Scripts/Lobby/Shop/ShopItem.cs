using UnityEngine;
using UnityEngine.UI;

public class ShopItem : CustomizeItem, IConfirmMenuAction
{
    [SerializeField] private Text _priceText;
    [SerializeField] private Image _priceIcon;

    [SerializeField] private Sprite[] _priceIconSprites;
    public Sprite[] GetSpriteForPriceIcon => _priceIconSprites;

    private ShopHandler _shop;
    private ConfirmMenu _confirmMenu;

    private int _price;
    public int GetPrice => _price;
    
    private int _priceAds;
    public int GetPriceAds => _priceAds;

    private void OnEnable()
    {
        UpdateUI();

        EventBus.OnPlayerBuyNewSkin += UpdateUI;
        EventBus.OnPlayerViewedAds += UpdateUI;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerBuyNewSkin -= UpdateUI;
        EventBus.OnPlayerViewedAds -= UpdateUI;
    }

    public void SetLinks(ShopHandler shop, ConfirmMenu confirmMenu)
    {
        _shop = shop;
        _confirmMenu = confirmMenu;
    }

    public void SetPrice(int value, bool forAds)
    {
        if(forAds)
        {
            _priceAds = value;
        }
        else
        {
            _price = value;
        }
        
        UpdateUI();
    }

    public override void UpdateUI()
    {
        base.UpdateUI();
        if(_priceAds > 0)
        {
            _priceIcon.sprite = _priceIconSprites[1];
            _priceText.text = $"{PlayerData.GetWatchedAds()} / {_priceAds}";
        }
        else
        {
            _priceIcon.sprite = _priceIconSprites[0];
            _priceText.text = _price.ToString();
        }
    }

    public override void SetObject(GameObject skin)
    {
        base.SetObject(skin);
        GetItem.name = "Model";
    }

    public override void Select()
    {
        _confirmMenu.Show3D(this, _priceAds > 0);
    }

    public void Action()
    {
        if((_priceAds == 0 && Coins.GetValue() < _price) || (_priceAds > 0 && PlayerData.GetWatchedAds() < _priceAds))
        {
            Notice.Simple(NoticeDialog.Message.Simple_NotMoney, false);
            EventBus.OnPlayerClickUI?.Invoke(3);
            return;
        }

        StringBus stringBus = new();
        if (PlayerPrefs.GetInt(stringBus.IsGuest) == 1)
        {
            Notice.Simple(NoticeDialog.Message.Simple_NeedLogin, false);
            return;
        }

        int userID = PlayerPrefs.GetInt(stringBus.UserID);
        StartCoroutine(_shop.BuyNewSkin(userID, GetItemID, _priceAds > 0 ? _priceAds : _price, _priceAds > 0));

        _confirmMenu.Hide();
    }
}
