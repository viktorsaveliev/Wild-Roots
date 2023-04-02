using CrazyGames;
using UnityEngine;

public class ConnectDatabase : MonoBehaviour
{
    [SerializeField] private Registration _registration;
    [SerializeField] private Authorization _authorization;

    private bool _isMobileDevice;
    public static bool IsUserEnter = false;

    private void Start()
    {
        if (IsUserEnter) return;

        StringBus stringBus = new();
        CrazySDK.Instance.GetUserInfo(userInfo =>
        {
            int deviceType = 0;
            if (userInfo.device.type == "desktop" || userInfo.browser.name == "demo")
            {
                deviceType = 1;
            }
            else
            {
                _isMobileDevice = true;
            }
            PlayerPrefs.SetInt(stringBus.PlayerDevice, deviceType);
        });

        int accStatus = PlayerPrefs.GetInt(stringBus.AccStatus);

        switch (accStatus)
        {
            case 1:
                _authorization.Show(_isMobileDevice);
                break;

            case 2:
                _authorization.Show(_isMobileDevice);
                string email = PlayerPrefs.GetString(stringBus.Email);
                string password = PlayerPrefs.GetString(stringBus.Password);
                _authorization.AutoInputData(email, password);
                //StartCoroutine(_authorization.GetPlayerLogin(email, password));
                break;

            default:
                _registration.Show(_isMobileDevice);
                break;
        }
    }
}