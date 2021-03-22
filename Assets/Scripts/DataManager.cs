using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.NetworkedVar.Collections;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.NetworkedVar.Collections;

public class DataManager : NetworkedBehaviour
{
    private static int maxPlayers;

    private static GameObject[] river = new GameObject[5];

    [SyncedVar]
    public string[] deck = new string[52];
    [SyncedVar]
    public GameObject[] players = new GameObject[5];
    [SyncedVar]
    public int playerNum = 0;

    public static readonly string[] suits = new string[] { "Heart", "Spade", "Diamond", "Club"};
    public static readonly string[] values = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13"};

    void Start()
    {

    }

    public override void NetworkStart()
    {
        Debug.Log("NETWORK START - Data Manager");

        if(isOwner) {
            generateDeck();
            shuffleDeck();
        }

        if(isClient) {
            Debug.Log("Data Manager - is client");
            //InvokeServerRpc(clientStart);
        }

    }

    [ServerRPC]
    void clientStart() 
    {

    }

    [ClientRPC]
    void syncDeck(NetworkedVar<int> i)
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void addPlayer(GameObject player) {
        players[playerNum] = player;
        playerNum++;
    }

    public string nextFreeSeat(){
        if (playerNum == 1)
        {
            return ("Seat");
        }
        if (playerNum == 2)
        {
            return ("Seat (1)");
        }
        if (playerNum == 3)
        {
            return ("Seat (2)");
        }
        if (playerNum == 4)
        {
            return ("Seat (3)");
        }
        if (playerNum == 5)
        {
            return ("Seat (4)");
        }
        if (playerNum == 6)
        {
            return ("Seat (5)");
        }
        if (playerNum == 7)
        {
            return ("Seat (6)");
        }
        else
            return("na");
    }

    public void generateDeck()
    {
        int i = 0;
        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                deck[i] = s + v;
                i++;
                //deck.Add (s + v);
            }
        }
    }

    public void shuffleDeck()
    {
        for(int i = 51; i > 1; i--)
        {
            int j = Random.Range(0, 51);
            string temp1 = deck[i];
            string temp2 = deck[j];
            deck[i] = temp2;
            deck[j] = temp1;
        }
    }

}
