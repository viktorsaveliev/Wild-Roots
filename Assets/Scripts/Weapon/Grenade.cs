using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class Grenade : Weapon, IExplodable
{
    public override void Init(PlayerWeapon player)
    {
        base.Init(player);

        Force = 1500f;
        Strength = 1;
        Radius = 3f;

        Label = "Grenade";
        LifetimeSeconds = 1.3f;       
        SetLocalPosAndRotate(new Vector3(-0.051f, 0.055f, 0.08f), Quaternion.Euler(-80.3f, 90, -40f));
    }

    [PunRPC]
    public override void Shoot(Vector2 target, Vector3 currentPos, Quaternion currentRotate)
    {
        base.Shoot(target, currentPos, currentRotate);
        GetComponent<CapsuleCollider>().isTrigger = false;

        Throw(target);

        //SetOwnerLocal(-1);
        Player.DeleteWeapon(false);
        EventBus.OnPlayerShoot?.Invoke(transform, LifetimeSeconds);
    }

    [PunRPC]
    public void Explode(int[] viewID, Vector3 position, float force)
    {
        TakeImpulse[] players = new TakeImpulse[viewID.Length];

        for (int i = 0; i < viewID.Length; i++)
        {
            players[i] = PhotonView.Find(viewID[i]).GetComponent<TakeImpulse>();
        }

        foreach (TakeImpulse player in players)
        {
            player.SetImpulse(force, position, this);
        }

        /*Honeycomb[] honeycombs = GetCellsInRadius(transform.position, Radius / 2);
        foreach (Honeycomb honeycomb in honeycombs)
        {
            honeycomb.TakeDamage(1);
        }*/
    }

    protected override void OnLifeTimeEnded()
    {
        base.OnLifeTimeEnded();
        EventBus.OnWeaponExploded?.Invoke(gameObject);

        if (PhotonNetwork.IsMasterClient)
        {
            if(!PhotonNetwork.OfflineMode)
            {
                PhotonViewObject.RPC(nameof(Explode), RpcTarget.All, GetPlayersInRadius(transform.position, Radius), transform.position, Force);
            }
            else
            {
                Explode(GetPlayersInRadius(transform.position, Radius), transform.position, Force);
            }
        }

        PlayAttackFX();

        AudioSource.PlayClipAtPoint(AudioFX[(int)AudioType.Explode], Vector3.zero);

        transform.DOScale(0.1f, 0.2f).OnComplete(() => 
            Invoke(nameof(DisableWeapon), 1f));
    }
}
