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
using TMPro;
using static MLAPI.Spawning.NetworkSpawnManager;


public class ButtonManager : NetworkBehaviour
{
    public GameObject buttonServer;
    public GameObject buttonClient;
    public GameObject buttonLobby2;
    public GameObject lobbyList;
    public GameObject data;
    public GameObject foldButton;
    public GameObject callButton;
    public GameObject raiseButton;
    public GameObject slider;
    public GameObject callText;
    public GameObject raiseText;
    public GameObject checkText;

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

    public void onFoldClicked()
    {
        GetLocalPlayerObject().GetComponent<Player>().fold();
    }

    public void onCallClicked()
    {
        GetLocalPlayerObject().GetComponent<Player>().call();

    }

    public void onRaiseClicked() 
    {
        GetLocalPlayerObject().GetComponent<Player>().raise();
    }

    public void onSliderChanged()
    {

    }

    public void updateCall(ulong value)
    {
        if (value > 0)
        {
            checkText.GetComponent<TextMeshProUGUI>().text = "<sprite=3>Call";
        }
        else
        {
            checkText.GetComponent<TextMeshProUGUI>().text = "<sprite=1>Check";
        }
        callText.GetComponent<Text>().text = "$" + value;
    }

    public void updateRaise()
    {

    }
}
