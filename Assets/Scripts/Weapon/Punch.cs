using UnityEngine;
using Photon.Pun;

public class Punch : Weapon
{
    private const int MAX_CHARACTERS_PUSHING = 2;
    private int[] _closedCharactersViewID = new int[MAX_CHARACTERS_PUSHING] { -1, -1 };
    private float _antiflood;

    public override void Init(CharacterWeapon character)
    {
        base.Init(character);
        Force = 800;
        Label = "Punch";
        LifetimeSeconds = -1;
        Radius = 1f;
    }

    private void OnTriggerEnter(Collider other)
    {
        Character target = other.GetComponent<Character>();
        if(target)
        {
            SetCharacterInArray(target);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Character target = other.GetComponent<Character>();
        if (target)
        {
            RemoveCharacterInArray(target);
        }
    }

    private void SetCharacterInArray(Character target)
    {
        for (int i = 0; i < _closedCharactersViewID.Length; i++)
        {
            if (_closedCharactersViewID[i] != -1) continue;
            _closedCharactersViewID[i] = target.PhotonView.ViewID;
            break;
        }
    }

    private void RemoveCharacterInArray(Character target)
    {
        for (int i = 0; i < _closedCharactersViewID.Length; i++)
        {
            if (_closedCharactersViewID[i] != target.PhotonView.ViewID) continue;
            _closedCharactersViewID[i] = -1;
            break;
        }
    }

    [PunRPC]
    public override void Shoot(Vector3 target, Vector3 currentPos, Quaternion currentRotate, bool isABot = false)
    {
        if (_antiflood >= Time.time) return;

        base.Shoot(target, currentPos, currentRotate, isABot);

        if (!PhotonNetwork.OfflineMode)
        {
            PhotonView.RPC(nameof(Punches), RpcTarget.All, _closedCharactersViewID, CharacterOwner.transform.position, Force);
        }
        else
        {
            Punches(_closedCharactersViewID, CharacterOwner.transform.position, Force);
        }

        PlayAttackFX();
    }

    [PunRPC]
    public void Punches(int[] viewID, Vector3 position, float force)
    {
        TakeImpulse[] players = new TakeImpulse[viewID.Length];

        for (int i = 0; i < viewID.Length; i++)
        {
            if (viewID[i] == -1) continue;
            players[i] = PhotonView.Find(viewID[i]).GetComponent<TakeImpulse>();
        }

        foreach (TakeImpulse player in players)
        {
            if (player == null) continue; //  || (character.Weapon == CharacterOwner && !character.IsABot)
            if(player.TryGetComponent<Character>(out var _) == false) continue;
            
            player.SetImpulse(force, position, 0, Owner, 1);
        }
        _antiflood = Time.time + 1f;
    }
}