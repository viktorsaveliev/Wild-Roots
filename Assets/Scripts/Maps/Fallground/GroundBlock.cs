using System.Collections;
using UnityEngine;
using DG.Tweening;

public class GroundBlock : MonoBehaviour
{
    [SerializeField] private SpawnPoint[] _spawnPointsOnBlock;

    private void Start()
    {
        gameObject.isStatic = true;
    }

    public void Fall(float delay)
    {
        for (int i = 0; i < _spawnPointsOnBlock.Length; i++)
        {
            _spawnPointsOnBlock[i].IsActive = false;
        }
        transform.DOShakeRotation(delay, 10, 10, 10, false);
        StartCoroutine(TimerToFall(delay));
    }

    private IEnumerator TimerToFall(float delay)
    {
        yield return new WaitForSeconds(delay);

        if(_spawnPointsOnBlock.Length > 0)
        {
            Weapon[] weapons = FindObjectsOfType<Weapon>();
            foreach (Weapon weapon in weapons)
            {
                for (int i = 0; i < _spawnPointsOnBlock.Length; i++)
                {
                    if (weapon.CurrentSpawnPoint != _spawnPointsOnBlock[i]) continue;
                    weapon.DisableWeapon();
                    break;
                }
            }
        }

        transform.DOMoveY(-7, 2f).OnComplete(() =>
        {
            Destroy(gameObject);
        });
    }
}
