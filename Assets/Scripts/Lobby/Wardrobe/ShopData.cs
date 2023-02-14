using UnityEngine;

public class ShopData : MonoBehaviour
{
    //[SerializeField] SkinChanger _characterSkin;

    [SerializeField] private Transform _simpleItemsParent;
    [SerializeField] private GameObject _simpleItemPrefab;

    [SerializeField] private Color[] _simpleColors;
    [SerializeField] private Material[] _simpleColorsMaterials;

    public enum SkinParts
    {
        Head,
        Body
    }

    public enum ItemType
    {
        Simple,
        Model
    }

    public enum Simple
    {
        White,
        Red,
        Yellow,
        Black
    }

    private Simple[] _simpleItemsInShop =
    {
        Simple.White,
        Simple.Red,
        Simple.Yellow, 
        Simple.Black
    };

    private void Start()
    {
        for(int i = 0; i < _simpleItemsInShop.Length; i++)
        {
            ShopItem shopItem = Instantiate(_simpleItemPrefab, _simpleItemsParent).GetComponent<ShopItem>();
            int itemID = (int)_simpleItemsInShop[i];
            shopItem.SimpleTypeInit(this, _simpleColors[itemID], _simpleColorsMaterials[itemID]);
        }
    }

    public void SetSkinColor(Material material)
    {
        //_characterSkin.SetBodyMaterial(material, SkinChanger.BodyParts.Body);
    }
}
