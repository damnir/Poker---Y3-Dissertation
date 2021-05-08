using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Client : MonoBehaviour
{
    private static readonly string GameVersion = "pureholdem_v0.1_5760021302";
    void Start()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(GameVersion);
        NetworkManager.Singleton.StartClient();
    }
}
