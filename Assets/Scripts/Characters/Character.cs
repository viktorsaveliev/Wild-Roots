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

    public string Nickname;
    public int Level;
    public int Exp;
    public int Wins;

    private void Awake()
    {
        Move = GetComponent<CharacterMovement>();
        Weapon = GetComponent<CharacterWeapon>();
        Health = GetComponent<PlayerHealth>();
        TakeImpulse = GetComponent<TakeImpulse>();
        PhotonView = GetComponent<PhotonView>();
        Rigidbody = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();

        if(_isABot == false)
        {
            StringBus stringBus = new();
            Nickname = stringBus.NicknameBus[Random.Range(0, stringBus.NicknameBus.Length)];
        }
    }

    private void OnEnable()
    {
        EventBus.OnMatchEnded += CheckWinner;
        EventBus.OnPlayerLoadAccount += UpdateLevel;
    }

    private void OnDisable()
    {
        EventBus.OnMatchEnded -= CheckWinner;
        EventBus.OnPlayerLoadAccount -= UpdateLevel;
    }

    private void CheckWinner(int winnerID)
    {
       
        if (!PhotonView.IsMine) return;

        PhotonView winner = PhotonView.Find(winnerID);
        if (winner == null || winner.GetComponent<Character>().IsABot) return;

        StringBus stringBus = new();
        bool isGuest = PlayerPrefs.GetInt(stringBus.GuestAcc) == 1;

        if(isGuest == false) LoadData.Instance.LoadLevelData(this);

        if (winner.IsMine)
        {
            Wins++;

            Exp += 100;
            if (Exp >= (Level * 3 * 100))
            {
                Level++;
                Exp = 0;
            }
            EventBus.OnPlayerWin?.Invoke();
        }
        else
        {
            Exp += 30;
            if (Exp >= (Level * 3 * 100))
            {
                Level++;
                Exp = 0;
            }
        }

        int id = PlayerPrefs.GetInt(stringBus.PlayerID);
        if (isGuest == false) SaveData.Instance.SaveLevelData(id, this);
    }

    private void UpdateLevel()
    {
        if (!PhotonView.IsMine) return;
        LoadData.Instance.LoadLevelData(this);
    }
}
