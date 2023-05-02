using UnityEngine;
using UnityEngine.UI;

public class RegionsButton : MonoBehaviour
{
    private Image _image;

    private void Start()
    {
        _image = GetComponent<Image>();
    }

    public void SwitchRegionUI(Sprite sprite)
    {
        _image.sprite = sprite;
    }
}
