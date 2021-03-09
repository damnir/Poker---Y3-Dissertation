using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Connection;

public class ButtonManager : NetworkedBehaviour
{
    public GameObject buttonServer;
    public GameObject buttonClient;

    public GameObject data;
    int test = 0;

    public void onServerClicked()
    {
        NetworkingManager.Singleton.StartServer();
        buttonServer.SetActive(false);
        buttonClient.SetActive(false);

    }

    public void onClientClicker()
    {
        NetworkingManager.Singleton.StartClient();
        //test ++;
        //Debug.Log("Clients connected: " + test);
        buttonClient.SetActive(false);
        buttonServer.SetActive(false);

    }

}
