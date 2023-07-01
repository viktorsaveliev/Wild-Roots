using UnityEngine;

public class PlayButton : MonoBehaviour
{
    [SerializeField] private JoinRoomHandler _joinRoom;

    public void SelectModeButton()
    {
        _joinRoom.ConnectToMatch();
    }
}
