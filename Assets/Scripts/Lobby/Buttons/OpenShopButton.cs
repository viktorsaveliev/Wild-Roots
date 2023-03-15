using UnityEngine;

public class OpenShopButton : MonoBehaviour
{
    [SerializeField] private GameObject _shop;
    [SerializeField] private GameObject _lobby;
    [SerializeField] private GameObject _character;

    public void OpenShop()
    {
        _shop.SetActive(true);
        _character.SetActive(false);
        _lobby.SetActive(false);
        EventBus.OnPlayerClickUI?.Invoke(0);
    }
}
