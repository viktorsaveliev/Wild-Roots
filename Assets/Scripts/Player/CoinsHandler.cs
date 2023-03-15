using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using DG.Tweening;

public class CoinsHandler : MonoBehaviour
{
    [SerializeField] private Text _coinsText;
    [SerializeField] private Text _differenceText;
    [SerializeField] private Animator _animator;

    private int _coins;

    public enum GiveReason
    {
        Ads,
        Winner,
        EndMatch
    }

    public void GiveCoins(GiveReason reason)
    {
        StartCoroutine(IEGiveCoins(reason));
    }

    private IEnumerator IEGiveCoins(GiveReason reason)
    {
        StringBus stringBus = new();
        bool isGuest = PlayerPrefs.GetInt(stringBus.IsGuest) == 1;
        if (isGuest)
        {
            UpdateCoins(_coins + 100);
            yield break;
        }

        WWWForm form = new();
        form.AddField("id", PlayerPrefs.GetInt(stringBus.UserID));

        string scriptName;
        switch (reason)
        {
            case GiveReason.Ads:
                scriptName = "give_coins_ads.php";
                break;

            default:
                scriptName = "give_coins_ads.php";
                break;
        }

        using UnityWebRequest www = UnityWebRequest.Post(stringBus.GameDomain + scriptName, form);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            bool success = int.TryParse(www.downloadHandler.text, out int value);
            if (success)
            {
                UpdateCoins(value);
            }
            else
            {
                Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
            }
        }
        else
        {
            print(www.downloadHandler.error);
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
    }

    public void UpdateCoins(int value, bool anim = true)
    {
        int differenceValue = value - _coins;
        if(_coinsText != null)
        {
            if(anim)
            {
                _differenceText.text = $"{(differenceValue > 0 ? '+' : '-')}{differenceValue}";
                _coinsText.DOCounter(_coins, value, 2f);
                _animator.SetTrigger("update");
            }
            else
            {
                _coinsText.text = value.ToString();
            }
        }
        _coins = value;
    }

    public int GetValue() => _coins;
}
