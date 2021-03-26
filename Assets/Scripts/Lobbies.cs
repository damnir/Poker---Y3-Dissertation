using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Connection;
//using MLAPI.NetworkedVar;

public class Lobbies : NetworkBehaviour
{ 
    public GameObject[] lobbies = new GameObject[15];
    
    public List<GameObject> connectedPlayers;

    public override void NetworkStart() 
    {

    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addPlayer(GameObject pp)
    {
        connectedPlayers.Add(pp);
    }

    public void addPlayerToLobby(int lobby, GameObject player)
    {
      
    }

    public GameObject getLobby(int i)
    {
        return lobbies[i];
    }


    public GameObject getPlayerGoById(ulong clientId) {
        foreach (GameObject player in connectedPlayers)
        {
            if ( player.GetComponent<Player>().getPlayerID() == (int)clientId ) {
                return player;
            }
        }
        return null;
    }

}
