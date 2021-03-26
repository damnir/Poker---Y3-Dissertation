using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;

public class DataManager : NetworkBehaviour
{
    private static int maxPlayers;

    private static GameObject[] river = new GameObject[5];

    public NetworkVariableInt playerNum = new NetworkVariableInt();

    public static readonly string[] suits = new string[] { "Heart", "Spade", "Diamond", "Club"};
    public static readonly string[] values = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13"};

    public NetworkList<string> deck = new NetworkList<string>();

    public bool gameActive = false;

    public int currentPlayer = 0;
    public int prevPlayer = 0;

    public GameObject buttons;

    public NetworkList<GameObject> players = new NetworkList<GameObject>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.ServerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public float time;

    void Start()
    {
    }

    public override void NetworkStart()
    {
        Debug.Log("NETWORK START - Data Manager");
        time = NetworkManager.Singleton.NetworkTime;

        if(IsServer) {
            generateDeck();
            shuffleDeck();
        }

        if(IsClient) {
            Debug.Log("Data Manager - is client");
        }

    }

    public int getPlayerNum() {
        return playerNum.Value;
    }

    // Update is called once per frame
    void Update()
    {
        if(IsServer) {

            if(!gameActive && playerNum.Value >= 2) {
                gameActive = true;
                time = NetworkManager.Singleton.NetworkTime;
            } 
            if(gameActive && NetworkManager.Singleton.NetworkTime > time) {
                endTurnClientRpc(players[prevPlayer].GetComponent<Player>().getPlayerID());
                nextTurnClientRpc(players[currentPlayer].GetComponent<Player>().getPlayerID());
                prevPlayer = currentPlayer;
                currentPlayer++;
                if(currentPlayer >= playerNum.Value){
                    currentPlayer = 0;
                }
                time += 5;
            }
        }
    }

    [ClientRpc]
    public void nextTurnClientRpc(int id) {
        if(id == (int)NetworkManager.Singleton.LocalClientId) {
            buttons.SetActive(true);
        }
    }

    [ClientRpc]
    public void endTurnClientRpc(int id) {
        if(id == (int)NetworkManager.Singleton.LocalClientId) {
            buttons.SetActive(false);
        }
    }

    public void addPlayer(GameObject player) {
        if(IsServer) {
            players.Add(player);
            Debug.Log("Added player: " + player.name);
            playerNum.Value++;
        }
    }

    public void generateDeck()
    {
        int i = 0;
        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                deck.Add(s + v);
                i++;
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
