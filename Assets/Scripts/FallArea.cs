using UnityEngine;

public class FallArea : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        Character character = other.GetComponent<Character>();
        if(character)
        {
            character.Health.PlayerFell();
        }
    }
}
