using CrazyGames;
using UnityEngine;

public class AdsForCoins : MonoBehaviour, IConfirmMenuAction
{
    [SerializeField] private ConfirmMenu _confirmMenu;
    [SerializeField] private Sprite _icon2D;

    public void OnClick()
    {
        _confirmMenu.Show2D(this, "Coins", _icon2D);
    }

    public void Action()
    {
        _confirmMenu.Hide();
        //CrazyAds.Instance.beginAdBreak(GiveReward, ErrorReward, CrazyAdType.rewarded);
        CrazyAds.Instance.beginAdBreakRewarded(GiveReward, ErrorReward);
    }

    private void GiveReward()
    {
        Coins.Give(CoinsHandler.GiveReason.Ads);
        EventBus.OnPlayerClickUI?.Invoke(4);
    }

    private void ErrorReward()
    {
        Notice.Dialog(NoticeDialog.Message.ConnectionError);
    }
}
