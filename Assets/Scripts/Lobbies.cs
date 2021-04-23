using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Connection;
//using MLAPI.NetworkedVar;

public class Lobbies : NetworkBehaviour
{ 
    public GameObject[] lobbies = new GameObject[5];
    
    public List<GameObject> connectedPlayers;

    void Start()
    {
        if(IsServer)
        {
            foreach(GameObject lobby in lobbies)
            {
                lobby.SetActive(true);
            }
        }
    }

    public void addPlayer(GameObject pp)
    {
        connectedPlayers.Add(pp);
    }

    public GameObject getLobby(int i)
    {
        return lobbies[i];
    }

    public GameObject getPlayerGoById(ulong clientId) {
        foreach (GameObject player in connectedPlayers)
        {
            if ( player.GetComponent<Player>().getPlayerID() == clientId ) {
                return player;
            }
        }
        return null;
    }

}
