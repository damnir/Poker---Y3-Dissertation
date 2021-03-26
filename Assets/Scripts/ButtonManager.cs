using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using MLAPI.Transports.UNET;
using MLAPI.Serialization.Pooled;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ButtonManager : NetworkBehaviour
{
    public GameObject buttonServer;
    public GameObject buttonClient;
    public GameObject buttonLobby2;
    public GameObject lobbyList;
    public GameObject data;

    void Start() {
        //NetworkingManager.Singleton.StartServer();
    }

    public void onServerClicked()
    {
        NetworkManager.Singleton.StartServer();
        buttonServer.SetActive(false);
        buttonClient.SetActive(false);
    }

    public void onClientClicker()
    {
        lobbyList.SetActive(true);

        NetworkManager.Singleton.StartClient();
        buttonClient.SetActive(false);
        buttonServer.SetActive(false);
    }
}
