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
        player.GetComponent<Player>().poo(GameObject.Find("Lobby01"));
        /*
        GameObject player = lobbyManager.getPlayerGoById(NetworkingManager.Singleton.LocalClientId);

        //NetworkedObject player = GetLocalPlayerObject();
        player.GetComponent<NetworkedObject>().transform.SetParent(GameObject.Find("Lobby01").transform);
        player.transform.SetParent(GameObject.Find("Lobby01").transform);

        //if (isClient) {
            //Player player = lobbyManager.getPlayerGoById(NetworkingManager.Singleton.LocalClientId);
            //player.transform.SetParent(GameObject.Find("Lobby01").transform);
            /*GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            go.GetComponent<NetworkedObject>().SpawnAsPlayerObject(NetworkingManager.Singleton.LocalClientId);
            //go.GetComponent<NetworkedObject>().Spawn();
            go.transform.SetParent(GameObject.Find("Lobby01").transform);
            NetworkedObject netObject = go.GetComponent<NetworkedObject>();
            netObject.NetworkShow(2);
            netObject.NetworkShow(3);
            netObject.NetworkShow(4);
            netObject.NetworkShow(5);*/
        //}
    }

    public void onTwoClick() {
        /*
        //if (isClient) {
        GameObject player = lobbyManager.getPlayerGoById(NetworkingManager.Singleton.LocalClientId);

        //NetworkedObject player = GetLocalPlayerObject();
        player.GetComponent<NetworkedObject>().transform.SetParent(GameObject.Find("Lobby02").transform);
        player.transform.SetParent(GameObject.Find("Lobby02").transform);

            /*
            //Player player = lobbyManager.getPlayerGoById(NetworkingManager.Singleton.LocalClientId);
            //player.transform.SetParent(GameObject.Find("Lobby02").transform);
            GameObject go = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
            go.GetComponent<NetworkedObject>().SpawnWithOwnership(NetworkingManager.Singleton.LocalClientId);
            go.transform.SetParent(GameObject.Find("Lobby02").transform);*/
        GameObject player = lobbyManager.getPlayerGoById(NetworkingManager.Singleton.LocalClientId);
        player.GetComponent<Player>().poo(GameObject.Find("Lobby02"));
        GameObject.Find("Menu").SetActive(false);        
    }


}
