using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private Image _taskPanel;
    [SerializeField] private Text _taskText;
    private LocalizeStringEvent _taskTextEvent;
    private Tutorial _tutorial;

    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private LocalizeStringEvent _infoText;
    [SerializeField] private Image _handCursor;

    [SerializeField] private GameObject _joysticks;

    private readonly string[] _taskKey =
    {
        "TakeGrenade",
        "ThrowGrenade",
        "ThrowEnemy",
        "ThrowEnemy"
    };

    private readonly string[] _infoTextKey =
    {
        "Info_TakeGrenade",
        "Info_ThrowGrenade",
        "Info_ThrowEnemy",
        "Info_ThrowEnemyWithHand"
    };

    private void OnEnable()
    {
        EventBus.OnSetTutorialTaskForPlayer += EnableInfoText;
        EventBus.OnPlayerEndTutorial += HideTask;
    }

    private void OnDisable()
    {
        EventBus.OnSetTutorialTaskForPlayer -= EnableInfoText;
        EventBus.OnPlayerEndTutorial -= HideTask;
    }

    private void Awake()
    {
        _tutorial = GetComponent<Tutorial>();
        _taskTextEvent = _taskText.GetComponent<LocalizeStringEvent>();

        EnableInfoText();
    }

    private void UpdateTask()
    {
        _taskTextEvent.SetEntry(_taskKey[(int)_tutorial.CurrentTask]);
        _taskPanel.rectTransform.DOAnchorPosY(-53f, 1f).SetEase(Ease.OutBounce);
    }

    private void HideTask()
    {
        _taskPanel.gameObject.SetActive(false);
    }

    private void EnableInfoText()
    {
        _taskPanel.rectTransform.DOAnchorPosY(250f, 1f);
        _infoPanel.SetActive(true);
        _joysticks.SetActive(false);
        _handCursor.gameObject.SetActive(false);

        _infoText.SetEntry(_infoTextKey[(int)_tutorial.CurrentTask]);
    }

    private void DisableInfoText()
    {
        _infoPanel.SetActive(false);
        _joysticks.SetActive(true);
        
        if(_tutorial.CurrentTask == Tutorial.Task.ThrowGrenade)
        {
            _handCursor.gameObject.SetActive(true);
        }
    }

    public void OnClickToContinue()
    {
        UpdateTask();
        DisableInfoText();
    }
}