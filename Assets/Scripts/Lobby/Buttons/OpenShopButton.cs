using UnityEngine;

public class OpenShopButton : MonoBehaviour
{
    public void OpenShop()
    {
        Notice.ShowDialog(NoticeDialog.Message.ItShop);
    }
}
