using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assets : MonoBehaviour
{
    private static LoadAssets _assetLoader;

    private void Awake()
    {
        if (!LoadingShower.IsCreated) _assetLoader = GetComponent<LoadAssets>();
    }

    public static void LoadSkin(int skinID, string url) => _assetLoader.LoadSkin(skinID, url);
    public static IEnumerator GetRandomSkin(Action<int> callback) => _assetLoader.GetRandomSkin(callback);
    public static IEnumerator DownloadSkin(int skinID, string url) => _assetLoader.DownloadSkin(skinID, url);

    public static Dictionary<int, GameObject> GetLoadedSkin => _assetLoader.GetLoadedSkin;
    public static Dictionary<int, Sprite> GetLoadedSkinIcon => _assetLoader.GetLoadedSkinIcon;
}
