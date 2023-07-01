using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections;
using CrazyGames;

public class MatchInformer : MonoBehaviour
{
    [SerializeField] private Image _loading;
    [SerializeField] private Image _mapImage;
    [SerializeField] private Sprite[] _maps;
    [SerializeField] private PlayerMatchStats[] _players;
    private const float _delayForStartRound = 8f;
    private int _oldMapShowed = -1;

    private void Start()
    {
        LoadingUI.Hide();
        _loading.fillAmount = 0;

        int winnerID = -1;
        for (int i = 0; i < _players.Length; i++)
        {
            _players[i].UpdateData(Match.GetPlayerNickname(i), Match.GetPlayerCupsCount(i));
            if(Match.GetPlayerCupsCount(i) >= 3)
            {
                winnerID = i;
                print("Winner founded. ID: " + winnerID);
            }
        }

        if(winnerID == -1)
        {
            StartCoroutine(SelectRandomMap());
        }

        _loading.DOFillAmount(1, _delayForStartRound).SetEase(Ease.Linear).OnComplete(() =>
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (winnerID == -1)
                {
                    Match.StartNextRound();
                }
                else
                {
                    SaveData.Instance.Stats(SaveData.Statistics.LeftTheMatch);

                    DOTween.Clear();
                    CrazyAds.Instance.beginAdBreak(AdsSucces, AdsError);

                    PhotonNetwork.LoadLevel((int)GameSettings.Scene.Lobby);
                    PhotonNetwork.LeaveRoom();
                    Match.ResetData();
                }
            }
        });
    }

    private IEnumerator SelectRandomMap()
    {
        float seconds = 0;
        while (seconds < _delayForStartRound - 2)
        {
            yield return new WaitForSeconds(0.3f);
            ShowRandomMap();
            seconds += 0.3f;
        }
        ShowMap(Match.NextMap);
    }

    private void ShowRandomMap()
    { 
        int newMapID = Random.Range(0, _maps.Length);

        if(_oldMapShowed == newMapID)
        {
            newMapID = newMapID > 0 ? newMapID - 1 : newMapID + 1;
        }
        
        _oldMapShowed = newMapID;
        _mapImage.sprite = _maps[newMapID];
    }

    private void ShowMap(int mapID)
    {
        _mapImage.sprite = _maps[mapID-4];
    }

    private void AdsSucces()
    {
        PlayerData.ViewedAds();
        SaveData.Instance.Stats(SaveData.Statistics.MidgameAds);
    }

    private void AdsError()
    {
        print("[Ads error] Some problem #036");
    }
}
