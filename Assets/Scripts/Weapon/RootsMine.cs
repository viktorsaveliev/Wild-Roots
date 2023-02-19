using UnityEngine;
using DG.Tweening;
using Photon.Pun;

public class RootsMine : Weapon, IExplodable
{
    public bool IsMineActive { get; private set; }

    public override void Init(CharacterWeapon character)
    {
        base.Init(character);

        Strength = 5;
        Radius = 1f;
        IsMineActive = false;

        Label = "Roots Mine";
        LifetimeSeconds = 15f;
        SetLocalPosAndRotate(new Vector3(-0.051f, 0.055f, 0.08f), Quaternion.Euler(97.74f, 0, 0));
    }

    [PunRPC]
    public override void Shoot(Vector2 target, Vector3 currentPos, Quaternion currentRotate)
    {
        base.Shoot(target, currentPos, currentRotate);
        GetComponent<BoxCollider>().isTrigger = false;

        Throw(target);
        CharacterOwner.DeleteWeapon(false);

        Invoke(nameof(SetMineActive), 1f);
        EventBus.OnPlayerShoot?.Invoke(transform, -1);
    }

    private void SetMineActive() => IsMineActive = true;

    [PunRPC]
    public void Explode(int[] viewID, Vector3 position, float force)
    {
        PlayAttackFX();
        AudioSource.PlayClipAtPoint(AudioFX[(int)AudioType.Explode], transform.position, 1f);

        PlayerMove[] targets = new PlayerMove[viewID.Length];

        for (int i = 0; i < viewID.Length; i++)
        {
            targets[i] = PhotonView.Find(viewID[i]).GetComponent<PlayerMove>();
        }

        foreach (PlayerMove target in targets)
        {
            target.SetMoveActive(false, Strength, true);
        }

        IsMineActive = false;
        
        transform.DOScale(0.1f, 0.2f).OnComplete(() =>
            Invoke(nameof(DisableWeapon), 1f));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (PhotonNetwork.IsMasterClient && IsMineActive)
        {
            if (collision.gameObject.TryGetComponent<TakeImpulse>(out var target))
            {
                PhotonViewObject.RPC(nameof(Explode), RpcTarget.All, GetPlayersInRadius(transform.position, Radius), transform.position, Force);
            }
        }
    }

    protected override void OnLifeTimeEnded()
    {
        base.OnLifeTimeEnded();
        DisableWeapon();
    }
}