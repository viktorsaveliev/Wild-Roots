using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadingShower : MonoBehaviour
{
    [SerializeField] private GameObject[] _loading;
    [SerializeField] private Text _loadingPercent;
    [SerializeField] private Image _progressBar;
    private int _loadingOpened;
    private readonly int _timeForCancel = 10;

    private Coroutine _timer;
    public static bool IsCreated = false;

    public enum Type
    {
        Simple,
        Progress
    }

    private void Start()
    {
        if(IsCreated)
        {
            Destroy(gameObject);
        }
        else
        {
            _loadingOpened = -1;
            DontDestroyOnLoad(gameObject);
            IsCreated = true;
        }
    }

    public void Show(Type type)
    {
        if (_loadingOpened != -1) return;
        _loadingPercent.text = "0";
        _progressBar.fillAmount = 0;

        _loadingOpened = (int)type;
        _loading[(int)type].SetActive(true);

        if(_timer != null) StopCoroutine(_timer);
        _timer = StartCoroutine(ShowErrorMessage());
    }

    public void Hide()
    {
        if (_loadingOpened == -1) return;

        StopCoroutine(_timer);
        _loading[_loadingOpened].SetActive(false);
        _loadingOpened = -1;
    }

    public void UpdateProgress(float progress)
    {
        if (_loadingOpened == -1) return;

        _loadingPercent.text = $"{progress * 100}";
        _progressBar.fillAmount = progress;

        /*if (progress >= 1f) // 1f
        {
            Hide();
        }*/
    }

    private IEnumerator ShowErrorMessage()
    {
        yield return new WaitForSeconds(_timeForCancel);
        Hide();
        Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
    }
}
