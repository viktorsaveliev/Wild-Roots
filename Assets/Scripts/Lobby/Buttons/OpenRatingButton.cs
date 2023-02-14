using UnityEngine;

public class OpenRatingButton : MonoBehaviour
{
    public void OpenRating()
    {
        Notice.ShowDialog(NoticeDialog.Message.Rating);
    }
}
