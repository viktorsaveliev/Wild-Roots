using UnityEngine;

public class Coins : MonoBehaviour
{
    private static CoinsHandler _coinsHandler;

    private void Start()
    {
        if (!LoadingShower.IsCreated) _coinsHandler = GetComponent<CoinsHandler>();
    }

    public static void Give(CoinsHandler.GiveReason reason) => _coinsHandler.GiveCoins(reason);
    public static void UpdateValue(int value, bool anim = true) => _coinsHandler.UpdateValue(value, anim);
    public static int GetValue() => _coinsHandler.GetValue();
    public static void Pay(int value) => _coinsHandler.Pay(value);
}
