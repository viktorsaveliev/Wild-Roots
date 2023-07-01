using System.Collections;
using UnityEngine;
using DG.Tweening;

public class RadiusIndicator : MonoBehaviour
{
    [SerializeField] private GameObject _owner;
    private SpriteRenderer _sprite;
    private bool _isEnable;
    private float _positionY;
    private Coroutine _timer;
    private int _charactersCountInRange;

    private Color _inRangeColor = new(1, 0, 0, 0.3f);
    private Color _outRangeColor = new(1, 1, 1, 0.2f);
    private Vector3 _scaleOnStart;

    private void Start()
    {
        _sprite = GetComponent<SpriteRenderer>();
        _scaleOnStart = transform.lossyScale;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_sprite == null) return;
        if (other.gameObject.layer == 6)
        {
            if (other.TryGetComponent<Character>(out _))
            {
                if (++_charactersCountInRange == 1)
                {
                    _sprite.color = _inRangeColor;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_sprite == null) return;
        if (other.gameObject.layer == 6)
        {
            if (other.TryGetComponent<Character>(out _)) //  && character.PhotonView.IsMine && character.IsABot == false
            {
                if (--_charactersCountInRange <= 0)
                {
                    _sprite.color = _outRangeColor;
                }
            }
        }
    }

    public void Show(float radius, float delay = 0)
    {
        if (_isEnable) return;
        gameObject.SetActive(true);

        if (delay == 0) EnableSprite(radius);
        else StartCoroutine(ShowWithDelay(radius, delay));
    }

    private IEnumerator ShowWithDelay(float radius, float delay)
    {
        if (_isEnable) yield break;

        yield return new WaitForSeconds(delay);
        EnableSprite(radius);
    }

    private void EnableSprite(float radius)
    {
        transform.localScale = _scaleOnStart;
        _sprite.enabled = true;
        transform.DOScale(Vector3.one * (radius + 1.7f) / _owner.transform.lossyScale.x, 1f); //   was0.7f / 4 - bomb scale

        float distance = 10f;
        if (Physics.Raycast(_owner.transform.position, Vector3.down, out RaycastHit hit, distance))
        {
            _positionY = hit.point.y + 0.1f;
        }

        _isEnable = true;
        _timer = StartCoroutine(Lifetimer());
    }

    private IEnumerator Lifetimer()
    {
        while (_isEnable)
        {
            transform.SetPositionAndRotation(new(_owner.transform.position.x, _positionY, _owner.transform.position.z), Quaternion.Euler(-90, 0, 0));
            yield return null;
        }
    }
    
    public void Hide()
    {
        _sprite.color = _outRangeColor;
        _charactersCountInRange = 0;
        _isEnable = false;
        StopCoroutine(_timer);
        _sprite.enabled = false;
        gameObject.SetActive(false);
    }
}
