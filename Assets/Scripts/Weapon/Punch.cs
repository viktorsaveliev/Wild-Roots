using UnityEngine;
using Photon.Pun;

public class Punch : Weapon
{
    private float _antiflood;

    public override void Init(PlayerWeapon player)
    {
        base.Init(player);
        Force = 1000;
        Label = "Punch";
        LifetimeSeconds = -1;
        Radius = 1f;
    }

    [PunRPC]
    public override void Shoot(Vector2 target, Vector3 currentPos, Quaternion currentRotate)
    {
        if (_antiflood >= Time.time) return;

        base.Shoot(target, currentPos, currentRotate);

        if (PhotonNetwork.IsMasterClient && !PhotonNetwork.OfflineMode)
        {
            PhotonViewObject.RPC(nameof(Punches), RpcTarget.All, GetPlayersInRadius(Player.transform.position, Radius), transform.position, Force);
        }
        else
        {
            Punches(GetPlayersInRadius(Player.transform.position, Radius), transform.position, Force);
        }

        PlayAttackFX();
    }

    [PunRPC]
    public void Punches(int[] viewID, Vector3 position, float force)
    {
        TakeImpulse[] players = new TakeImpulse[viewID.Length];

        for (int i = 0; i < viewID.Length; i++)
        {
            players[i] = PhotonView.Find(viewID[i]).GetComponent<TakeImpulse>();
        }
        foreach (TakeImpulse player in players)
        {
            PlayerWeapon characterWeapon = player.GetComponent<PlayerWeapon>();
            if (characterWeapon != null && characterWeapon == Player) continue;

            var direction = (player.transform.position - Player.transform.position).normalized;
            if (Vector3.Angle(Player.transform.forward, direction) < 60f / 2f) // ViewAngle
            {
                player.SetImpulse(force, position, this, false);
            }
        }
        _antiflood = Time.time + 1f;
    }
}