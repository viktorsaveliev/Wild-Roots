using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class RoundEndUI : MonoBehaviour
{
    [SerializeField] private GameObject _winnerScreen;
    [SerializeField] private GameObject _defeatScreen;
    [SerializeField] private GameObject _buttonMenu;
    [SerializeField] private Text _text;
    [SerializeField] private Image _loading;

    public void Show(string winnerNickname)
    {
        _buttonMenu.SetActive(false);
        _defeatScreen.SetActive(false);
        _winnerScreen.SetActive(true);
        _text.text = $"Round {Match.CurrentRound+1}\nWinner: <color=red>{winnerNickname}</color>";

        _loading.fillAmount = 0;
        _loading.DOFillAmount(1, 3f).OnComplete(() => DOTween.Clear());
    }
}
