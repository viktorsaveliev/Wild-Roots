using System.Collections;
using UnityEngine;

public class PlayersSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _interface;
    [SerializeField] private Vector3 _spawnPos = new(-1, 5, 0);

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
        yield return new WaitForSeconds(2f);
        if(health > 0)
        {
            character.gameObject.SetActive(true);
            character.Health.FromWhomDamage = -1;
            character.transform.SetPositionAndRotation(_spawnPos, Quaternion.Euler(0, 0, 150));

            if (character.Rigidbody != null)
            {
                character.Rigidbody.velocity = Vector3.zero;
                if (character.Rigidbody.angularVelocity == Vector3.zero)
                {
                    character.Rigidbody.AddTorque(Random.insideUnitSphere * 100f);
                }
            }

            character.TakeImpulse.SetImmunity(3f);

            if (character.Move) character.Move.SetMoveActive(false, 2);
        }
        else
        {
            if(character.PhotonView.IsMine && character.IsABot == false)
            {
                _interface.SetActive(false);
            }
        }
    }

    private void SpawnPlayer(Character character, int health)
    {
        StartCoroutine(SpawnTimer(character, health));
    }
}
