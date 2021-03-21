﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Spawning;
using MLAPI.Messaging;
using MLAPI.Transports.UNET;
using MLAPI.Serialization.Pooled;
using System.Collections;
using System.Collections.Generic;
using System.IO;



public class Lobby : NetworkedBehaviour
{

    Lobbies lobbyManager;
    public GameObject playerPrefab;

    public bool clicked = false;

    // Start is called before the first frame update
    void Start()
    {
        lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void onClick() {
        if (isClient) {
            Debug.Log("Client ID: " + NetworkingManager.Singleton.LocalClientId);
        }
    }

    public void onOneClick() {

        GameObject.Find("Menu").SetActive(false);

        GameObject player = lobbyManager.getPlayerGoById(NetworkingManager.Singleton.LocalClientId);
        player.GetComponent<Player>().poo(lobbyManager.lobbies[0]);

    }

    public void onTwoClick() {

        GameObject player = lobbyManager.getPlayerGoById(NetworkingManager.Singleton.LocalClientId);
        player.GetComponent<Player>().poo(lobbyManager.lobbies[1]);
        GameObject.Find("Menu").SetActive(false);        
    }

}
