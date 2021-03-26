using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class Seat : MonoBehaviour
{
    public bool taken = false;
    public int seatNo;

    Lobbies lobbyManager;

    void Start() {
        lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();
    }

    void OnMouseDown()
    {
        sit();
    }

    public bool isTaken(){
        return taken;
    }

    public void setTaken(bool newTaken)
    {
        taken = newTaken;
    }

    public void sit() {
        GameObject player = lobbyManager.getPlayerGoById(NetworkManager.Singleton.LocalClientId);
        player.GetComponent<Player>().sit(this.gameObject.name);
    }
}
