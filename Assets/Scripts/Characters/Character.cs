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
    public int PlayerID { get; private set; }

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

        PlayerID = -1;
    }

    private void Start()
    {
        if (_isABot == false)
        {
            if (PhotonView.IsMine == false) return;
            Nickname = PlayerData.GetNickname();
            Skin.Change(PlayerData.GetSkinID(), true);
        }
        else
        {
            Skin.IsABot = true;
        }
    }

    private void OnEnable()
    {
        EventBus.OnRoundEnded += CheckWinner;
        EventBus.OnCharacterFall += AddPointsForKiller;
    }

    private void OnDisable()
    {
        EventBus.OnRoundEnded -= CheckWinner;
        EventBus.OnCharacterFall -= AddPointsForKiller;
    }

    private void AddPointsForKiller(Character character, int health)
    {
        if (PhotonView.IsMine == false || IsABot || character == this) return;
        if (character.Health.FromWhomDamage == PhotonView.ViewID)
        {
            PlayerData.AddDroppedPlayersCount();
        }
    }

    private void CheckWinner(int winnerID)
    {
        if (_isReceivedPrize || gameObject.activeSelf == false) return;

        Character character = PhotonView.Find(winnerID).GetComponent<Character>();
        if (character == null || PlayerID == -1 || character.PlayerID != PlayerID) return;

        Match.GiveWinnerCoup(PlayerID, character.IsABot);
        print("Winner ID: " + PlayerID);

        _isReceivedPrize = true;
    }

    public void SetPlayerID(int playerid)
    {
        if (PlayerID != -1) return;
        PlayerID = playerid;
    }
}
