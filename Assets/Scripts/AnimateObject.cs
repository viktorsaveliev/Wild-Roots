using UnityEngine;
using DG.Tweening;

public class AnimateObject : MonoBehaviour
{
    private Tweener _tween;
    private bool _isStop;

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
        _tween = transform.DOMoveY(onUp ? 0.95f : 0.8f, 1f).SetEase(Ease.Linear).OnComplete(() => TweenAnimate(!onUp));
    }

    public void StopTweenAnimate()
    {
        _tween.Kill();
        _isStop = true;
    }

    private void AnimateWeaponWithTimer(Transform weapon, float lifetime)
    {
        if (lifetime == -1) return;
        weapon.DOScale(1.5f, lifetime-0.1f).SetEase(Ease.Linear);
        weapon.DOShakeRotation(lifetime-0.1f, 15, 15, 50, false);
    }
}
