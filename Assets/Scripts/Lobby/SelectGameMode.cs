using UnityEngine;

public class SelectGameMode : MonoBehaviour
{
    [SerializeField] private JoinRoomHandler _joinRoom;
    [SerializeField] private GameObject _selectModeUI;

    public enum GameMode
    {
        PvP,
        Deathmatch
    }

    public void ShowSelector()
    {
        _selectModeUI.SetActive(true);
        EventBus.OnPlayerClickUI?.Invoke(0);
    }

    public void SelectModeButton(int modeID)
    {
        _joinRoom.SelectMode(modeID);
        _selectModeUI.SetActive(false);
    }
}
