using System;
using UnityEngine;

public static class EventBus
{
    public static Action OnPlayerDisconnected;
    public static Action<GameObject> OnWeaponExploded;

    public static Action<PlayerInfo, bool> OnPlayerChangedMoveState;
    public static Action<Transform, float> OnPlayerShoot;
    public static Action<GameObject> OnWeaponSpawned;
    public static Action<PlayerInfo> OnPlayerTakeDamage;
    public static Action<PlayerInfo, int> OnPlayerFall;
    public static Action<PlayerInfo> OnPlayerTakeAim;
    public static Action OnPlayerTakeWeapon;

    public static Action OnPlayerWin;
    public static Action OnPlayerLose;
    public static Action OnPlayerStartSearchMatch;

    public static Action<int> OnMatchEnded;

    public static Action<int> OnPlayerClickUI;

    public static Action OnSetTutorialTaskForPlayer;
    public static Action OnPlayerEndTutorial;
}
