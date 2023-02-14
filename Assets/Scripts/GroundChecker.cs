using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerInfo player = other.gameObject.GetComponent<PlayerInfo>();
        if (player)
        {
            //player.PlayerMove.IsGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerInfo player = other.gameObject.GetComponent<PlayerInfo>();
        if (player)
        {
            player.Move.IsGrounded = false;
            //player.PlayerMove.SetMoveActive(false);
        }
    }
}
