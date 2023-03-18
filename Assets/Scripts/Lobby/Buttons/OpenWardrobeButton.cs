using UnityEngine;

public class OpenWardrobeButton : MonoBehaviour
{
    [SerializeField] private GameObject _wardrobe;
    [SerializeField] private GameObject _lobby;
    //[SerializeField] private GameObject _character;

    public void OpenWardrobe()
    {
        _wardrobe.SetActive(true);
        //_character.SetActive(false);
        _lobby.SetActive(false);
        EventBus.OnPlayerClickUI?.Invoke(0);
    }
}
