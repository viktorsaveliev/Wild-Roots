using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadAssets : MonoBehaviour
{
    private Dictionary<int, GameObject> _loadedSkins = new();
    private Dictionary<int, Sprite> _loadedSkinsIcon = new();

    public Dictionary<int, GameObject> GetLoadedSkin => _loadedSkins;
    public Dictionary<int, Sprite> GetLoadedSkinIcon => _loadedSkinsIcon;

    private Shader _shaderForChar;

    private void Awake()
    {
        if (!LoadingShower.IsCreated)
        {
            _shaderForChar = Shader.Find("Supyrb/Unlit/Texture"); // Universal Render Pipeline/Lit // Supyrb/Unlit/Texture
            DontDestroyOnLoad(this);
        }
    }

    public IEnumerator GetRandomSkin(Action<int> callback)
    {
        StringBus stringBus = new();

        using UnityWebRequest www = UnityWebRequest.Get(stringBus.GameDomain + "get_random_skin.php");
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Notice.Dialog(www.downloadHandler.error);
        }
        else
        {
            string jsonString = www.downloadHandler.text;
            SkinData skin = JsonConvert.DeserializeObject<SkinData>(jsonString);

            callback?.Invoke(skin.id);
        }
    }

    public void LoadSkin(int skinID, string url) => StartCoroutine(DownloadSkin(skinID, url));

    public IEnumerator DownloadSkin(int skinID, string url)
    {
        if (url.Length < 40 || _loadedSkins.ContainsKey(skinID)) yield break;

        using UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError ||
            www.result == UnityWebRequest.Result.ProtocolError)
        {
            Notice.Dialog(NoticeDialog.Message.ConnectionError);
        }
        else
        {
            if (_loadedSkins.ContainsKey(skinID)) yield break;
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

            GameObject prefab = bundle.LoadAsset<GameObject>("Model");
            UpdateMaterial(prefab);

            if(prefab.transform.Find("Icon").gameObject.TryGetComponent<SpriteRenderer>(out var icon))
            {
                _loadedSkinsIcon.Add(skinID, icon.sprite);
            }
            _loadedSkins.Add(skinID, prefab);
        }
    }

    private void UpdateMaterial(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                material.shader = _shaderForChar;
                //material.SetFloat("_Smoothness", 0f);
            }
        }
    }
}
