using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Messaging;
using static MLAPI.Spawning.NetworkSpawnManager;
using TMPro;

public class GameInvite : MonoBehaviour
{
    public TMP_Text inviteText;
    public TMP_Text lobbyText;
    //public GameObject newLobby;
    /*
    public NetworkVariable<GameObject> newLobby = new NetworkVariable<GameObject>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        });*/

    public string lobby;

    Lobbies lobbyManager;

    void Start()
    {
        lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();
    }
    public void onAcceptClick()
    {
        GetLocalPlayerObject().GetComponent<Player>().acceptInvite(lobby);
        this.gameObject.SetActive(false);
        //Destroy(this.gameObject);

    }

/*
    [ServerRpc(RequireOwnership = false)]
    public void setLobbyServerRpc(string _lobby)
    {
        lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();
        
        foreach(GameObject lobby in lobbyManager.lobbies)
        {
            if (lobby.name == _lobby)
            {
                newLobby.Value = lobby;
                Debug.Log("LOBBY FOUND");
                break;
            }
        }
    }*/

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
