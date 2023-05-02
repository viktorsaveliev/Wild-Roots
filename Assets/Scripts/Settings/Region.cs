using System.Collections;
using UnityEngine;
using Photon.Pun;

public class Region : MonoBehaviour
{
    [SerializeField] private Sprite[] _buttonSprites;
    [SerializeField] private RegionsButton[] _buttons;

    private void Start()
    {
        if (PhotonNetwork.CloudRegion == "eu")
        {
            _buttons[0].SwitchRegionUI(_buttonSprites[0]);
            _buttons[1].SwitchRegionUI(_buttonSprites[1]);
        }
        else
        {
            _buttons[1].SwitchRegionUI(_buttonSprites[0]);
            _buttons[0].SwitchRegionUI(_buttonSprites[1]);
        }
    }

    public void SwitchRegion(int id) => StartCoroutine(StartSwitchRegion(id));
    private IEnumerator StartSwitchRegion(int id)
    {
        PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
        {
            yield return null;
        }

        if (id == 0) PhotonNetwork.ConnectToRegion("eu");
        else PhotonNetwork.ConnectToRegion("us");

        while (!PhotonNetwork.IsConnected)
        {
            yield return null;
        }

        if(id == 0)
        {
            _buttons[0].SwitchRegionUI(_buttonSprites[0]);
            _buttons[1].SwitchRegionUI(_buttonSprites[1]);
        }
        else
        {
            _buttons[1].SwitchRegionUI(_buttonSprites[0]);
            _buttons[0].SwitchRegionUI(_buttonSprites[1]);
        }

        EventBus.OnPlayerClickUI?.Invoke(1);
    }
}
