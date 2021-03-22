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
        player.GetComponent<Player>().changeLobby(lobbyManager.lobbies[0]);

        foreach(GameObject room in lobbyManager.lobbies)
        {
            if (room.name != "Lobby01")
            {
                room.SetActive(false);
            }
        }

    }

    public void onTwoClick() {

        GameObject player = lobbyManager.getPlayerGoById(NetworkingManager.Singleton.LocalClientId);
        player.GetComponent<Player>().changeLobby(lobbyManager.lobbies[1]);
        GameObject.Find("Menu").SetActive(false);

        foreach (GameObject room in lobbyManager.lobbies)
        {
            if (room.name != "Lobby02")
            {
                room.SetActive(false);
            }
        }
    }

}
