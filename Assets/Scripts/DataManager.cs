using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using static System.Action;
using System;
using static System.Exception;
using static MLAPI.Spawning.NetworkSpawnManager;

public class DataManager : NetworkBehaviour
{
    private static GameObject[] river = new GameObject[5];

    static NetworkVariableSettings serverOnlyWriteSetting = new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.ServerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        };

    public NetworkVariableInt playerNum = new NetworkVariableInt(serverOnlyWriteSetting);

    public NetworkList<GameObject> players = new NetworkList<GameObject>(serverOnlyWriteSetting);

    public NetworkList<ulong> playersId = new NetworkList<ulong>(serverOnlyWriteSetting);

    public NetworkList<string> deck = new NetworkList<string>(serverOnlyWriteSetting);

    //keep server only - leave empty on client side
    public int[] seatOrder = new int[7];
    public List<int> playerOrder = new List<int>();

    private static readonly string[] suits = new string[] { "Heart", "Spade", "Diamond", "Club"};
    private static readonly string[] values = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13"};

    public bool gameActive = false;

    public float time;
    public int maxPlayers;
    public int currentPlayer = 0;
    public int prevPlayer = 0;

    enum Stage
    {
        Wait,
        WaitEnd,
        Deal,
        Flop1,
        Flop2,
        Flop3
    }

    Stage currentStage;

    ClientRpcParams clientRpcParams = new ClientRpcParams();

    public GameObject buttons;

    void Start()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += clientDisconnect; //GETS CALLED ON ALL LOBBIES -- FIX
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

    // Update is called once per frame
    void Update()
    {
        if(IsServer) {

            if(playerNum.Value < 2) {
                gameActive = false;
            }
            if(!gameActive && playerNum.Value >= 2) {
                orderSeats();
                orderPlayers();
                if(playerOrder.Count >= 2){
                    gameActive = true;
                    currentStage = Stage.Deal;
                    time = NetworkManager.Singleton.NetworkTime;
                }
            } 

            if(gameActive && NetworkManager.Singleton.NetworkTime > time) {
                updateClientParams();

                if(currentStage == Stage.Deal) {
                    dealCardsClientRpc(clientRpcParams);
                    currentStage = Stage.Wait;
                    orderSeats();
                    orderPlayers();
                }
                ///
                ///...

                if(currentStage == Stage.WaitEnd) {
                    endTurnClientRpc(playerOrder[0], clientRpcParams);
                    playerOrder.RemoveAt(0);
                    if(playerOrder == null) {
                        currentStage = Stage.Flop1;
                    }
                    else {
                        currentStage = Stage.Wait;
                    }
                }

                if(currentStage == Stage.Wait) {
                    // endTurnClientRpc()
                    nextTurnClientRpc(playerOrder[0]);  
                    currentStage = Stage.WaitEnd;                  
                }

                time += 5;
            }

        }

        //re
        if(IsServer) {

        }
    }

    public void clientDisconnect(ulong id) {
        if(IsServer){
            Debug.Log("Client Disconnected. ID: " + id);
            //GameObject dp = players.Find(x => x.Contains.GetComponent<Player>().getPlayerID());
            if(playersId.Contains(id)){
                this.playerNum.Value--;
            }
        }
    }

    public int getPlayerNum() {
        return playerNum.Value;
    }

    public void updateClientParams() {
        clientRpcParams.Send.TargetClientIds = new ulong[playersId.Count];

        for(int i = 0; i < playersId.Count; i++)
        {
            clientRpcParams.Send.TargetClientIds[i] = playersId[i];
        }
    }

    //server only function
    public void orderSeats() {
        Array.Clear(seatOrder, 0, seatOrder.Length);

        foreach(GameObject player in players) {
            int seat = player.GetComponent<Player>().currentSeat.Value;
            int pId = player.GetComponent<Player>().getPlayerID();

            seatOrder[seat] = pId;
        }
    }

    public void orderPlayers() {
        playerOrder.Clear();

        foreach (int id in seatOrder)
        {
            if(id != 0) {
                playerOrder.Add(id);
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
    public void endTurnClientRpc(int id, ClientRpcParams clientRpcParams) {
        if(id == (int)NetworkManager.Singleton.LocalClientId) {
            buttons.SetActive(false);
        }
    }

    [ClientRpc]
    public void dealCardsClientRpc(ClientRpcParams clientRpcParams) {
        int r = UnityEngine.Random.Range(0, deck.Count);
        string card1 = deck[r];
        deck.RemoveAt(r);
        r = UnityEngine.Random.Range(0, deck.Count);
        string card2 = deck[r];
        deck.RemoveAt(r);
        GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().dealCards(card1, card2);
    }

    public void addPlayer(GameObject player) {
        if(IsServer) {
            players.Add(player);
            playersId.Add((ulong)player.GetComponent<Player>().getPlayerID());
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
            int j = UnityEngine.Random.Range(0, 51);
            string temp1 = deck[i];
            string temp2 = deck[j];
            deck[i] = temp2;
            deck[j] = temp1;
        }
    }

    public string evaluateHand(List<string> cards) {
        return null;
    }

}
