using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ControllViewer : MonoBehaviour
{
    [SerializeField] private Image _wasdKeys;
    private readonly int _loopsCount = 10;
    private readonly float _animDuration = 0.4f;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);

        _wasdKeys.rectTransform.DOScale(1, 1f);

        yield return new WaitForSeconds(1f);

        StringBus stringBus = new();
        if(PlayerPrefs.GetInt(stringBus.PlayerDevice) == 1)
        {
            _wasdKeys.transform.localScale = Vector2.zero;
            _wasdKeys.gameObject.SetActive(true);
            StartCoroutine(LoopAnimation());
        }
    }

    private IEnumerator LoopAnimation()
    {
        int currentLoopsCount = 0;
        bool upScale = false;
        while(currentLoopsCount < _loopsCount)
        {
            currentLoopsCount++;
            upScale = !upScale;
            AnimateImage(upScale);
            
            yield return new WaitForSeconds(_animDuration);
        }

        _wasdKeys.rectTransform.DOScale(0, 1f).OnComplete(() =>
        {
            _wasdKeys.gameObject.SetActive(false);
        });
    }

    private void AnimateImage(bool upScale)
    {
        _wasdKeys.rectTransform.DOScale(upScale ? 1 : 0.9f, _animDuration);
    }
}
