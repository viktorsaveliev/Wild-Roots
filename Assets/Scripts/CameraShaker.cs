using UnityEngine;
using DG.Tweening;

public class CameraShaker : MonoBehaviour
{
    private Camera _camera;
    private Tween tween;
    private Quaternion _startRotation;

    private void Start()
    {
        _camera = Camera.main;
        _startRotation = transform.rotation;
        //_camera.DOShakeRotation(2f, 0.1f, 2, 1, false).SetLoops(-1);
    }
    private void OnEnable()
    {
        EventBus.OnWeaponExploded += CameraShake;
    }
    private void OnDisable()
    {
        EventBus.OnWeaponExploded -= CameraShake;
    }

    private void CameraShake(GameObject weapon)
    {
        if (tween != null) _camera.DOKill();
        tween = _camera.DOShakeRotation(0.3f, 2).OnComplete(() =>
        {
            transform.DORotateQuaternion(_startRotation, 0.2f);
            //transform.DORotate(new Vector3(55.69f, 0, 0), 0.2f);
        });
    }
}
