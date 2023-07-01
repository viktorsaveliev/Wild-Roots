using Photon.Pun;

public class GameData
{
    public readonly string GameVersion = "v0.05.19";
    public readonly int SkinsCount = 7;

    public void CallMethod<T>(string methodName, PhotonView photonView, RpcTarget rpc, params object[] args)
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.OfflineMode == false)
        {
            photonView.RPC(methodName, rpc, args);
        }
        else
        {
            var method = typeof(T).GetMethod(methodName);
            if (method != null) method.Invoke(null, args);
        }
    }
}