using UnityEngine;

public class SwipeRotate : MonoBehaviour
{
    [SerializeField] private GameObject _object;
    private readonly float _rotationSpeed = 200f;

    private Vector2 _startPos;
    private bool _isSwipe;

    private void OnEnable()
    {
        _isSwipe = false;
        _object.transform.rotation = Quaternion.Euler(0, 180f, 0);
        //_object.transform.Rotate(new Vector3(0, 180f, 0));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _startPos = Input.mousePosition;
            _isSwipe = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _isSwipe = false;
        }

        if (_isSwipe)
        {
            Vector2 swipe = (Vector2)Input.mousePosition - _startPos;
            float swipeMagnitude = swipe.magnitude;

            if (swipeMagnitude > 20f)
            {
                float swipeDirection = Mathf.Sign(swipe.x);
                _object.transform.Rotate(Vector3.down, _rotationSpeed * swipeDirection * Time.deltaTime);
            }
        }
    }
}