using UnityEngine;

public class PlayerDataHandler : MonoBehaviour
{
    public string Nickname { get; private set; }
    public int Level { get; private set; }
    public int Exp { get; private set; }
    public int Wins { get; private set; }
    public int WinsToday { get; private set; }
    public int WatchedAds { get; private set; }
    public int SkinID { get; private set; }

    public int DroppedPlayersCount;

    public void UpdateData(string nickname, int level, int exp, int wins, int winsToday, int watchedAds, int skinID)
    {
        Nickname = nickname;
        Level = level;
        Exp = exp;
        Wins = wins;
        WinsToday = winsToday;
        WatchedAds = watchedAds;
        SkinID = skinID;

        DroppedPlayersCount = 0;
    }

    public void UpdateSkinID(int id)
    {
        SkinID = id;
    }

    public void ViewedAds()
    {
        WatchedAds++;

        SaveData.Instance.AdsCount();
        EventBus.OnPlayerViewedAds?.Invoke();
    }

    public void PayAds(int value)
    {
        WatchedAds -= value;
        if(WatchedAds < 0)
        {
            WatchedAds = 0;
            print("Error #003: Watched ads have wrong value!");
        }

        StringBus stringBus = new();
        int id = PlayerPrefs.GetInt(stringBus.UserID);
        if (id > 0)
        {
            SaveData.Instance.SaveProgress(id);
        }
    }

    public void GiveExp(int exp, bool winner = false)
    {
        StringBus stringBus = new();

        Exp += exp;
        if (Exp >= (Level * 3 * 100))
        {
            Level++;
            Exp = 0;
        }
        if (winner)
        {
            Wins++;
            WinsToday++;
        }

        int id = PlayerPrefs.GetInt(stringBus.UserID);
        if (id > 0)
        {
            SaveData.Instance.SaveProgress(id);
        }

        //StartCoroutine(GiveExpAsync(exp, winner));
    }

    /*private IEnumerator GiveExpAsync(int exp, bool winner = false)
    {
        StringBus stringBus = new();

        //LoadData.Instance.LoadUserData(this);
        //yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(LoadData.Instance.UpdateUserDataAsync(_currentCharacter));

        Exp += exp;
        if (Exp >= (Level * 3 * 100))
        {
            Level++;
            Exp = 0;
        }
        if(winner)
        {
            Wins++;
            WinsToday++;
        }

        print($"gived {exp} exp");

        int id = PlayerPrefs.GetInt(stringBus.UserID);
        if (id > 0)
        {
            SaveData.Instance.SaveLevelData(id);
        }
    }*/

    public void GiveWinnerAward()
    {
        GiveExp(100, true);
        if (DroppedPlayersCount > 0)
        {
            Coins.Give(CoinsHandler.GiveReason.ForKiller);
        }
    }
}
