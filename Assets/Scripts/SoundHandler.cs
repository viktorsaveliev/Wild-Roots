using UnityEngine;

public class SoundHandler : MonoBehaviour
{
    [Header("Weapons")]
    [SerializeField] private AudioClip _weaponSpawn;

    private void OnEnable()
    {
        EventBus.OnWeaponSpawned += WeaponSpawn;
    }

    private void OnDisable()
    {
        EventBus.OnWeaponSpawned -= WeaponSpawn;
    }

    private void WeaponSpawn(GameObject weapon)
    {
        AudioSource.PlayClipAtPoint(_weaponSpawn, transform.position);
    }
}
