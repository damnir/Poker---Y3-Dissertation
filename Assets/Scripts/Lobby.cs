using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using System;
using UnityEngine.UI;
using static MLAPI.Spawning.NetworkSpawnManager;


public class Lobby : NetworkBehaviour
{

    Lobbies lobbyManager;
    public GameObject playerPrefab;
    public GameObject lobby;
    public GameObject text;
    public GameObject errorMessage;

    DataManager dm;
    public bool clicked = false;

    // Start is called before the first frame update
    void Start()
    {
        lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();
        dm = lobby.GetComponent<DataManager>();
    }

    // Update is called once per frame
    void Update()
    {
        try {
            text.GetComponent<Text>().text = lobby.name + " | Players: " + dm.playerNumNet.Value +"/7 | $" + dm.smallBlind+ "/$" +dm.bigBlind;

        }catch(NullReferenceException e) { }
        
    }

    public void onClick() {
        if(IsClient) {
            GameObject.Find("MainGame").GetComponent<Canvas>().enabled = true;
            if(GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().cash.Value < lobby.GetComponent<DataManager>().bigBlind*2)
            {
                StartCoroutine(showErrorMessage());
                return;
            }


            if(GameObject.Find("LoginScreen") != null)
            {
                return;
            }

            GameObject.Find("Menu").SetActive(false);

            Player player = GetLocalPlayerObject().GetComponent<Player>();

            player.changeLobby(lobby);


        }
    }

    public IEnumerator showErrorMessage()
    {
        errorMessage.SetActive(true);

        yield return new WaitForSeconds(3);

        errorMessage.SetActive(false);

    }

}
