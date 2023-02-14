using UnityEngine;

public class LoadingUI : MonoBehaviour 
{
    public static LoadingShower _loadingUI; // Facade

    private void Awake()
    {
        if (!LoadingShower.IsCreated) _loadingUI = GetComponent<LoadingShower>();
    }

    public static void Show(LoadingShower.Type type) => _loadingUI.Show(type);
    public static void Hide() => _loadingUI.Hide();
    public static void UpdateProgress(float progress) => _loadingUI.UpdateProgress(progress);
}
