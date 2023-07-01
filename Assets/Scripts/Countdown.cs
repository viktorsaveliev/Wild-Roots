using Photon.Pun;
using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;

[RequireComponent(typeof(ServerHandler))]
public class Countdown : MonoBehaviour
{
    [SerializeField] private TMP_Text _textCount;
    [SerializeField] private AudioClip _timerSound;
    [SerializeField] private AudioClip _levelMusic;

    private AudioSource _audioSource;
    private ServerHandler _server;
    private int _secondsToStart = 4;

    private void Start()
    {
        _server = GetComponent<ServerHandler>();
        _audioSource = GetComponent<AudioSource>();
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(CountdownToStart());
        }
    }

    private IEnumerator CountdownToStart()
    {
        yield return new WaitForSeconds(1f);
        _textCount.gameObject.SetActive(true);
        while (_secondsToStart > 0)
        {
            if(PhotonNetwork.OfflineMode)
            {
                CountdownUpdate(--_secondsToStart);
            }
            else
            {
                _server.ServerPhotonView.RPC(nameof(CountdownUpdate), RpcTarget.All, --_secondsToStart);
            }
            yield return new WaitForSeconds(1f);
        }

        if (PhotonNetwork.OfflineMode)
        {
            StartMatch();
        }
        else
        {
            _server.ServerPhotonView.RPC(nameof(StartMatch), RpcTarget.All);
        }
    }

    [PunRPC]
    private void CountdownUpdate(int seconds)
    {
        if(seconds > 2) _textCount.gameObject.SetActive(true);

        _textCount.rectTransform.anchoredPosition = new Vector2(0, 123f);
        _textCount.rectTransform.localScale = new Vector2(1, 1);
        _textCount.color = Color.white;

        _secondsToStart = seconds;

        StringBus stringBus = new();
        if (seconds == 0)
        {
            _textCount.text = "Grab a grenade!";
            _textCount.rectTransform.DOScale(1.2f, 1f);
            _textCount.rectTransform.DOShakeRotation(1f, 20).OnComplete(() => _textCount.gameObject.SetActive(false));

            _audioSource.clip = _levelMusic;
            _audioSource.loop = true;

            if (PlayerPrefs.GetInt(stringBus.SettingsMusic) == 0) _audioSource.Play();
        }
        else
        {
            _textCount.text = seconds.ToString();
            _textCount.rectTransform.DOAnchorPosX(131f, 1f);
            _textCount.rectTransform.DOScale(0.5f, 1f);
            _textCount.DOColor(new Color(1, 1, 1, 0), 1f);

            _audioSource.clip = _timerSound;
            if (PlayerPrefs.GetInt(stringBus.SettingsSoundFX) == 0) _audioSource.Play();
        }
    }

    [PunRPC]
    private void StartMatch()
    {
        foreach (Character character in _server.Characters)
        {
            if (character.IsABot == false) continue;
            character.Move.SetMoveActive(true);
        }
    }
}
