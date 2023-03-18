using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class LoadAssets : MonoBehaviour
{
    private Dictionary<int, GameObject> _loadedSkins = new();
    public Dictionary<int, GameObject> GetLoadedSkin => _loadedSkins;

    public void LoadAsset(int skinID, string url) => StartCoroutine(DownloadSkin(skinID, url));

    public IEnumerator DownloadSkin(int skinID, string url)
    {
        if (url.Length < 40 || _loadedSkins.ContainsKey(skinID)) yield break;

        using UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError ||
            www.result == UnityWebRequest.Result.ProtocolError)
        {
            Notice.ShowDialog(NoticeDialog.Message.ConnectionError);
        }
        else
        {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);

            GameObject prefab = bundle.LoadAsset<GameObject>("Model");
            UpdateMaterial(prefab);

            _loadedSkins.Add(skinID, prefab);
        }
    }

    private void UpdateMaterial(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                material.shader = shader;
            }
        }
    }
}
