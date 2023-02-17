using Photon.Pun;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    public PlayerMove Move { get; private set; }
    public PlayerWeapon Weapon { get; private set; }
    public PlayerHealth Health { get; private set; }

    public PlayerHUD HUD;
    public TakeImpulse TakeImpulse { get; private set; }
    public PhotonView PhotonView { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public AudioSource AudioSource { get; private set; }
    public Animator Animator { get; private set; }

    public bool IsDisconnect;

    void Awake()
    {
        Move = GetComponent<PlayerMove>();
        Weapon = GetComponent<PlayerWeapon>();
        Health = GetComponent<PlayerHealth>();
        TakeImpulse = GetComponent<TakeImpulse>();
        PhotonView = GetComponent<PhotonView>();
        Rigidbody = GetComponent<Rigidbody>();
        AudioSource = GetComponent<AudioSource>();
        Animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        EventBus.OnMatchEnded += CheckWinner;
    }

    private void OnDisable()
    {
        EventBus.OnMatchEnded -= CheckWinner;
    }

    private void CheckWinner(int winnerID)
    {
        if (!PhotonView.IsMine) return;

        PhotonView winner = PhotonView.Find(winnerID);
        if (winner == null) return;

        StringBus stringBus = new();
        int level = PlayerPrefs.GetInt(stringBus.PlayerLevel);
        int levelExp = PlayerPrefs.GetInt(stringBus.PlayerExp);

        if (winner.IsMine)
        {
            int winsCount = PlayerPrefs.GetInt(stringBus.PlayerWinsCount);
            PlayerPrefs.SetInt(stringBus.PlayerWinsCount, winsCount + 1);

            levelExp += 100;
            if (levelExp >= (level * 3 * 100))
            {
                level++;
                levelExp = 0;
            }
            EventBus.OnPlayerWin?.Invoke();
        }
        else
        {
            levelExp += 50;
            if (levelExp >= (level * 3 * 100))
            {
                level++;
                levelExp = 0;
            }
        }

        PlayerPrefs.SetInt(stringBus.PlayerLevel, level);
        PlayerPrefs.SetInt(stringBus.PlayerExp, levelExp);
        PlayerPrefs.Save();
    }
}
