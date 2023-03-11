using CrazyGames;
using UnityEngine;

public class PlayAsGuest : MonoBehaviour
{
    [SerializeField] private GameObject _note;

    private Registration _reg;
    private Authorization _auth;

    private void Start()
    {
        _reg = GetComponent<Registration>();
        _auth = GetComponent<Authorization>();
    }

    public void OnClickButton()
    {
        _note.SetActive(true);

        EventBus.OnPlayerClickUI?.Invoke(0);

        StringBus stringBus = new();
        CrazySDK.Instance.GetUserInfo(userInfo =>
        {
            int deviceType = 0;
            if(userInfo.device.type == "desktop" || userInfo.browser.name == "demo")
            {
                deviceType = 1;
            }
            PlayerPrefs.SetInt(stringBus.PlayerDevice, deviceType);
        });

        _reg.Hide();
        _auth.Hide();
        gameObject.SetActive(false);

        PlayerPrefs.SetInt(stringBus.GuestAcc, 1);
        PlayerPrefs.Save();

        EventBus.OnPlayerLoadAccount?.Invoke();
        gameObject.SetActive(false);
    }
}
