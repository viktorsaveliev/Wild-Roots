using UnityEngine;

public class PlayerData : MonoBehaviour
{
    private static PlayerDataHandler _playerProgress;

    

    private void Start()
    {
        if (!LoadingShower.IsCreated) _playerProgress = GetComponent<PlayerDataHandler>();
    }

    public static int GetExp() => _playerProgress.Exp;
    public static int GetLevel() => _playerProgress.Level;
    public static int GetWins() => _playerProgress.Wins;
    public static int GetWinsToday() => _playerProgress.WinsToday;
    public static int GetWatchedAds() => _playerProgress.WatchedAds;
    public static string GetNickname() => _playerProgress.Nickname;
    public static int GetSkinID() => _playerProgress.SkinID;

    public static int GetDroppedPlayersCount => _playerProgress.DroppedPlayersCount;
    public static void SetDroppedPlayersCount(int value)
    {
        _playerProgress.DroppedPlayersCount = value;
    }
    public static void AddDroppedPlayersCount()
    {
        _playerProgress.DroppedPlayersCount++;
    }

    public static void UpdateSkinID(int id) => _playerProgress.UpdateSkinID(id);
    public static void PayAds(int value) => _playerProgress.PayAds(value);
    public static void ViewedAds() => _playerProgress.ViewedAds();
    public static void GiveExp(int exp) => _playerProgress.GiveExp(exp);
    public static void GiveWinnerAward() => _playerProgress.GiveWinnerAward();
    public static void Update(string nickname, int level, int exp, int wins, int winsToday, int watchedAds, int skinID) => _playerProgress.UpdateData(nickname, level, exp, wins, winsToday, watchedAds, skinID);

}
