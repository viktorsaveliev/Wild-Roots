using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Character : MonoBehaviour
{
    public CharacterMovement Move { get; private set; }
    public CharacterWeapon Weapon { get; private set; }
    public PlayerHealth Health { get; private set; }
    public CharacterSkin Skin { get; private set; }

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

    private bool _isReceivedPrize;

    private void Awake()
    {
        Move = GetComponent<CharacterMovement>();
        Weapon = GetComponent<CharacterWeapon>();
        Health = GetComponent<PlayerHealth>();
        Skin = GetComponent<CharacterSkin>();
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
        EventBus.OnPlayerGetUserIDFromDB += UpdateData;
    }

    private void OnDisable()
    {
        EventBus.OnMatchEnded -= CheckWinner;
        EventBus.OnPlayerGetUserIDFromDB -= UpdateData;
    }

    private void CheckWinner(int winnerID)
    {
        if (!PhotonView.IsMine || _isReceivedPrize) return;

        PhotonView winner = PhotonView.Find(winnerID);
        if (winner == null || winner.GetComponent<Character>().IsABot) return;

        if (winner.IsMine)
        {
            Wins++;
            StartCoroutine(GiveExp(100));
            EventBus.OnPlayerWin?.Invoke();
        }
        else
        {
            StartCoroutine(GiveExp(30));
        }

        _isReceivedPrize = true;
    }

    private IEnumerator GiveExp(int exp)
    {
        StringBus stringBus = new();
        bool isGuest = PlayerPrefs.GetInt(stringBus.IsGuest) == 1;

        if (isGuest == false)
        {
            yield return StartCoroutine(LoadData.Instance.IELoadUserData(this));
        }

        Exp += exp;
        if (Exp >= (Level * 3 * 100))
        {
            Level++;
            Exp = 0;
        }

        int id = PlayerPrefs.GetInt(stringBus.UserID);
        if (isGuest == false && id > 0)
        {
            SaveData.Instance.SaveLevelData(id, this);
        }
        print($"gived {exp} exp");
    }

    private void UpdateData()
    {
        StringBus stringBus = new();
        bool isGuest = PlayerPrefs.GetInt(stringBus.IsGuest) == 1;

        if (!PhotonView.IsMine || isGuest) return;
        
        LoadData.Instance.LoadUserData(this);
    }
}
