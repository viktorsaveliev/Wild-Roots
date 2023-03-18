using System;
using UnityEngine;

public static class EventBus
{
    public static Action OnPlayerDisconnected;
    public static Action<GameObject> OnWeaponExploded;

    public static Action<Character, bool> OnPlayerChangedMoveState;
    public static Action<Transform, float> OnPlayerShoot;
    public static Action<GameObject> OnWeaponSpawned;
    public static Action<Character> OnCharacterTakeDamage;
    public static Action<Character, int> OnCharacterFall;
    public static Action<Character> OnCharacterTakeAim;
    public static Action OnPlayerTakeWeapon;

    public static Action OnPlayerWin;
    public static Action OnCharacterLose;
    public static Action OnPlayerStartSearchMatch;

    public static Action<int> OnMatchEnded;

    public static Action<int> OnPlayerClickUI;

    public static Action OnSetTutorialTaskForPlayer;
    public static Action OnPlayerEndTutorial;

    public static Action OnPlayerGetUserIDFromDB;
    public static Action OnPlayerChangeNickname;
    public static Action<int> OnPlayerChangeSkin;
}
