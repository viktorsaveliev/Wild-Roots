using UnityEngine;

public class Coins : MonoBehaviour
{
    private static CoinsHandler _coinsHandler;

    private void Start()
    {
        _coinsHandler = GetComponent<CoinsHandler>();
    }

    public static void Give(CoinsHandler.GiveReason reason) => _coinsHandler.GiveCoins(reason);
    public static void UpdateValue(int value, bool anim = true) => _coinsHandler.UpdateCoins(value, anim);
    public static int GetValue() => _coinsHandler.GetValue();
}
