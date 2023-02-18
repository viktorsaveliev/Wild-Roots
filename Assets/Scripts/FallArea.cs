using UnityEngine;

public class FallArea : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        PlayerInfo player = other.GetComponent<PlayerInfo>();
        if(player)
        {
            player.Health.PlayerFell();
        }
    }
}
