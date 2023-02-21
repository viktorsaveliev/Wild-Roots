using Photon.Pun;
using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterMovement Move { get; private set; }
    public CharacterWeapon Weapon { get; private set; }
    public PlayerHealth Health { get; private set; }

    public CharacterHUD HUD;
    public TakeImpulse TakeImpulse { get; private set; }
    public PhotonView PhotonView { get; private set; }
    public Rigidbody Rigidbody { get; private set; }
    public Animator Animator { get; private set; }

    [SerializeField] private bool _isABot;
    public bool IsABot => _isABot;

    void Awake()
    {
        Move = GetComponent<CharacterMovement>();
        Weapon = GetComponent<CharacterWeapon>();
        Health = GetComponent<PlayerHealth>();
        TakeImpulse = GetComponent<TakeImpulse>();
        PhotonView = GetComponent<PhotonView>();
        Rigidbody = GetComponent<Rigidbody>();
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
        if (winner == null || winner.GetComponent<Character>().IsABot) return;

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
