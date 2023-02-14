using UnityEngine;
using UnityEngine.UI;

public class RawAnimation : MonoBehaviour
{
    [SerializeField] private float[] speed;
    private RawImage _background;
    
    void Start()
    {
        _background = GetComponent<RawImage>();
    }

    private void FixedUpdate()
    {
        _background.uvRect = new Rect(_background.uvRect.x + speed[0], _background.uvRect.y + speed[1], 1, 1);
    }
}