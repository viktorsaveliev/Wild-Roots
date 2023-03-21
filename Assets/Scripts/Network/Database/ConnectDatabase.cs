using UnityEngine;

public class ConnectDatabase : MonoBehaviour
{
    [SerializeField] private Registration _registration;
    [SerializeField] private Authorization _authorization;

    private void Start()
    {
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
    }
}