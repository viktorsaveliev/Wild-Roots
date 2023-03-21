using UnityEngine;

public class Notice : MonoBehaviour
{
    private static NoticeDialog _notice; // Facade

    private void Start()
    {
        if(!LoadingShower.IsCreated) _notice = GetComponent<NoticeDialog>();
    }

    public static void ShowDialog(NoticeDialog.Message message, INoticeAction action = null, string leftButtonTextKey = "Notice_Close", string rightButtonTextKey = null)
    {
        _notice.ShowDialog(message, action, leftButtonTextKey, rightButtonTextKey);
    }

    public static void ShowDialog(string message, INoticeAction action = null, string leftButtonTextKey = "Notice_Close")
    {
        _notice.ShowDialog(message, action, leftButtonTextKey);
    }

    public static void HideDialog()
    {
        _notice.HideDialog();
    }
}
