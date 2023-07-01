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
    public static Action<Weapon> OnCharacterGetWeapon;

    public static Action OnPlayerWin;
    public static Action<int> OnCharacterLose;
    public static Action OnPlayerStartSearchMatch;

    public static Action OnRoundStarted;
    public static Action<int> OnRoundEnded;

    public static Action<int> OnPlayerClickUI;

    public static Action OnSetTutorialTaskForPlayer;
    public static Action OnPlayerEndTutorial;

    public static Action OnPlayerLogged;
    public static Action OnPlayerChangeNickname;
    public static Action<string> OnPlayerTryChangeNickname;
    public static Action<int> OnPlayerNeedChangeSkin;
    public static Action<int, bool> OnPlayerUpdateCoinsValue;
    public static Action OnPlayerBuyNewSkin;
    public static Action OnPlayerViewedAds;

    public static Action<string, int> OnPlayerTopTodayWinner;
}
