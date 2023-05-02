using Photon.Pun;
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
    }

    private void Start()
    {
        if (_isABot == false)
        {
            if (PhotonView.IsMine == false) return;
            Nickname = PlayerData.GetNickname();
            if (Nickname == string.Empty)
            {
                StringBus stringBus = new();
                Nickname = stringBus.NicknameBus[Random.Range(0, stringBus.NicknameBus.Length)];
            }

            Skin.Change(PlayerData.GetSkinID(), true);
        }
        else
        {
            Skin.IsABot = true;
            StartCoroutine(Skin.ChangeToRandom());
        }
    }

    private void OnEnable()
    {
        EventBus.OnMatchEnded += CheckWinner;
        EventBus.OnCharacterLose += OnLose;
        EventBus.OnCharacterFall += AddPointsForKiller;
    }

    private void OnDisable()
    {
        EventBus.OnMatchEnded -= CheckWinner;
        EventBus.OnCharacterLose -= OnLose;
        EventBus.OnCharacterFall -= AddPointsForKiller;
    }

    private void AddPointsForKiller(Character character, int health)
    {
        if (PhotonView.IsMine == false || IsABot || character == this) return;
        if (character.Health.FromWhomDamage != null && character.Health.FromWhomDamage.CharacterOwner.GetPhotonView().ViewID == PhotonView.ViewID)
        {
            PlayerData.AddDroppedPlayersCount();
        }
    }

    private void CheckWinner(int winnerID)
    {
        if (gameObject.activeSelf == false || _isReceivedPrize || IsABot) return;

        PhotonView winner = PhotonView.Find(winnerID);
        if (winner == null || winner.GetComponent<Character>().IsABot || winner.IsMine == false) return;

        PlayerData.GiveWinnerAward();
        EventBus.OnPlayerWin?.Invoke();

        _isReceivedPrize = true;
    }

    private void OnLose(int viewID)
    {
        if (_isReceivedPrize || IsABot) return;

        PhotonView loser = PhotonView.Find(viewID);
        if (loser.IsMine == false || loser.GetComponent<Character>().IsABot) return;

        PlayerData.GiveExp(30);

        if(PlayerData.GetDroppedPlayersCount > 0)
        {
            Coins.Give(CoinsHandler.GiveReason.ForKiller);
        }
        
        _isReceivedPrize = true;
    }

    /*private void UpdateData()
    {
        if (IsABot) return;
        LoadData.Instance.LoadUserData(this);
    }*/
}
