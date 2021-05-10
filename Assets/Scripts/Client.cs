using UnityEngine;
using MLAPI;

public class Client : MonoBehaviour
{
    private static readonly string GameVersion = "pureholdem_v0.1_5760021302";
    //handshake KEY
    void Start()
    {
        //perform handshake before connecting to the server
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(GameVersion);
        NetworkManager.Singleton.StartClient();
    }
}
