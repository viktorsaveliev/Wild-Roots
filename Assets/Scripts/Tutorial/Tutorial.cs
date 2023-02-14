using DG.Tweening;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour, INoticeAction
{
    [SerializeField] private GameObject _character;
    [SerializeField] private GameObject _enemy;
    [SerializeField] private Weapon _weapon;

    [SerializeField] private JoystickAttack _joystickAttack;
    [SerializeField] private JoystickMovement _joystickMovement;

    private PlayerInfo _player;

    public enum Task
    {
        TakeGrenade,
        ThrowGrenade,
        ThrowEnemy,
        ThrowEnemyWithHand
    }

    public Task CurrentTask { get; private set; }

    private void OnEnable()
    {
        EventBus.OnPlayerTakeWeapon += OnPlayerGetWeapon;
        EventBus.OnPlayerFall += OnCharacterFall;

        EventBus.OnWeaponExploded += WeaponSpawner;
    }

    private void OnDisable()
    {
        EventBus.OnPlayerTakeWeapon -= OnPlayerGetWeapon;
        EventBus.OnPlayerFall -= OnCharacterFall;

        EventBus.OnWeaponExploded -= WeaponSpawner;
    }

    private void Awake()
    {
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
    }

    private void Start()
    {
        Invoke(nameof(SetOfflineMode), 1f);

        _player = _character.GetComponent<PlayerInfo>();
        SetTask(Task.TakeGrenade);

        _joystickAttack.Init(_player);
        _joystickMovement.Init(_player);
    }

    private void SetOfflineMode()
    {
        PhotonNetwork.OfflineMode = true;
    }

    private void SetTask(Task task)
    {
        CurrentTask = task;
        EventBus.OnSetTutorialTaskForPlayer?.Invoke();
    }

    private void OnPlayerGetWeapon()
    {
        if (CurrentTask != Task.TakeGrenade) return;
        SetTask(Task.ThrowGrenade);
    }

    private void OnCharacterFall(PlayerInfo player, int health)
    {
        if(player.PhotonView.ViewID == 2)
        {
            _player.transform.position = new Vector3(0, 0.5f, -4.82f);
            _player.gameObject.SetActive(true);

            if (_player.Weapon.GetCurrentWeapon is Punch)
            {
                WeaponSpawner(_weapon.gameObject);
                Invoke(nameof(ResetPlayerWeapon), 0.5f);
            }
        }
        else
        {
            PhotonNetwork.OfflineMode = false;
            Notice.ShowDialog(NoticeDialog.Message.EndTutorial, this, "NoticeButton_Menu");
            EventBus.OnPlayerEndTutorial?.Invoke();
        }
    }

    public void ActionOnClickNotice(int button)
    {
        StartCoroutine(LoadingLobby());
        Notice.HideDialog();
    }

    private void ResetPlayerWeapon() => _player.Weapon.DeleteWeapon(false);

    private IEnumerator LoadingLobby()
    {
        LoadingUI.Show(LoadingShower.Type.Progress);
        AsyncOperation percentload = SceneManager.LoadSceneAsync(0);
        while (!percentload.isDone)
        {
            yield return new WaitForEndOfFrame();
            LoadingUI.UpdateProgress(percentload.progress);
        }
    }

    private void WeaponSpawner(GameObject weaponObj)
    {
        StartCoroutine(RespawnWeapon());
    }

    private IEnumerator RespawnWeapon()
    {
        yield return new WaitForSeconds(2);
        _weapon.transform.position = new Vector3(0.14f, 0.26f, 0);

        _weapon.GetComponent<Collider>().isTrigger = true;

        Rigidbody rigidbody = _weapon.GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        _weapon.gameObject.SetActive(true);
        _weapon.SpawnAnimation = _weapon.transform.DOScale(1, 0.3f).OnComplete(() => _weapon.SpawnAnimation = null);

        EventBus.OnWeaponSpawned?.Invoke(_weapon.gameObject);

        if (CurrentTask == Task.ThrowGrenade)
        {
            _enemy.SetActive(true);
            SetTask(Task.ThrowEnemy);
        }
        else if(CurrentTask == Task.ThrowEnemy)
        {
            SetTask(Task.ThrowEnemyWithHand);
        }
    }
}
