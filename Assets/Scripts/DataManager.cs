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
    public GameObject seat1;
    public GameObject seat2;
    public GameObject seat3;
    public GameObject seat4;
    public GameObject seat5;
    public GameObject seat6;
    public GameObject seat7;

    
    public bool CONNECTED = false;

    private static int maxPlayers;
    //keep gameobject references of all players that join the game
    private static GameObject[] seats = new GameObject[maxPlayers];
    //reference to script component of player
    private static Player player; 

    private static GameObject[] river = new GameObject[5];

    [SyncedVar]
    public string[] deck = new string[52];
    [SyncedVar]
    public GameObject[] players = new GameObject[5];
    [SyncedVar]
    public int playerNum = 0;

    public static readonly string[] suits = new string[] { "Heart", "Spade", "Diamond", "Club"};
    public static readonly string[] values = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13"};
    //public List<string> deck;

    Seat seat1S;
    Seat seat2S;
    Seat seat3S;
    Seat seat4S;
    Seat seat5S;
    Seat seat6S;
    Seat seat7S;


    // Start is called before the first frame update
    void Start()
    {
        //players = new NetworkedList<GameObject>();

        seat1S = seat1.GetComponent<Seat>();
        seat2S = seat2.GetComponent<Seat>();
        seat3S = seat3.GetComponent<Seat>();
        seat4S = seat4.GetComponent<Seat>();
        seat5S = seat5.GetComponent<Seat>();
        seat6S = seat6.GetComponent<Seat>();
        seat7S = seat7.GetComponent<Seat>();


        ///Debug.Log("Seat 1 taken? " + seat1S.isTaken());
        //Debug.Log("Seat 1 taken? " + seat1S.isTaken());

        /*
        if (isServer) {


            InvokeClientRpcOnEveryone(syncDeck, deck);
        }

        if (isClient) {
            InvokeServerRpc(clientStart);
        }*/


    }

    public override void NetworkStart() {
        Debug.Log("NETWORK START - Data Manager");

        if(isOwner) {
            generateDeck();
            shuffleDeck();
        }

        if(isClient) {
            Debug.Log("Data Manager - is client");
            InvokeServerRpc(clientStart);
        }

    }

    [ServerRPC]
    void clientStart() {

        /*
        TEST.Value ++;
        Debug.Log("server RPC: " + TEST);

        InvokeClientRpcOnEveryone(syncDeck, TEST);*/
    }

    [ClientRPC]
    void syncDeck(NetworkedVar<int> i) {
        /*
        Debug.Log("client RPC: " + TEST);

        //Debug.Log("Receiving client id: " + NetworkingManager.Singleton.LocalClientId);
        TEST = i;
        //deck = i;*/
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Test Value: " + TEST.Value);

            /*
            foreach (string c in deck)
            {
                Debug.Log(c);
            }*/

            //InvokeClientRpcOnEveryone(syncDeck, 1);
        
        /*if (isClient) {
            Debug.Log("CLIENT");
            Debug.Log(NetworkingManager.Singleton.LocalClientId);
        }
        if (isServer) {
            Debug.Log("SERVER");
        }*/
    }

    public void addPlayer(GameObject player) {
        players[playerNum] = player;
        playerNum++;
    }

    public void test(){
        //Debug.Log(seat1.isTaken());
    }

    public string nextFreeSeat(){
        if (playerNum == 1)
        {
            seat1S.setTaken(true);
            return ("Seat");
        }
        if (playerNum == 2)
        {
            seat2S.setTaken(true);
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

    public void setSeatTaken(bool newtaken, GameObject seat)
    {
        Seat temp = seat.GetComponent<Seat>();
        temp.setTaken(newtaken);

    }

/*
    public string getNameField()
    {
        string text = nameField.GetComponent<InputField>().text;
        //nameField.GetComponent<InputField>().text = "";
        return text;

    }*/

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
