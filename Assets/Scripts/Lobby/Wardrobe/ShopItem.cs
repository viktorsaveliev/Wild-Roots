using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    private Image _image;
    private ShopData _shopData;
    private ShopData.ItemType _type;
    private Material _material;

    public void SimpleTypeInit(ShopData shopData, Color color, Material material)
    {
        _image = GetComponent<Image>();

        _shopData = shopData;
        _type = ShopData.ItemType.Simple;
        _image.color = color;
        _material = material;
    }

    public void SetSkin()
    {
        if (_type == ShopData.ItemType.Simple)
        {
            _shopData.SetSkinColor(_material);
        }
        else
        {

        }
    }
}
