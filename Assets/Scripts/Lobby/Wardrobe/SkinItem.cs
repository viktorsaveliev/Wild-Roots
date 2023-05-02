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
        bool isSelectedSkin = PlayerData.GetSkinID() == GetItemID;
        UpdateSelectUI(isSelectedSkin);
    }

    private void OnEnable()
    {
        bool isSelectedSkin = PlayerData.GetSkinID() == GetItemID;
        UpdateSelectUI(isSelectedSkin);

        EventBus.OnPlayerNeedChangeSkin += UnSelect;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerNeedChangeSkin -= UnSelect;
    }

    public override void Select()
    {
        base.Select();

        if (PlayerData.GetSkinID() == GetItemID)
        {
            EventBus.OnPlayerClickUI?.Invoke(1);
            return;
        }

        UpdateSelectUI(true);

        EventBus.OnPlayerNeedChangeSkin?.Invoke(GetItemID);
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
