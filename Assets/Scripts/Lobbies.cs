using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Connection;

public class Lobbies : NetworkedBehaviour
{ 
    public GameObject[] lobbies = new GameObject[15];
    
    private List<GameObject> connectedPlayers;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addPlayer(GameObject player){
        connectedPlayers.Add(player);
    }

    public void addPlayerToLobby(int lobby, GameObject player) {
        //--------------lobbies[lobby].GetComponent<DataManager>().addPlayer(player);
    }

    public GameObject getLobby(int i)
    {
        return lobbies[i];
    }

}
