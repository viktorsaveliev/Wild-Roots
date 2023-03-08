using System.Collections;
using UnityEngine;

public class PlayersSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _defeatText;
    [SerializeField] private GameObject _buttonMenu;
    [SerializeField] private GameObject _interface;

    private void OnEnable()
    {
        EventBus.OnCharacterFall += SpawnPlayer;
    }

    private void OnDisable()
    {
        EventBus.OnCharacterFall -= SpawnPlayer;
    }

    private IEnumerator SpawnTimer(Character character, int health)
    {
        yield return new WaitForSeconds(3f);
        if(health > 0)
        {
            character.Rigidbody.velocity = Vector3.zero;
            character.Health.FromWhomDamage = null;
            character.transform.SetPositionAndRotation(new Vector3(0, 5, 0), Quaternion.Euler(0, 0, 150));
            character.gameObject.SetActive(true);
            character.TakeImpulse.SetImmunity(3f);

            if (character.Move) character.Move.SetMoveActive(false, 2);
        }
        else
        {
            if(character.PhotonView.IsMine && character.IsABot == false)
            {
                _interface.SetActive(false);
                _defeatText.SetActive(true);
                _buttonMenu.SetActive(true);
            }
        }
    }

    private void SpawnPlayer(Character character, int health)
    {
        StartCoroutine(SpawnTimer(character, health));
    }
}
