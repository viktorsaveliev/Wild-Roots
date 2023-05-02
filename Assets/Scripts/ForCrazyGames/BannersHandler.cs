using UnityEngine;
using CrazyGames;

public class BannersHandler : MonoBehaviour
{
    [SerializeField] private CrazyBanner[] _banners;

    private void Awake()
    {
        foreach (CrazyBanner banner in _banners)
        {
            banner.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        UpdateVisible(true);
    }

    private void OnEnable()
    {
        UpdateVisible(true);
    }

    private void OnDisable()
    {
        UpdateVisible(false);
    }

    public void UpdateVisible(bool visible)
    {
        if (CrazyAds.Instance == null) return;

        foreach (CrazyBanner banner in _banners)
        {
            banner.MarkVisible(visible);
        }
        CrazyAds.Instance.updateBannersDisplay();
    }
}
