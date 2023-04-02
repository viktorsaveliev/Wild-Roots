using UnityEngine;

public class PlayAsGuest : MonoBehaviour
{
    [SerializeField] private GameObject _note;
    [SerializeField] private GameObject _character;

    private Registration _reg;
    private Authorization _auth;

    private void Start()
    {
        _reg = GetComponent<Registration>();
        _auth = GetComponent<Authorization>();
    }

    public void OnClickButton()
    {
        LoadingUI.Show(LoadingShower.Type.Simple);
        _note.SetActive(true);

        EventBus.OnPlayerClickUI?.Invoke(0);

        StringBus stringBus = new();

        _reg.Hide();
        _auth.Hide();
        gameObject.SetActive(false);

        PlayerPrefs.SetInt(stringBus.IsGuest, 1);
        PlayerPrefs.Save();

        EventBus.OnPlayerGetUserIDFromDB?.Invoke();
        gameObject.SetActive(false);

        ConnectDatabase.IsUserEnter = true;
        _character.SetActive(true);
    }
}
