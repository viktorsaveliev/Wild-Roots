using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using Photon.Pun;

public class FightUI : MonoBehaviour
{
    // Knocked down
    [SerializeField] private GameObject _knockedUI;
    [SerializeField] private Text _header;
    [SerializeField] private Image _progressBar;
    private Tween _animation;
    private Coroutine _waitingCoroutine;

    // Eliminate
    [SerializeField] private GameObject _eliminateUI;
    [SerializeField] private Text _eliminate;

    private void OnEnable()
    {
        EventBus.OnCharacterTakeDamage += EnableKnockedUI;
        EventBus.OnCharacterFall += EnableEliminateUI;
    }

    private void OnDisable()
    {
        EventBus.OnCharacterTakeDamage -= EnableKnockedUI;
        EventBus.OnCharacterFall -= EnableEliminateUI;
    }

    private void EnableEliminateUI(Character killed, int health)
    {
        StartCoroutine(EnableEliminateUIAsync(killed, health));
    }

    private IEnumerator EnableEliminateUIAsync(Character killed, int health)
    {
        if (killed.Health.FromWhomDamage == -1) yield break;
        PhotonView killerPhotonView = PhotonView.Find(killed.Health.FromWhomDamage);
        Character killer = killerPhotonView.GetComponent<Character>();
        if (killerPhotonView.IsMine == false || killer.IsABot || killer == killed) yield break;

        _eliminate.transform.localScale = Vector3.zero;
        _eliminateUI.SetActive(true);
        _eliminate.text = $"Eliminated <color=red>{killed.Nickname}</color>";
        _eliminate.transform.DOScale(1, 0.5f).SetEase(Ease.OutElastic);

        yield return new WaitForSeconds(3f);

        _eliminateUI.SetActive(false);
    }

    private void EnableKnockedUI(Character character)
    {
        if (character.PhotonView.IsMine == false || character.IsABot) return;

        if(_knockedUI.activeSelf == false)
        {
            _header.transform.localScale = Vector3.zero;
            _knockedUI.SetActive(true);

            _header.transform.DOScale(1, 0.5f).SetEase(Ease.OutElastic);
        }

        _progressBar.fillAmount = 0;

        if (character.Move.GetFreezeTime > 0)
        {
            StartFillProgress(character);
        }
        else
        {
            if (_waitingCoroutine != null) StopCoroutine(_waitingCoroutine);
            _waitingCoroutine = StartCoroutine(WaitForCharacterFall(character));
        }
    }

    private IEnumerator WaitForCharacterFall(Character character)
    {
        while(character.Move.GetFreezeTime < 1)
        {
            yield return null;
        }
        StartFillProgress(character);
    }

    private void StartFillProgress(Character character)
    {
        if (_animation != null)
        {
            _animation.Kill();
        }

        _animation = _progressBar.DOFillAmount(1, character.Move.GetFreezeTime).OnComplete(() =>
        {
            if (_waitingCoroutine != null)
            {
                StopCoroutine(_waitingCoroutine);
                _waitingCoroutine = null;
            }
            _knockedUI.SetActive(false);
            _animation = null;
        });
    }
}
