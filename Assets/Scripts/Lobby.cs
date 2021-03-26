using System.Collections;
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
using MLAPI.NetworkVariable;
using System;
using UnityEngine.UI;

public class Lobby : NetworkBehaviour
{

    Lobbies lobbyManager;
    public GameObject playerPrefab;
    public GameObject lobby;
    public GameObject text;


    public bool clicked = false;

    // Start is called before the first frame update
    void Start()
    {
        lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();
    }

    // Update is called once per frame
    void Update()
    {
        try {
            Text tt = text.GetComponent<Text>();
            tt.text = lobby.name +  " " + lobby.GetComponent<DataManager>().getPlayerNum() + "/7";
        }catch(NullReferenceException e) { }
        
    }

    public void onClick() {
        if(IsClient) {
            GameObject.Find("Menu").SetActive(false);

            GameObject player = lobbyManager.getPlayerGoById(NetworkManager.Singleton.LocalClientId);
            player.GetComponent<Player>().changeLobby(lobby);

            foreach(GameObject room in lobbyManager.lobbies)
            {
                if (room.name != lobby.name)
                {
                    room.SetActive(false);
                }
            }
        }
    }

}
