using UnityEngine;
using MLAPI;
using static MLAPI.Spawning.NetworkSpawnManager;


public class Seat : MonoBehaviour
{
    public bool taken = false;
    public int seatNo;
    public GameObject betText;

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
        GameObject player = GetLocalPlayerObject().gameObject;
        player.GetComponent<Player>().sit(this.gameObject.name);

    }
}
