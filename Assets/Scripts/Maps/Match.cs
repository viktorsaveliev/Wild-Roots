using UnityEngine;

public class Match : MonoBehaviour
{
    private static MatchHandler _matchStats;

    private void Awake()
    {
        if(_matchStats == null)
        {
            _matchStats = GetComponent<MatchHandler>();
        }
    }

    public static int CurrentRound => _matchStats.CurrentRound;
    public static int GetPlayersSkinID(int playerID) => _matchStats.GetPlayersSkinID[playerID];
    public static int NextMap => _matchStats.NextMap;

    public static void Init() => _matchStats.Init();
    public static void CreateBotData(int ID, int nameID, int skinID) => _matchStats.CreateBotData(ID, nameID, skinID);
    public static string GetPlayerNickname(int playerID) => _matchStats.GetPlayersNicknames[playerID];
    public static int GetPlayerCupsCount(int playerID) => _matchStats.GetPlayersCupsCount[playerID];

    public static void SetIDForCharacter(Character character) => _matchStats.SetIDForCharacter(character);
    public static void GiveWinnerCoup(int playerID, bool isABot) => _matchStats.GiveWinnerCoup(playerID, isABot);
    public static void SetNickname(int playerID, string nickname) => _matchStats.SetNickname(playerID, nickname);
    public static void StartNextRound() => _matchStats.StartNextRound();
    public static void ResetData() => _matchStats.ResetData();

}
