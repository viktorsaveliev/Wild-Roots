using System.Collections;
using UnityEngine;

public class PlayersSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _defeatText;
    [SerializeField] private GameObject _buttonMenu;
    [SerializeField] private GameObject _interface;

    private void OnEnable()
    {
        EventBus.OnPlayerFall += SpawnPlayer;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerFall -= SpawnPlayer;
    }

    private IEnumerator SpawnTimer(PlayerInfo player, int health)
    {
        yield return new WaitForSeconds(3f);
        if(health > 0)
        {
            player.Health.FromWhomDamage = null;
            player.transform.position = new Vector3(0, 5, 0);
            player.transform.rotation = Quaternion.Euler(80, 0, 95);
            player.gameObject.SetActive(true);
            player.Move.SetMoveActive(false);
        }
        else
        {
            if(player.PhotonView.IsMine)
            {
                _interface.SetActive(false);
                _defeatText.SetActive(true);
                _buttonMenu.SetActive(true);
            }
        }
    }

    private void SpawnPlayer(PlayerInfo player, int health)
    {
        StartCoroutine(SpawnTimer(player, health));
    }
}
