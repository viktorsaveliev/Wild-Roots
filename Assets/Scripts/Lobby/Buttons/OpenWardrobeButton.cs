using UnityEngine;

public class OpenWardrobeButton : MonoBehaviour
{
    public void OpenWardrobe()
    {
        Notice.ShowDialog(NoticeDialog.Message.Wardrobe);
        EventBus.OnPlayerClickUI?.Invoke(0);
    }
}
