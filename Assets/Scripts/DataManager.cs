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
    public GameObject[] river = new GameObject[5];

    static NetworkVariableSettings serverOnlyWriteSetting = new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.ServerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        };

    public NetworkList<string> riverCards = new NetworkList<string>(serverOnlyWriteSetting);

    public NetworkVariableInt playerNum = new NetworkVariableInt(serverOnlyWriteSetting);

    public NetworkList<GameObject> players = new NetworkList<GameObject>(serverOnlyWriteSetting);

    public NetworkList<ulong> playerIds = new NetworkList<ulong>(serverOnlyWriteSetting);

    public NetworkList<string> deck = new NetworkList<string>(serverOnlyWriteSetting);

    //keep server only - leave empty on client side
    public int[] seatOrder = new int[7];
    public List<int> playerOrder = new List<int>();

    private static readonly string[] suits = new string[] { "Heart", "Spade", "Diamond", "Club"};
    private static readonly string[] values = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13"};

    public bool gameActive = false;

    public float time;
    public int maxPlayers;

    enum Stage
    {
        Wait,
        WaitEnd,
        Deal,
        Flop1,
        Flop2,
        Flop3,
        End
    }

    Stage currentStage;
    Stage prevStage;

    public int smallBlind;
    public int bigBlind;
    public int mainPot;
    public int sidePot;
    public int currentBet;

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
                orderPlayers(false);
                if(playerOrder.Count >= 2){
                    gameActive = true;
                    currentStage = Stage.Deal;
                    time = NetworkManager.Singleton.NetworkTime;
                }
            } 

            if(gameActive && NetworkManager.Singleton.NetworkTime > time) {
                updateClientParams();

                if(currentStage == Stage.WaitEnd) {
                    endTurnClientRpc(playerOrder[0], clientRpcParams);
                    playerOrder.RemoveAt(0);
                    if(playerOrder.Count < 1) {
                        if(prevStage == Stage.Flop1) {
                            currentStage = Stage.Flop2;
                        }
                        else if (prevStage == Stage.Flop2) {
                            currentStage = Stage.Flop3;
                        }
                        else if (prevStage == Stage.Flop3) {
                            currentStage = Stage.End;
                        }
                        else if (prevStage == Stage.Deal) {
                            currentStage = Stage.Flop1;
                        }
                        if(currentStage != Stage.End){
                            orderPlayers(true);
                            resetBetStateClientRpc(clientRpcParams);
                        }
                    }
                    else {
                        currentStage = Stage.Wait;
                    }
                }

                if(currentStage == Stage.Deal) {
                    dealCardsClientRpc(clientRpcParams);
                    orderPlayers(false);
                    deal();
                    prevStage = currentStage;
                    currentStage = Stage.Wait;
                }

                if( currentStage == Stage.Flop1) {
                    flop1();
                    flop1ClientRpc(clientRpcParams);

                    orderPlayers(true);
                    prevStage = currentStage;
                    currentStage = Stage.Wait;
                }

                if( currentStage == Stage.Flop2) {
                    flop2();
                    flop2ClientRpc(clientRpcParams);

                    orderPlayers(true);
                    prevStage = currentStage;
                    currentStage = Stage.Wait;
                }

                if( currentStage == Stage.Flop3) {
                    flop3();
                    flop3ClientRpc(clientRpcParams);

                    orderPlayers(true);
                    prevStage = currentStage;
                    currentStage = Stage.Wait;
                }

                if(currentStage == Stage.Wait) {
                    nextTurnClientRpc(playerOrder[0]);  
                    currentStage = Stage.WaitEnd;                  
                }

                if( currentStage == Stage.End) {
                    endStage();
                    endStageClientRpc(clientRpcParams);
                    orderSeats();
                    orderPlayers(false);
                    prevStage = currentStage;
                    currentStage = Stage.Deal;
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
            if(playerIds.Contains(id)){
                this.playerNum.Value--;
            }
        }
    }

    public int getPlayerNum() {
        return playerNum.Value;
    }

    public void updateClientParams() {
        clientRpcParams.Send.TargetClientIds = new ulong[playerIds.Count];

        for(int i = 0; i < playerIds.Count; i++)
        {
            clientRpcParams.Send.TargetClientIds[i] = playerIds[i];
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

    public void orderPlayers(bool reOrder) {
        playerOrder.Clear();

        if(reOrder) {
            foreach (int id in seatOrder)
            {
                if(id != 0 && !GetPlayerNetworkObject((ulong)id).GetComponent<Player>().folded.Value) {
                    playerOrder.Add(id);
                }
            } 
        }
        else {
            foreach (int id in seatOrder)
            {
                if(id != 0) {
                    playerOrder.Add(id);
                }
            }
        }
    }

    [ClientRpc]
    public void nextTurnClientRpc(int id) {
        if(id == (int)NetworkManager.Singleton.LocalClientId) {
            buttons.SetActive(true);
            GetLocalPlayerObject().GetComponent<Player>().turn();
        }
    }

    [ClientRpc]
    public void endTurnClientRpc(int id, ClientRpcParams clientRpcParams) {
        if(id == (int)NetworkManager.Singleton.LocalClientId) {
            buttons.SetActive(false);
            GetLocalPlayerObject().GetComponent<Player>().turn();
        }
    }

    public void deal () {

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

    public void flop1() {
        for (int i = 0; i < 3; i++) {
            int r = UnityEngine.Random.Range(0, deck.Count);
            string card = deck[r];
            deck.RemoveAt(r);
            riverCards.Add(card);
        }
    }

    [ClientRpc]
    public void flop1ClientRpc(ClientRpcParams clientRpcParams) {
        for(int i = 0; i < 3; i++)
        {
            GameObject card = river[i];
            card.SetActive(true);
            Image cardImage = card.GetComponent<Image>();
            cardImage.sprite = Resources.Load<Sprite>("Cards/" + riverCards[i]);
        }
    }

    public void flop2() {
        int r = UnityEngine.Random.Range(0, deck.Count);
        string card = deck[r];
        deck.RemoveAt(r);
        riverCards.Add(card);
    }

    [ClientRpc]
    public void flop2ClientRpc(ClientRpcParams clientRpcParams) {
        GameObject card = river[3];
        card.SetActive(true);
        Image cardImage = card.GetComponent<Image>();
        cardImage.sprite = Resources.Load<Sprite>("Cards/" + riverCards[3]);
    }

    public void flop3() {
        int r = UnityEngine.Random.Range(0, deck.Count);
        string card = deck[r];
        deck.RemoveAt(r);
        riverCards.Add(card);
    }

    [ClientRpc]
    public void flop3ClientRpc(ClientRpcParams clientRpcParams) {
        GameObject card = river[4];
        card.SetActive(true);
        Image cardImage = card.GetComponent<Image>();
        cardImage.sprite = Resources.Load<Sprite>("Cards/" + riverCards[4]);
    }

    public void endStage() {
        generateDeck();
        shuffleDeck();
    }

    [ClientRpc]
    public void endStageClientRpc(ClientRpcParams clientRpcParams) {
        foreach(GameObject card in river) {
            card.SetActive(false);
        }
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void playerFoldServerRpc(ulong senderId) {
        time = NetworkManager.Singleton.NetworkTime; //force loop update
        Debug.Log("player fold called");
    }

    [ServerRpc]
    public void playerCallServerRpc(ulong senderID) {
        //
    }

    [ClientRpc]
    public void resetBetStateClientRpc(ClientRpcParams clientRpcParams) {
        GetLocalPlayerObject().GetComponent<Player>().resetBetState();
    }

    public void addPlayer(GameObject player) {
        if(IsServer) {
            players.Add(player);
            playerIds.Add((ulong)player.GetComponent<Player>().getPlayerID());
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
