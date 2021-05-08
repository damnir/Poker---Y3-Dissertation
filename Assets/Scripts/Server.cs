using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;

public class Server : MonoBehaviour
{
    private static readonly string GameVersion = "pureholdem_v0.1_5760021302";
    GameObject PlayerPrefab;

    void Start()
    {
        Setup();
    }

    private void Setup() 
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartServer();
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientId,
        MLAPI.NetworkManager.ConnectionApprovedDelegate callback)
    {
        string clientGameVersion = 
            System.Text.Encoding.ASCII.GetChars(connectionData).ToString();
        bool approve = false;
        bool createPlayerObject = false;

        if ( clientGameVersion == GameVersion )
        {
            approve = true;
            createPlayerObject = true;
        }

        ulong? prefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("Player");
        
        callback(createPlayerObject, prefabHash, approve, 
            PlayerPrefab.transform.position, PlayerPrefab.transform.rotation);
    }
}
