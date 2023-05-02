using UnityEngine;

public class GroundChecker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Character player = other.gameObject.GetComponent<Character>();
        if (player)
        {
            //player.PlayerMove.IsGrounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Character player = other.gameObject.GetComponent<Character>();
        if (player)
        {
            player.Move.IsGrounded = false;
            //player.PlayerMove.SetMoveActive(false);
        }
    }
}
