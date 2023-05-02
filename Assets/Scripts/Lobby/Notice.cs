using UnityEngine;

public class Notice : MonoBehaviour
{
    private static NoticeDialog _notice; // Facade

    private void Start()
    {
        if(!LoadingShower.IsCreated) _notice = GetComponent<NoticeDialog>();
    }

    public static void Dialog(NoticeDialog.Message message, INoticeAction action = null, string leftButtonTextKey = "Notice_Close", string rightButtonTextKey = null)
    {
        _notice.Dialog(message, action, leftButtonTextKey, rightButtonTextKey);
    }

    public static void Dialog(string message, INoticeAction action = null, string leftButtonTextKey = "Notice_Close")
    {
        _notice.Dialog(message, action, leftButtonTextKey);
    }

    public static void HideDialog()
    {
        _notice.HideDialog();
    }

    public static void HideSimple()
    {
        _notice.HideSimple();
    }

    public static void Simple(NoticeDialog.Message message, bool success)
    {
        _notice.Simple(message, success);
    }
}
