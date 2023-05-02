using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class AnimateObject : MonoBehaviour
{
    [SerializeField] private bool _freezePosition;

    private Tweener _tween;
    private bool _isStop;
    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
        _isStop = false;
        TweenAnimate();
    }

    private void OnEnable()
    {
        _isStop = false;
        TweenAnimate();
        EventBus.OnPlayerShoot += AnimateWeaponWithTimer;
    }

    private void OnDisable()
    {
        StopTweenAnimate();
        EventBus.OnPlayerShoot -= AnimateWeaponWithTimer;
    }

    private void TweenAnimate(bool onUp = true)
    {
        if (_isStop) return;
        if(_tween != null)
        {
            _tween.Kill();
            _tween = null;
        }

        if (_freezePosition == false)
        {
            _tween = transform.DOMoveY(onUp ? 0.95f : 0.8f, 1f).SetEase(Ease.Linear).OnComplete(() => TweenAnimate(!onUp));
        } 
        else
        {
            if(_image != null)
                _tween = _image.rectTransform.DOAnchorPosY(onUp ? _image.rectTransform.localPosition.y + 15f : _image.rectTransform.localPosition.y - 15f, 1f)
                    .SetEase(Ease.Linear).OnComplete(() => TweenAnimate(!onUp));
        }
    }

    public void StopTweenAnimate()
    {
        _tween.Kill();
        _tween = null;
        _isStop = true;
    }

    private void AnimateWeaponWithTimer(Transform weapon, float lifetime)
    {
        if (lifetime == -1) return;
        weapon.DOScale(1.5f, lifetime-0.1f).SetEase(Ease.Linear);
        weapon.DOShakeRotation(lifetime-0.1f, 15, 15, 50, false);
    }
}
