using UnityEngine;
using UnityEngine.UI;

public class SkinItem : CustomizeItem
{
    [SerializeField] private Image _background;
    [SerializeField] private Sprite[] _variantsBG;
    [SerializeField] private Image _selectMark;

    private enum Status
    {
        NotSelected,
        Selected
    }

    private Status _status;

    private void Start()
    {
        StringBus stringBus = new();
        bool isSelectedSkin = PlayerPrefs.GetInt(stringBus.SkinID) == GetItemID;
        UpdateSelectUI(isSelectedSkin);
    }

    private void OnEnable()
    {
        StringBus stringBus = new();
        bool isSelectedSkin = PlayerPrefs.GetInt(stringBus.SkinID) == GetItemID;
        UpdateSelectUI(isSelectedSkin);

        EventBus.OnPlayerChangeSkin += UnSelect;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerChangeSkin -= UnSelect;
    }

    public override void Select()
    {
        base.Select();

        StringBus stringBus = new();
        int currentSkin = PlayerPrefs.GetInt(stringBus.SkinID);
        if (currentSkin == GetItemID)
        {
            EventBus.OnPlayerClickUI?.Invoke(1);
            return;
        }

        UpdateSelectUI(true);

        EventBus.OnPlayerChangeSkin?.Invoke(GetItemID);
        EventBus.OnPlayerClickUI?.Invoke(1);
    }

    public void UpdateSelectUI(bool on)
    {
        if (on)
        {
            _selectMark.enabled = true;
            _status = Status.Selected;
            _background.sprite = _variantsBG[(int)Status.Selected];
        }
        else
        {
            _selectMark.enabled = false;
            _status = Status.NotSelected;
            _background.sprite = _variantsBG[(int)Status.NotSelected];
        }
        _background.SetNativeSize();
    }

    private void UnSelect(int newSkinID)
    {
        if(_status == Status.Selected && GetItemID != newSkinID)
        {
            UpdateSelectUI(false);
        }
    }
}
