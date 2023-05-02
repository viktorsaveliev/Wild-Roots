using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Rating : MonoBehaviour
{
    [SerializeField] private GameObject _ratingSlotPrefab;
    [SerializeField] private GameObject _ratingUI;
    [SerializeField] private Transform _content;

    [SerializeField] private Sprite[] _placeIcons;
    [SerializeField] private Image[] _buttons;
    [SerializeField] private Sprite[] _buttonsBG;

    [SerializeField] private GameObject _loading;
    //[SerializeField] private BannersHandler _bannersLobby;

    public Sprite[] GetPlaceIcon => _placeIcons;

    private bool _isOpen;
    private float _antiflood;

    public void Open(bool topToday)
    {
        if (_antiflood >= Time.time) return;
        _loading.SetActive(true);
        if(_isOpen == false)
        {
            //_bannersLobby.UpdateVisible(false);
            _ratingUI.SetActive(true);
            _isOpen = true;
        }
        else
        {
            ResetList();
        }

        _buttons[topToday ? 0 : 1].sprite = _buttonsBG[1];
        _buttons[topToday ? 1 : 0].sprite = _buttonsBG[0];

        StartCoroutine(GetTop10Players(topToday));

        _antiflood = Time.time + 1f;
        EventBus.OnPlayerClickUI?.Invoke(0);
    }

    public void Close()
    {
        //_bannersLobby.UpdateVisible(true);
        ResetList();
        _ratingUI.SetActive(false);
        _isOpen = false;
        EventBus.OnPlayerClickUI?.Invoke(1);
    }

    private void ResetList()
    {
        RatingSlot[] slots = _content.GetComponentsInChildren<RatingSlot>();
        foreach (RatingSlot child in slots)
        {
            Destroy(child.gameObject);
        }
    }

    private IEnumerator GetTop10Players(bool topToday)
    {
        StringBus stringBus = new();
        string script = topToday ? "get_top10_wins_today.php" : "get_top10_wins.php";

        using UnityWebRequest www = UnityWebRequest.Get(stringBus.GameDomain + script);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;
            if(json == "false")
            {
                _loading.SetActive(false);
                print("Top list is empty");
                yield break;
            }

            List<TopPlayer> players = JsonConvert.DeserializeObject<List<TopPlayer>>(json);

            int i = 1;
            foreach (TopPlayer player in players)
            {
                if (player.Wins <= 0) continue;
                if (player.Nickname == string.Empty)
                {
                    player.Nickname = stringBus.NicknameBus[Random.Range(0, stringBus.NicknameBus.Length)];
                }

                RatingSlot ratingSlot = Instantiate(_ratingSlotPrefab, _content).GetComponent<RatingSlot>();
                ratingSlot.UpdateInfo(this, i, player.Nickname, player.Wins);

                if(i == 1 && topToday)
                {
                    ratingSlot.ShowPrize();
                }
                else
                {
                    ratingSlot.HidePrize();
                }

                i++;
            }
        }
        else
        {
            Notice.Dialog(www.downloadHandler.error);
        }
        _loading.SetActive(false);
    }
}

public class TopPlayer
{
    public string Nickname { get; set; }
    public int Wins { get; set; }
}