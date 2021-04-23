using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using MLAPI.NetworkVariable.Collections;
using System;
using static MLAPI.Spawning.NetworkSpawnManager;
using System.Linq;
using TMPro;

public class DataManager : NetworkBehaviour
{
    public GameObject[] river = new GameObject[5];

    static NetworkVariableSettings serverOnlyWriteSetting = new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.ServerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        };

    public string[] riverCards = new string[5];

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
        End,
        Showdown,
        Forcewin
    }

    public struct Hand
    {
        public int rank; //1# royal flush, 2# straight flush, 3# four of a kind, 4# full house, #5 flush, #6 straight, #7 three of a kind, #8 two pair, #9 pair, #10 high card
        public int strength; //2-14 (2-A)
        public ulong pId;
        public string text;
    }

    public List<Hand> playerHands = new List<Hand>();

    public struct Rank
    {
        public List<int> pairs;
        public List<int> threeof;
        public int singlePairInt;
        public int pairsV;
        public int threeofV;
        public int fourof;
        public int straight;
        public int flush;
        public int straightflush;
        public int fullhouse;
        public int highCard;
    }

    Stage currentStage;
    Stage prevStage;

    public ulong smallBlind;
    public ulong bigBlind;
    public ulong sidePot;

    Game game = new Game();


    public NetworkVariable<ulong> currentBet = new NetworkVariable<ulong>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
        ReadPermission = NetworkVariablePermission.Everyone
    });
    public NetworkVariable<ulong> previousBet = new NetworkVariable<ulong>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
        ReadPermission = NetworkVariablePermission.Everyone
    });
    public NetworkVariable<ulong> mainPot = new NetworkVariable<ulong>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
        ReadPermission = NetworkVariablePermission.Everyone
    });

    public NetworkList<string> messages = new NetworkList<string>(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
        ReadPermission = NetworkVariablePermission.Everyone
    });

    ClientRpcParams clientRpcParams = new ClientRpcParams();

    public GameObject buttons;
    public GameObject pot;
    public TMP_Text handRankText;


    public bool actionTaken = false;

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
            messages.OnListChanged += newMessage;

        }

        if(IsClient) {
            Debug.Log("Data Manager - is client");
            //clientSideRpc = new ulong[]{NetworkManager.Singleton.LocalClientId };
        }
    }

    public void newMessage(NetworkListEvent<string> newMessage)
    {
        updateClientParams();
        newMessageClientRpc(newMessage.Value, clientRpcParams);
    }

    [ClientRpc]
    public void newMessageClientRpc(string _message, ClientRpcParams clientRpcParams)
    {
        ButtonManager.instance.updateMessages(_message);
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

                if(currentStage == Stage.WaitEnd) 
                {
                    endTurn();

                    if(playerOrder.Count < 1)
                    {
                        if(prevStage >= Stage.Flop1 && prevStage <= Stage.Flop3)
                        {
                            currentStage = prevStage+1;
                        }
                        else if (prevStage == Stage.Deal) {
                            currentStage = Stage.Flop1;
                        }
                        if(currentStage != Stage.End){
                            orderPlayers(true);
                            updatePotClientRpc(clientRpcParams);

                            if(currentStage != Stage.Forcewin && prevStage != Stage.Forcewin)
                            {
                                resetBetStateClientRpc(clientRpcParams);
                            }
                        }
                        playerOrderRe.Clear();
                    }
                    else
                    {
                        currentStage = Stage.Wait;
                    }
                }
               
                updateClientParams();

                if(prevStage != Stage.Forcewin)
                {
                    ulong fIpd = checkForceWin();
                    if(fIpd != 0)
                    {
                        GetPlayerNetworkObject(fIpd).GetComponent<Player>().winner(mainPot.Value);
                        currentStage = Stage.Forcewin;
                    }
                }

                switch (currentStage)
                {
 
                    case Stage.Deal:
                        hideCards();
                        deal();

                        prevStage = currentStage;
                        currentStage = Stage.Wait;
                        break;

                    case Stage.End:
                        endStage();

                        prevStage = currentStage;
                        currentStage = Stage.Showdown;
                        break;
                    
                    case Stage.Showdown:
                        flipCards();

                        determineWinner();
                        pushNewRound(); //DB GAME REPLAY
                        playerHands.Clear();
                        prevStage = currentStage;
                        currentStage = Stage.Deal; 
                        break;

                    case Stage.Flop1: case Stage.Flop2: case Stage.Flop3:
                        postFlop(currentStage);
                        orderPlayers(true);
                        prevStage = currentStage;
                        currentStage = Stage.Wait;
                        break;

                    case Stage.Forcewin:
                        flipCards();
                        prevStage = currentStage;
                        currentStage = Stage.Deal;
                        break;
                }
                
                if (currentStage == Stage.Wait)
                {
                    nextTurnClientRpc(playerOrder[0]);
                    currentStage = Stage.WaitEnd; 
                }
                
                time += 7;
                
            }
        }

    }

    public void clientDisconnect(ulong id) {

        if(IsServer){
            Debug.Log("Client Disconnected. ID: " + id);
            //GameObject dp = players.Find(x => x.Contains.GetComponent<Player>().getPlayerID());
            if(playerIds.Contains(id)){
                playerNum--;
                for(int i = 0; i < seatOrder.Length; i++) {
                    if(seatOrder[i] == id){
                        seatOrder[i] = 0;
                    }
                }
                int index = playerIds.FindIndex(a => a == id);
                players.RemoveAt(index);
                playerOrder.Remove(id);
                playerOrderRe.Remove(id);
                playerIds.Remove(id);
                
                LoginManager.instance.clientDisconnect(id);
                
                updateClientParams();
                if(playerIds.Count == 1)
                {
                    endTurnClientRpc(playerIds[0], clientRpcParams);

                }
            }

        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void sitUpServerRpc(ulong id)
    {
        Debug.Log("sitUp called for id: " + id);
        playerNum--;

        try {
            int index = playerIds.FindIndex(a => a == id);
            
            players.RemoveAt(index);
            playerOrder.Remove(id);
            playerOrderRe.Remove(id);
            playerIds.Remove(id);
            endTurnClientRpc(id, clientRpcParams);

        }catch(ArgumentOutOfRangeException e) {
            Debug.Log("Player never sat down - no need to remove from lobby");
        }
        
        if(playerIds.Count == 1)
        {
            endTurnClientRpc(playerIds[0], clientRpcParams);
            resetBetStateClientRpc(clientRpcParams);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void clientDisconnectServerRpc(ulong _id)
    {
        clientDisconnect(_id);
    }

    public int getPlayerNum() {
        return playerNum;
    }

    public void updateClientParams() {
        clientRpcParams.Send = new ClientRpcSendParams{ TargetClientIds = new ulong[playerIds.Count]};

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

            for(int i = 0; i < dealer-folded-1; i++){
                ulong temp = playerOrder[0];
                playerOrder.Add(temp);
                playerOrder.RemoveAt(0);
            }
        }
        else
        {
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

    public void flipCards()
    {
        foreach(GameObject player in players)
        {
            Player _player = player.GetComponent<Player>();
            _player.end.Value = true;
        }
    }

    public void hideCards()
    {
        foreach(GameObject player in players)
        {
            Player _player = player.GetComponent<Player>();
            _player.end.Value = false;
        }
    }

    [ClientRpc]
    public void forceFoldClientRpc(ulong id)
    {
        if(NetworkManager.Singleton.LocalClientId == id)
        {
            GetLocalPlayerObject().GetComponent<Player>().fold();
        }
    }

    [ClientRpc]
    public void forceCallClientRpc(ulong id)
    {
        if(NetworkManager.Singleton.LocalClientId == id)
        {
            GetLocalPlayerObject().GetComponent<Player>().call();
        }
    }

    [ClientRpc]
    public void updatePotClientRpc(ClientRpcParams clientRpcParams)
    {
        pot.GetComponent<Text>().text = "$"+mainPot.Value.ToString();
    }

    [ClientRpc]
    public void updateHandRankTextClientRpc(string text, ClientRpcParams clientRpcParams)
    {
        handRankText.text = text;
        Debug.Log("UPDATE ON CLIENT CALLED");
        Debug.Log("Hand rnak text : " + handRankText.text);
        Debug.Log("String: " + text);
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

    public void endTurn()
    {
        if(actionTaken)
        {
            endTurnClientRpc(playerOrder[0], clientRpcParams);
            if(!GetPlayerNetworkObject(playerOrder[0]).GetComponent<Player>().folded.Value)
            {
                playerOrderRe.Add(playerOrder[0]);//////////////////
            }
            playerOrder.RemoveAt(0);
        }
        else
        {
            if(GetPlayerNetworkObject(playerOrder[0]).GetComponent<Player>().currentBet.Value == currentBet.Value)        
            {
                forceCallClientRpc(playerOrder[0]);
            }
            else
            {
                forceFoldClientRpc(playerOrder[0]);
            }
        }
        actionTaken = false;
    }

    public void deal () {
        game.clearGame(); //GAME DB REPLAY
        Array.Clear(riverCards, 0, 5);
        updateHandRankTextClientRpc("", clientRpcParams);
        generateDeck();
        shuffleDeck();

        previousBet.Value = 0;
        endStageClientRpc(clientRpcParams);
        mainPot.Value = 0;
        updatePotClientRpc(clientRpcParams);

        currentBet.Value = bigBlind;
        orderSeats();
        orderPlayers(false);
        string card1;
        string card2;
        foreach(ulong id in playerOrder)
        {
            card1 = getRandomCard();
            card2 = getRandomCard();

            game.addPlayer(GetPlayerNetworkObject(id).GetComponent<Player>().netId.Value, card1 + "-" + card2);

            GetPlayerNetworkObject(id).GetComponent<Player>().dealCards(card1, card2);

        }
        dealer++;
        if(dealer > 6) {
            dealer = 0;
        }
        
        callBlindPlayer(smallBlind);
        callBlindPlayer(bigBlind);
    }

    public void postFlop(Stage stage)
    {
        currentBet.Value = 0;
        previousBet.Value = 0;
        string card;

        switch(stage){
            case Stage.Flop1:
                for (int i = 0; i < 3; i++)
                {
                    card = getRandomCard();
                    riverCards[i] = card;
                    game.addRiverCard(card);
                }  
                postFlopClientRpc(riverCards, 3, clientRpcParams);
                break;
            case Stage.Flop2:
                card = getRandomCard();
                game.addRiverCard(card);
                riverCards[3] = card;
                postFlopClientRpc(riverCards, 4, clientRpcParams);
                break;
            case Stage.Flop3:
                card = getRandomCard();
                game.addRiverCard(card);
                riverCards[4] = card;
                postFlopClientRpc(riverCards, 5, clientRpcParams);
                break;
        }
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
        resetBetStateClientRpc(clientRpcParams);
        updatePotClientRpc(clientRpcParams);
        generateDeck();
        shuffleDeck();
        playerEndClientRpc(riverCards, clientRpcParams);
        time -=(float)5.5;
    }

    [ClientRpc]
    public void endStageClientRpc(ClientRpcParams clientRpcParams) {
        foreach(GameObject card in river) {
            card.SetActive(false);
        }
    }

    [ClientRpc]
    public void playerEndClientRpc(string[] riverCards, ClientRpcParams clientRpcParams)
    {
        if(GetLocalPlayerObject().GetComponent<Player>().folded.Value == true)
        {
            return;
        }
        List<string> cards = new List<string>();
        cards.Add(GetLocalPlayerObject().GetComponent<Player>().card1v.Value);
        cards.Add(GetLocalPlayerObject().GetComponent<Player>().card2v.Value);
        foreach(string c in riverCards)
        {
            cards.Add(c);
        }
        Rank rank = evaluateHand(cards);
        Hand hand = evaluateRank(rank);

        addHandServerRpc(hand.pId, hand.rank, hand.strength, hand.text);
    }

    [ServerRpc(RequireOwnership = false)]
    public void addHandServerRpc(ulong id, int rank, int strength, string ranktext)
    {
        Hand newHand = new Hand();
        newHand.pId = id;
        newHand.rank = rank;
        newHand.strength = strength;
        newHand.text = ranktext;
        playerHands.Add(newHand);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void playerFoldServerRpc(ulong senderId) {
        game.addTurn(senderId.ToString(), "fold", "0"); //GAME DB REPLAY


        actionTaken = true;
        time = NetworkManager.Singleton.NetworkTime+(float)0.15; //force loop update
        Debug.Log("player fold called");
    }

    [ServerRpc(RequireOwnership = false)]
    public void playerCallServerRpc(ulong senderID, ulong bet) {
        game.addTurn(senderID.ToString(), "call", bet.ToString()); //GAME DB REPLAY

        actionTaken = true;

        mainPot.Value += bet;
        time = NetworkManager.Singleton.NetworkTime+(float)0.15; //force loop update
    }

    public void playerCall(ulong senderID, ulong bet){
        mainPot.Value += bet;
    }

    [ServerRpc(RequireOwnership = false)]
    public void playerRaiseServerRpc(ulong senderID, ulong call, ulong bet) {
        game.addTurn(senderID.ToString(), "raise", bet.ToString()); //GAME DB REPLAY

        actionTaken = true;

        mainPot.Value += call;
        currentBet.Value += bet;
        previousBet.Value = bet;

        foreach(ulong id in playerOrderRe) {
            if(!GetPlayerNetworkObject(id).GetComponent<Player>().folded.Value)
            {
                playerOrder.Add(id);
            }
        } 
        playerOrderRe.Clear();

        time = NetworkManager.Singleton.NetworkTime+(float)0.15; //force loop update
    }

    public void callBlindPlayer(ulong blind)
    {
        GetPlayerNetworkObject(playerOrder[0]).GetComponent<Player>().callBlind(blind);
        playerOrder.Add(playerOrder[0]);
        playerOrder.RemoveAt(0);
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

    public void addPlayerPL(ulong id)
    {
        players.Add(GetPlayerNetworkObject(id).gameObject);
        playerIds.Add(id);
        Debug.Log("Added player [ServerRpc]: Client ID: " + id);
        playerNum++;
        foreach(string cc in riverCards)
        {
            Debug.Log("Card: " + cc);
        }
    }

    [ClientRpc]
    public void SyncPreviousClientRpc(string[] rCards, ClientRpcParams clientRpcParams = default)
    {
        for(int i = 0; i < rCards.Length; i++)
        {
            Debug.Log("Card: " + rCards[i]);
            
            if(rCards[i] == "" || rCards[i] == null)
            {
                river[i].SetActive(false);
            }
            else
            {
                GameObject card = river[i];
                card.SetActive(true);
                Image cardImage = card.GetComponent<Image>();
                cardImage.sprite = Resources.Load<Sprite>("Cards/" + rCards[i]);
            }
        }
        pot.GetComponent<Text>().text = "$"+mainPot.Value.ToString();                
    }

    [ServerRpc(RequireOwnership = false)]
    public void SyncObjectsServerRpc(ulong[] _id)
    {
        SyncPreviousClientRpc(riverCards, new ClientRpcParams {
            Send = new ClientRpcSendParams {
                    TargetClientIds = _id
                }
            }
        );
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

    public Rank newRank()
    {
        Rank rank = new Rank();
        rank.pairs = new List<int>();
        rank.threeof = new List<int>();
        rank.fourof = 0;
        return rank;
    }

    public Rank evaluateHand(List<string> cards) {

        int len = cards.Count;
        int[] v = new int[len];
        string[] s = new string[len];

        for(int i = 0; i < len; i++)
        {
            string[] split = cards[i].Split('-');
            s[i] = split[0];
            int val = Int32.Parse(split[1]);
            if(val == 1)
            {
                v[i] = 14;
            }
            else
            {
                v[i] = val;
            }

        }

        int highCard;
        if(v[0] > v[1])
        {
            highCard = v[0];
        }
        else{
            highCard = v[1];
        }

        string[] sOg = (string[])s.Clone(); //for straight and royal flush
        Array.Sort(sOg);
        Array.Sort(v, s);

        Hand hand = new Hand();
        Rank rank = newRank();
        rank.highCard = highCard;

        int count = 0; int countS = 0; int countF = 0;
        //pairs         suits           flush
        if(v[0] == 2 && highCard == 14)
        {
            countS++; //for straight check...
        }

        for(int i = 1; i < len; i++)
        {
            //------ checking for duplicate values
            if(v[i] == v[i-1]){
                count++;
            }
            if(v[i] != v[i-1] || i == len-1)
            {
                if(count != 0)
                {
                    switch(count)
                    {
                        case 1: //one pair
                            rank.pairs.Add(v[i-1]);
                            break;
                        case 2: //three of a kind
                            rank.threeof.Add(v[i-1]);
                            break;
                        case 3: //four of a kind
                            rank.fourof = (v[i-1]);
                            Debug.Log("FOUR: " + v[i-1]);
                            break;
                            //RETURN - no need to check for anything else other than straight flush and royal flush
                    }
                    count = 0;
                }
            }
            //------- checking for flush
            if(sOg[i] == sOg[i-1]) //FLUSH FLAW
            {
                countF++;
                if(countF >= 4)
                {
                    Debug.Log("FLUSH!");
                    rank.flush = countF;
                }
            }
            else{
                countF = 0;
            }
            //------- checking for straight 
            if( (v[i-1]+1) == v[i] )
            {
                countS++;
                if(countS >= 4)
                {
                    Debug.Log("STRAIGHT!");
                    rank.straight = v[i]*5-10;;
                    //check for royalflush
                    if(countF >= 5)
                    {
                        string[] royal = new string[5];
                        Array.Copy(s, i-4, royal, 0, 5);

                        if(royal.All(ss => string.Equals(s[i], ss, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Debug.Log("STRAIGHT FLUSH!!");
                            //add all values together for strength
                            rank.straightflush = v[i]*5-10;
                            //45678
                        }

                    }
                }
            }
            else{
                if(count == 0)
                {
                    countS = 0;
                }
            }
        }

        //check for full house
        if(rank.pairs.Count >= 1 && rank.threeof.Count >= 1)
        {
            int value = 0;
            if(rank.pairs.Count > 1) //if multiple pairs, pick the highest value
            {
                rank.pairs.Sort((a, b) => b.CompareTo(a));
                value += rank.pairs[0];
            }
            if(rank.threeof.Count > 1)//if multiple pairs, pick the highest value
            {
                rank.threeof.Sort((a, b) => b.CompareTo(a));
                value += rank.threeof[0];
            }
            if(rank.pairs.Count == 1 && rank.threeof.Count == 1)
            {
                value += rank.pairs[0] + rank.threeof[0];
            }
            rank.fullhouse = value;
            
            Debug.Log("FULL HOUSE!");
        }

        if(rank.pairs.Count > 1) //if multiple pairs, pick the highest value
        {
            rank.pairs.Sort((a, b) => b.CompareTo(a));
        }
        else if(rank.pairs.Count == 1)
        {
            rank.singlePairInt = rank.pairs[0];
        }

        if(rank.threeof.Count > 1)//if multiple pairs, pick the highest value
        {
            rank.threeof.Sort((a, b) => b.CompareTo(a));
        }
        else if(rank.threeof.Count == 1)
        {
            rank.threeofV = rank.threeof[0];
        }

        //check for 2 pairs
        if(rank.pairs.Count > 1)
        {
            int value2 = 0;
            if(rank.pairs.Count > 2)
            {
                rank.pairs.Sort((a, b) => b.CompareTo(a));
            }
            value2 = rank.pairs[0] + rank.pairs[1];
            rank.pairsV = value2;
            Debug.Log("TWO PAIRS!");
        }
        else if(rank.pairs.Count == 1)
        {
            rank.singlePairInt = rank.pairs[0];
        }
        //check if new best hand DB

        return rank;
    }
 

//1# royal flush, 2# straight flush, 3# four of a kind, 4# full house, #5 flush, #6 straight, #7 three of a kind, #8 two pair, #9 pair, #10 high card
    public Hand evaluateRank(Rank rank)
    {
        Hand hand = new Hand();
        string handText;
        if(rank.straightflush != 0)
        {
            hand.text = "straight flush";
            hand.rank = 2;
            hand.strength = rank.straightflush;
        }
        else if(rank.fourof != 0)
        {
            hand.text = "four of a kind";
            hand.rank = 3;
            hand.strength = rank.fourof;
        }
        else if(rank.fullhouse != 0)
        {
            hand.text = "full house";
            hand.rank = 4;
            hand.strength = rank.fullhouse;
        }
        else if(rank.flush != 0)
        {
            hand.text = "flush";
            hand.rank = 5;
            hand.strength = rank.flush;
        }
        else if(rank.straight != 0)
        {
            hand.text = "straight";
            hand.rank = 6;
            hand.strength = rank.straight;
        }
        else if(rank.threeofV != 0)
        {
            hand.text = "three of a kind";
            hand.rank = 7;
            hand.strength = rank.threeofV;
        }
        else if(rank.pairsV != 0)
        {
            hand.text = "two pairs";
            hand.rank = 8;
            hand.strength = rank.pairsV;
        }
        else if(rank.singlePairInt != 0)
        {
            hand.text = "single pair";
            hand.rank = 9;
            hand.strength = rank.singlePairInt;
        }
        else
        {
            hand.text = "high card";
            hand.rank = 10;
            hand.strength = rank.highCard;
        }
        //handRankText.text = handText; dajspkidj alksjd oalskjdoasid jaosdj aios jdoi

        hand.pId = NetworkManager.Singleton.LocalClientId;
        return hand;

    }

    public void determineWinner()
    {
        playerHands.Sort((x,y) => x.rank.CompareTo(y.rank));
        List<Hand> winners = new List<Hand>();

        winners.Add(playerHands[0]);

        for (int i = 1; i < playerHands.Count; i++)
        {
            //Debug.Log("ID: " + playerHands[i].pId + " Rank: " + playerHands[i].rank + " Strength: " + playerHands[i].strength);
            if(playerHands[i].strength == playerHands[i-1].strength && playerHands[i].rank == playerHands[i-1].rank)
            {
                winners.Add(playerHands[i]);
            }
            else if(playerHands[i].strength > playerHands[i-1].strength && playerHands[i].rank == playerHands[i-1].rank)
            {
                winners.Clear();
                winners.Add(playerHands[i]);
            }
            else if(playerHands[i].rank != playerHands[i-1].rank)
            {
                break;
            }
        }

        foreach (Hand hand in winners)
        {
            Debug.Log("WINNER- ID: " + hand.pId + " Rank: " + hand.rank + " Strength: " + hand.strength);
        }

        announceWinner(winners);
    }

    public void announceWinner(List<Hand> winners)
    {
        updateClientParams();
        updateHandRankTextClientRpc(winners[0].text, clientRpcParams);
        
        int count = winners.Count;
        ulong win;
        if (count > 1)
        {
            win = mainPot.Value/(ulong)count;
        }
        else
        {
            win = mainPot.Value;
        }

        foreach(Hand hand in winners)
        {
            GetPlayerNetworkObject(hand.pId).GetComponent<Player>().winner(win);
        }
    }

    public ulong checkForceWin()
    {
        int folded = 0;
        ulong pId = 0;
        foreach(ulong id in seatOrder)
        {
            if(id != 0 && !GetPlayerNetworkObject(id).GetComponent<Player>().folded.Value)
            {
                folded ++;
                pId = id;
            }
        }

        if(folded == 1)
        {
            return pId;
        }
        return 0;
    }

    public class Game
    {
        //public List<string> pIds = new List<string>();
        public List<string> players = new List<string>();
        //public List<Dictionary<int, string>> round = new List<Dictionary<int, string>>();
        //IDictionary<int, string> turn = new Dictionary<int, string>();
        //public string[] turn = new string[3];
        public List<string> river = new List<string>();
        public List<string> round = new List<string>();

        public Game() 
        {
        }

        public void addTurn(string _id, string action, string bet)
        {
            round.Add(_id+"-"+action+"-"+bet);
        }

        public void addPlayer(string _id, string cards)
        {
            players.Add(_id+"-"+cards);
        }

        public void addRiverCard(string _card)
        {
            river.Add(_card);
        }

        public void clearGame()
        {
            players.Clear();
            round.Clear();
            river.Clear();
        }
        /*
        public Game(string _userNetId, string bet, string action) {
            
        }*/
    }

    public void pushNewRound()
    {
        StartCoroutine(LoginManager.instance.AddNewGame(game));
        Debug.Log("testDb called");  
    }

    /*
    public void testDBGame()
    {
        Game _game = new Game();

        _game.addPlayerId("2");
        _game.addPlayerId("3");
        _game.addPlayerId("4");
        _game.addTurn("2", "fold", "100");
        _game.addTurn("3", "call", "200");
        _game.addTurn("4", "call", "200");

    
        foreach(string[] poo in _game.round)
        {
            Debug.Log(poo[0] + poo[1] + poo[2]);
        }

        StartCoroutine(LoginManager.instance.AddNewGame(_game));
        Debug.Log("testDb called");

    }*/

}
