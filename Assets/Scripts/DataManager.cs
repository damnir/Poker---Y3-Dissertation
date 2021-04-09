﻿using System.Collections;
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

    public string[] riverCards = new string[5];

    /*
    public NetworkList<string> deck = new NetworkList<string>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
        ReadPermission = NetworkVariablePermission.Everyone
    });*/

    public List<string> deck = new List<string>();

    public List<GameObject> players = new List<GameObject>();
    public List<ulong> playerIds = new List<ulong>();
    public int playerNum;

    //keep server only - leave empty on client side
    public ulong[] seatOrder = new ulong[7];
    public List<ulong> playerOrder = new List<ulong>();
    public List<ulong> playerOrderRe = new List<ulong>();

    private static readonly string[] suits = new string[] { "Heart", "Spade", "Diamond", "Club"};
    private static readonly string[] values = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13"};

    public bool gameActive = false;

    public float time;
    public int maxPlayers;
    public int dealer = 0;

    public enum Stage
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

    public ulong smallBlind;
    public ulong bigBlind;
    public ulong sidePot;

    public NetworkVariable<ulong> currentBet = new NetworkVariable<ulong>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
        ReadPermission = NetworkVariablePermission.Everyone
    });
    public NetworkVariable<ulong> mainPot = new NetworkVariable<ulong>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
        ReadPermission = NetworkVariablePermission.Everyone
    });

    ClientRpcParams clientRpcParams = new ClientRpcParams();

    public GameObject buttons;
    public GameObject pot;

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

            if(playerNum < 2) {
                gameActive = false;
            }
            if(!gameActive && playerNum >= 2) {
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
                    playerOrderRe.Add(playerOrder[0]);//////////////////
                    playerOrder.RemoveAt(0);
                    if(playerOrder.Count < 1)
                    {
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
                            updatePotClientRpc(clientRpcParams);
                        }
                        playerOrderRe.Clear();
                    }
                    else
                    {
                        currentStage = Stage.Wait;
                    }
                }

                if(currentStage == Stage.Deal) {
                    currentBet.Value = bigBlind;
                    orderSeats();
                    orderPlayers(false);
                    deal();

                    dealer++;
                    if(dealer > 6) {
                        dealer = 0;
                    }

                    prevStage = currentStage;
                    currentStage = Stage.Wait;
                }

                if( currentStage == Stage.Flop1) {
                    postFlop(Stage.Flop1);
                    orderPlayers(true);
                    prevStage = currentStage;
                    currentStage = Stage.Wait;
                }

                if( currentStage == Stage.Flop2) {
                    postFlop(Stage.Flop2);
                    orderPlayers(true);
                    prevStage = currentStage;
                    currentStage = Stage.Wait;
                }

                if( currentStage == Stage.Flop3) {
                    postFlop(Stage.Flop3);
                    orderPlayers(true);
                    prevStage = currentStage;
                    currentStage = Stage.Wait;
                }

                if(currentStage == Stage.Wait) {
                    nextTurnClientRpc(playerOrder[0]);
                    currentStage = Stage.WaitEnd;                  
                }

                if( currentStage == Stage.End) {
                    evaluateHand();
                    endStage();
                    endStageClientRpc(clientRpcParams);

                    prevStage = currentStage;
                    currentStage = Stage.Deal;
                }

                time += 8;
            }
        }
    }

    public void clientDisconnect(ulong id) {

        if(IsServer){
            Debug.Log("Client Disconnected. ID: " + id);
            //GameObject dp = players.Find(x => x.Contains.GetComponent<Player>().getPlayerID());
            if(playerIds.Contains(id)){
                this.playerNum--;
                for(int i = 0; i < seatOrder.Length; i++) {
                    if(seatOrder[i] == id){
                        seatOrder[i] = 0;
                    }
                }
                playerOrder.Remove(id);
                playerOrderRe.Remove(id);
                playerIds.Remove(id);
            }

        }
    }

    public int getPlayerNum() {
        return playerNum;
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
            ulong seat = (ulong)player.GetComponent<Player>().currentSeat.Value;
            ulong pId = player.GetComponent<Player>().getPlayerID();

            seatOrder[seat] = pId;
        }
    }

    public void orderPlayers(bool reOrder) {
        playerOrder.Clear();
        int folded = 0;
        if(reOrder) {
            foreach (ulong id in seatOrder)
            {
                if(id != 0 && !GetPlayerNetworkObject((ulong)id).GetComponent<Player>().folded.Value) {
                    playerOrder.Add(id);
                }
                else if (id != 0 && GetPlayerNetworkObject((ulong)id).GetComponent<Player>().folded.Value){
                    folded++;
                }
            } 
            playerOrder.Reverse();

            for(int i = 0; i < dealer-folded; i++){
                ulong temp = playerOrder[0];
                playerOrder.Add(temp);
                playerOrder.RemoveAt(0);
            }
        }
        else {
            foreach (ulong id in seatOrder)
            {
                if(id != 0) {
                    playerOrder.Add(id);
                }
            }
            
            playerOrder.Reverse();
            
            for(int i = 0; i < dealer; i++){
                ulong temp = playerOrder[0];
                playerOrder.Add(temp);
                playerOrder.RemoveAt(0);
            }
        }
    }
    [ClientRpc]
    public void updatePotClientRpc(ClientRpcParams clientRpcParams)
    {
        pot.GetComponent<Text>().text = "$"+mainPot.Value.ToString();
    }

    [ClientRpc]
    public void nextTurnClientRpc(ulong id) {
        if(id == NetworkManager.Singleton.LocalClientId) {
            buttons.SetActive(true);
            GetLocalPlayerObject().GetComponent<Player>().turn(true);
        }
    }

    [ClientRpc]
    public void endTurnClientRpc(ulong id, ClientRpcParams clientRpcParams) {
        if(id == NetworkManager.Singleton.LocalClientId) {
            buttons.SetActive(false);
            GetLocalPlayerObject().GetComponent<Player>().turn(false);
        }
    }

    public void deal () {
        //dealCardsClientRpc(clientRpcParams);
        string card1;
        string card2;
        foreach(ulong id in playerOrder)
        {
            card1 = getRandomCard();
            card2 = getRandomCard();
            GetPlayerNetworkObject(id).GetComponent<Player>().dealCards(card1, card2);
        }
    }

    /*
    [ClientRpc]
    public void dealCardsClientRpc(ClientRpcParams clientRpcParams) {
        string card1 = getRandomCard();
        string card2 = getRandomCard();
        GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().dealCards(card1, card2);
    }*/

    public void postFlop(Stage stage)
    {
        switch(stage){
            case Stage.Flop1:
                for (int i = 0; i < 3; i++)
                {
                    riverCards[i] = getRandomCard();
                }  
                postFlopClientRpc(riverCards, 3, clientRpcParams);
                break;
            case Stage.Flop2:
                riverCards[3] = getRandomCard();
                postFlopClientRpc(riverCards, 4, clientRpcParams);
                break;
            case Stage.Flop3:
                riverCards[4] = getRandomCard();
                postFlopClientRpc(riverCards, 5, clientRpcParams);
                break;
        }
        currentBet.Value = 0;
    }

    [ClientRpc]
    public void postFlopClientRpc(string[] cards, int stage, ClientRpcParams clientRpcParams) {
        if(GetLocalPlayerObject().GetComponent<Player>().currentLobby.Value == this.gameObject)
        {
            for(int i = 0; i < stage; i++)
            {
                GameObject card = river[i];
                card.SetActive(true);
                Image cardImage = card.GetComponent<Image>();
                cardImage.sprite = Resources.Load<Sprite>("Cards/" + cards[i]);
            }
        }
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
        //Debug.Log("River Cards Count(CLIENT): " + riverCards.Count);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void playerFoldServerRpc(ulong senderId) {
        time = NetworkManager.Singleton.NetworkTime; //force loop update
        //playerOrder.Remove(senderId);
        //playerOrderRe.Remove(senderId);
        Debug.Log("player fold called");
    }

    [ServerRpc(RequireOwnership = false)]
    public void playerCallServerRpc(ulong senderID, ulong bet) {
        mainPot.Value += bet;
        time = NetworkManager.Singleton.NetworkTime; //force loop update
    }

    [ServerRpc(RequireOwnership = false)]
    public void playerRaiseServerRpc(ulong senderID, ulong bet) {
        mainPot.Value += bet;
        currentBet.Value += bet;

        foreach(ulong id in playerOrderRe) {
            if(!GetPlayerNetworkObject(id).GetComponent<Player>().folded.Value)
            {
                playerOrder.Add(id);
            }
        } 
        playerOrderRe.Clear();

        time = NetworkManager.Singleton.NetworkTime; //force loop update
    }

    [ClientRpc]
    public void resetBetStateClientRpc(ClientRpcParams clientRpcParams) {
        GetLocalPlayerObject().GetComponent<Player>().resetBetState();
    }

    [ServerRpc(RequireOwnership = false)]
    public void addPlayerServerRpc(ulong id)
    {
        players.Add(GetPlayerNetworkObject(id).gameObject);
        playerIds.Add(id);
        Debug.Log("Added player [ServerRpc]: Client ID: " + id);
        playerNum++; 
    }

    public void addPlayer(GameObject player) {
        if(IsServer) {
            players.Add(player);
            playerIds.Add((ulong)player.GetComponent<Player>().getPlayerID());
            Debug.Log("Added player: " + player.name);
            playerNum++;
        }
    }

    public void generateDeck()
    {
        deck.Clear();
        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                deck.Add(s + "-" + v);
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

    public string getRandomCard() {
        int r = UnityEngine.Random.Range(0, deck.Count);
        string card = deck[r];
        deck.RemoveAt(r);
        return card;
    }

    public string evaluateHand() {

        //Replace 7 with cards.len later and also in loops ennit
        string[,] asd = new string[7, 2]; //split into [card][suit, value]
        List<int> v = new List<int>();
        List<string> s = new List<string>();


        int i = 0;
        foreach(string card in riverCards) {
            string[] split = card.Split('-');
            asd[i,0] = split[0];
            asd[i,1] = split[1];
            v.Add(Int32.Parse(asd[i,1]));
            i++;
        }

        v.Sort();
        int highCard = v[v.Count-1];

        int count = 0;
        
        for(i = 1; i < v.Count; i++)
        {
            if(v[i] == v[i-1]){
                count++;
            }
            else {
                Debug.Log("Value: " + v[i-1] + " count: " + count);
                count = 0;
            }

            if(i == v.Count-1){
                Debug.Log("Value: " + v[i-1] + " count: " + count);
            }

        }


        Debug.Log("HIGH CARD: " + highCard);

        return null;
    }

}
