using CrazyGames;
using UnityEngine;

public class ConnectDatabase : MonoBehaviour
{
    [SerializeField] private Registration _registration;
    [SerializeField] private Authorization _authorization;

    public static bool IsUserEnter = false;

    private void Start()
    {
        if (IsUserEnter) return;

        StringBus stringBus = new();
        int accStatus = PlayerPrefs.GetInt(stringBus.AccStatus);

        switch (accStatus)
        {
            case 1:
                _authorization.Show();
                break;

            case 2:
                _authorization.Show();
                string email = PlayerPrefs.GetString(stringBus.Email);
                string password = PlayerPrefs.GetString(stringBus.Password);
                _authorization.AutoInputData(email, password);
                //StartCoroutine(_authorization.GetPlayerLogin(email, password));
                break;

            default:
                _registration.Show();
                break;
        }

        IdentifyUserDevice();
    }

    private void IdentifyUserDevice()
    {
        StringBus stringBus = new();
        CrazySDK.Instance.GetUserInfo(userInfo =>
        {
            int deviceType;
            if (userInfo.device.type == "desktop" || userInfo.browser.name == "demo")
            {
                deviceType = 1;
            }
            else
            {
                deviceType = 0;
            }

            PlayerPrefs.SetInt(stringBus.PlayerDevice, deviceType);
            PlayerPrefs.SetInt(stringBus.IsGuest, 0);
            PlayerPrefs.Save();

            print($"Device: {deviceType} / {userInfo.device.type}");
        });
    }
}