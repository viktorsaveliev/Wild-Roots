using DG.Tweening;
using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour, INoticeAction
{
    [SerializeField] private GameObject _characterPrefab;
    [SerializeField] private GameObject _enemy;
    [SerializeField] private Weapon _weapon;

    [SerializeField] private JoystickAttack _joystickAttack;
    [SerializeField] private JoystickMovement _joystickMovement;

    private Character _character;

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
        EventBus.OnCharacterGetWeapon += OnPlayerGetWeapon;
        EventBus.OnCharacterFall += OnCharacterFall;

        EventBus.OnWeaponExploded += WeaponSpawner;
    }

    private void OnDisable()
    {
        EventBus.OnCharacterGetWeapon -= OnPlayerGetWeapon;
        EventBus.OnCharacterFall -= OnCharacterFall;

        EventBus.OnWeaponExploded -= WeaponSpawner;
    }

    private void Awake()
    {
        if (PhotonNetwork.IsConnected) PhotonNetwork.Disconnect();
    }

    private void Start()
    {
        Invoke(nameof(SetOfflineMode), 1f);

        _character = _characterPrefab.GetComponent<Character>();
        SetTask(Task.TakeGrenade);
    }

    private void SetOfflineMode()
    {
        PhotonNetwork.OfflineMode = true;

        _character.Rigidbody.freezeRotation = true;
        _character.Move.SetMoveActive(true);
        _joystickAttack.Init(_character);
        _joystickMovement.Init(_character);
    }

    private void SetTask(Task task)
    {
        CurrentTask = task;
        EventBus.OnSetTutorialTaskForPlayer?.Invoke();
    }

    private void OnPlayerGetWeapon(Weapon weapon)
    {
        if (CurrentTask != Task.TakeGrenade) return;
        SetTask(Task.ThrowGrenade);
    }

    private void OnCharacterFall(Character character, int health)
    {
        if(character.PhotonView.ViewID == 2)
        {
            _character.transform.position = new Vector3(0, 0f, -4.82f);
            _character.gameObject.SetActive(true);

            if (_character.Weapon.GetCurrentWeapon() is Punch)
            {
                WeaponSpawner(_weapon.gameObject);
                Invoke(nameof(ResetPlayerWeapon), 0.5f);
            }

            _character.Move.SetMoveActive(true);
        }
        else
        {
            PhotonNetwork.OfflineMode = false;
            Notice.Dialog(NoticeDialog.Message.EndTutorial, this, "NoticeButton_Menu");
            EventBus.OnPlayerEndTutorial?.Invoke();
        }
    }

    public void ActionOnClickNotice(int button)
    {
        StartCoroutine(LoadingLobby());
        Notice.HideDialog();
    }

    private void ResetPlayerWeapon() => _character.Weapon.DeleteWeapon(false);

    private IEnumerator LoadingLobby()
    {
        LoadingUI.Show(LoadingShower.Type.Progress);
        AsyncOperation percentload = SceneManager.LoadSceneAsync((int)GameSettings.Scene.Lobby);
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
        _weapon.transform.position = new Vector3(0.21f, 1.98f, 0.64f);

        _weapon.GetComponent<Collider>().isTrigger = true;

        Rigidbody rigidbody = _weapon.GetComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        _weapon.gameObject.SetActive(true);
        _weapon.transform.DOScale(4, 0.3f);

        EventBus.OnWeaponSpawned?.Invoke(_weapon.gameObject);

        if (CurrentTask == Task.ThrowGrenade)
        {
            _enemy.SetActive(true);
            Character enemy = _enemy.GetComponent<Character>();
            enemy.Nickname = "Ivan";
            yield return new WaitForSeconds(0.7f);
            enemy.Move.SetMoveActive(true);


            SetTask(Task.ThrowEnemy);
        }
        else if(CurrentTask == Task.ThrowEnemy)
        {
            SetTask(Task.ThrowEnemyWithHand);
        }
    }
}
