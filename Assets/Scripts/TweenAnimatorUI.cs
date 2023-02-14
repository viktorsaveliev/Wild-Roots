using UnityEngine;
//using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.UI;

public class TweenAnimatorUI : MonoBehaviour
{
    [SerializeField] private bool _changePos = true;
    [SerializeField] private Vector2 _endedPos;
    [SerializeField] private Events _event;

    [SerializeField] private Ease _easeAnim;
    [SerializeField] private float _duration;
    [SerializeField] private bool _disableOnComplete;

    [SerializeField] private bool _changeColor;
    [SerializeField] private Color _endedColor;

    private RectTransform _target;

    enum Events
    {
        SearchMatch,
        Start
    }

    private void Start()
    {
        _target = GetComponent<RectTransform>();
        if (_event == Events.Start) Animate();
    }

    private void OnEnable()
    {
        if (_event == Events.SearchMatch) EventBus.OnPlayerStartSearchMatch += Animate;
    }

    private void OnDisable()
    {
        if (_event == Events.SearchMatch) EventBus.OnPlayerStartSearchMatch -= Animate;
    }

    private void Animate()
    {
        if(_changePos)
        {
            _target.DOAnchorPos(_endedPos, _duration).SetEase(_easeAnim).SetDelay(0.1f).OnComplete(() =>
            {
                if (_disableOnComplete) _target.gameObject.SetActive(true);
            });
        }
        if(_changeColor)
        {
            Image image = _target.GetComponent<Image>();
            RawImage rawImage = _target.GetComponent<RawImage>();

            if(image != null) image.DOColor(_endedColor, _duration);
            else rawImage.DOColor(_endedColor, _duration);
        }
    }
}
