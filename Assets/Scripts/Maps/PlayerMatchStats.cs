using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerMatchStats : MonoBehaviour
{
    [SerializeField] private Image _avatar;
    [SerializeField] private Text _nickname;
    [SerializeField] private Image[] _cups;

    private void Awake()
    {
        _nickname.text = string.Empty;
    }

    public void UpdateData(string nickname, int cupsCount)
    {
        _nickname.DOText(nickname ?? "Unknown hero", 0.5f);

        for (int i = 0; i < cupsCount; i++)
        {
            _cups[i].transform.localScale = Vector3.zero;
            _cups[i].gameObject.SetActive(true);
            _cups[i].transform.DOScale(1, 0.5f + i).SetEase(Ease.OutElastic);
        }
    }
}
