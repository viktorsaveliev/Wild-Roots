using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RatingSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text _nickname;
    [SerializeField] private TMP_Text _wins;
    [SerializeField] private TMP_Text _place;
    [SerializeField] private Image _placeIcon;
    [SerializeField] private GameObject _todayPrize;
    //[SerializeField] private Image _avatar;

    private Rating _rating;

    public void UpdateInfo(Rating rating, int place, string nickname, int wins)
    {
        _rating = rating;

        if(place > 3)
        {
            _place.text = place.ToString();
            _placeIcon.gameObject.SetActive(false);
        }
        else
        {
            _place.text = string.Empty;
            _placeIcon.gameObject.SetActive(true);
            _placeIcon.sprite = _rating.GetPlaceIcon[place - 1];
        }

        _nickname.text = nickname;
        _wins.text = wins.ToString();
    }

    public void ShowPrize()
    {
        _todayPrize.SetActive(true);
    }

    public void HidePrize()
    {
        _todayPrize.SetActive(false);
    }
}
