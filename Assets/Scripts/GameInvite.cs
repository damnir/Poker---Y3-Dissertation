using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using static MLAPI.Spawning.NetworkSpawnManager;
using TMPro;
using System;

public class GameInvite : MonoBehaviour
{
    public TMP_Text inviteText;
    public TMP_Text lobbyText;

    public string lobby;

    Lobbies lobbyManager;
    Player player;

    void Start()
    {
        lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();
        player = this.GetComponentInParent<Player>();
        if(player.gameState.Value == Player.GameState.Menu)
        {
            this.transform.SetParent(GameObject.Find("Menu").transform);
        }
        
    }
    public void onAcceptClick()
    {
        GameObject.Find("MainGame").GetComponent<Canvas>().enabled = true;

        this.transform.SetParent(player.gameObject.transform);

        GetLocalPlayerObject().GetComponent<Player>().accept(lobby.Substring(0, lobby.IndexOf(" ")));
        this.gameObject.SetActive(false);
        //GameObject.Find("Menu").SetActive(false);
        //Destroy(this.gameObject);
    }

    public void onDeclineClick()
    {
        this.gameObject.SetActive(false);

    }

    public void setValues(string _username, string _lobby)
    {
                //lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();

        inviteText.text = "Game Invite From: " + _username;
        lobbyText.text = _lobby;
        lobby = _lobby;


        //setLobbyServerRpc(_lobby);

    }

}
