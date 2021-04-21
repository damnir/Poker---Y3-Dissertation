using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using static MLAPI.Spawning.NetworkSpawnManager;
using TMPro;

public class GameInvite : MonoBehaviour
{
    public TMP_Text inviteText;
    public TMP_Text lobbyText;
    public GameObject newLobby;
    Lobbies lobbyManager;

    void Start()
    {
        lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();
    }
    public void onAcceptClick()
    {
        GetLocalPlayerObject().GetComponent<Player>().changeLobby(newLobby);
        foreach(GameObject room in lobbyManager.lobbies)
            {
                if (room.name != newLobby.name)
                {
                    room.SetActive(false);
                }
                else{
                    room.SetActive(true);
                }
            }

        Destroy(this);

    }

    public void onDeclineClick()
    {

    }

    public void setValues(string _username, string _lobby)
    {
                lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();

        inviteText.text = "Game Invite From: " + _username;
        lobbyText.text = _lobby;
        foreach(GameObject lobby in lobbyManager.lobbies)
        {
            if (lobby.name == _lobby)
            {
                newLobby = lobby;
            }
        }
    }

}
