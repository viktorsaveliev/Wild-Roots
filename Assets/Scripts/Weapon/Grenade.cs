using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class Grenade : Weapon, IExplodable
{
    [SerializeField] private RadiusIndicator _radiusIndicator;
    [SerializeField] private ParticleSystem _activeFX;

    public override void Init(CharacterWeapon character)
    {
        base.Init(character);

        Force = 1000f;
        Strength = 1;
        Radius = 2f;

        Label = "Grenade";
        LifetimeSeconds = 1.5f;       
        SetLocalPosAndRotate(new Vector3(0.106f, 0.055f, 0.03f), Quaternion.Euler(89.68f, 185.1f, -137.61f));
    }

    [PunRPC]
    public override void Shoot(Vector3 target, Vector3 currentPos, Quaternion currentRotate, bool isABot = false)
    {
        base.Shoot(target, currentPos, currentRotate);
        GetComponent<Collider>().isTrigger = false;

        Throw(target, isABot);
        _radiusIndicator.Show(Radius, LifetimeSeconds/2);

        CharacterOwner.DeleteWeapon(false);
        _activeFX.Play();

        EventBus.OnPlayerShoot?.Invoke(transform, LifetimeSeconds);
    }

    [PunRPC]
    public void Explode(int[] viewID, Vector3 position, float force)
    {
        for (int i = 0; i < viewID.Length; i++)
        {
            if (viewID[i] == -1) continue;
            if(PhotonView.Find(viewID[i]).TryGetComponent<TakeImpulse>(out var takeImpulse))
            {
                takeImpulse.SetImpulse(force, position, 1, Owner, -1);
            }
        }
    }

    protected override void OnLifeTimeEnded()
    {
        base.OnLifeTimeEnded();
        EventBus.OnWeaponExploded?.Invoke(gameObject);

        if (PhotonNetwork.IsMasterClient)
        {
            GameData gameData = new();
            gameData.CallMethod<Grenade>(nameof(Explode), PhotonView, RpcTarget.All, GetTargetsInRadius(transform.position, Radius), transform.position, Force);
        }

        _activeFX.Stop();
        PlayAttackFX();
        _radiusIndicator.Hide();

        AudioSource.PlayClipAtPoint(AudioFX[(int)AudioType.Explode], transform.position, 2f);

        transform.DOScale(0.2f, 0.2f).OnComplete(() => 
            Invoke(nameof(DisableWeapon), 1f));
    }

    /*            if (PhotonNetwork.OfflineMode == false)
            {
                PhotonView.RPC(nameof(Explode), RpcTarget.All, GetTargetsInRadius(transform.position, Radius), transform.position, Force);
            }
            else
            {
                Explode(GetTargetsInRadius(transform.position, Radius), transform.position, Force);
            }*/
}