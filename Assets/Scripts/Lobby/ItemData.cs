using UnityEngine;

public class ItemData : MonoBehaviour
{
    [SerializeField] private Sprite[] _backgroundForRarity;
    public Sprite[] GetRarityBackground => _backgroundForRarity;

    public readonly string[] RarityStringKey =
    {
        "Rarity_Regular",
        "Rarity_Rare",
        "Rarity_Legendary"
    };

    public enum Rarity
    {
        Regular,
        Rare,
        Legendary
    }

    public readonly Color[] RarityTextColor =
    {
        new Color(0.75f, 0.75f, 0.75f),
        new Color(0, 0.79f, 1f),
        new Color(1f, 0.83f, 0f)
    };


}

[System.Serializable]
public class SkinData
{
    public int id;
    public string name;
    public string url_fbx;
    public string url_icon;
    public int price;
    public int rarity;
}
