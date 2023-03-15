using CrazyGames;
using UnityEngine;

public class AdsForCoins : MonoBehaviour
{
    public void StartAdsForReward()
    {
        EventBus.OnPlayerClickUI?.Invoke(0);
        CrazyAds.Instance.beginAdBreakRewarded(GiveReward, ErrorReward);
    }

    private void GiveReward()
    {
        Coins.Give(CoinsHandler.GiveReason.Ads);
        EventBus.OnPlayerClickUI?.Invoke(4);
    }

    private void ErrorReward()
    {
        Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
    }
}
