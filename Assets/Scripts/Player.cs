using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using TMPro;
using static MLAPI.Spawning.NetworkSpawnManager;

public class Player : NetworkBehaviour
{
    public enum BetState 
    {
        Wait,
        Fold,
        Check,
        Raise,
        Call,
        Win
    }

    public enum GameState
    {
        Menu,
        Spec,
        Ingame
    }

    public static NetworkVariableSettings netVarSettings = new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone,
            ReadPermission = NetworkVariablePermission.Everyone
        };

    [Header("Synced Variables")]
    public NetworkVariable<ulong> clientID = new NetworkVariable<ulong>(netVarSettings);
    public NetworkVariableVector3 Position = new NetworkVariableVector3(netVarSettings);
    public NetworkVariableString pos = new NetworkVariableString(netVarSettings);
    public NetworkVariable<GameObject> currentLobby = new NetworkVariable<GameObject>(netVarSettings);
    public NetworkVariableInt currentSeat = new NetworkVariableInt(netVarSettings);
    public NetworkVariable<ulong> cash = new NetworkVariable<ulong>(netVarSettings);
    public NetworkVariable<ulong> currentBet = new NetworkVariable<ulong>(netVarSettings);
    public NetworkVariable<bool> folded = new NetworkVariable<bool>(netVarSettings);
    public NetworkVariable<bool> isTurn = new NetworkVariable<bool>(netVarSettings);
    public NetworkVariableString betGoText = new NetworkVariableString(netVarSettings);
    public NetworkVariable<BetState> betState = new NetworkVariable<BetState>(netVarSettings);
    public NetworkVariableString card1v = new NetworkVariableString(netVarSettings);
    public NetworkVariableString card2v = new NetworkVariableString(netVarSettings);
    public NetworkVariableString netId = new NetworkVariableString(netVarSettings);
    public NetworkVariableString username = new NetworkVariableString(netVarSettings);
    public NetworkVariableBool end = new NetworkVariableBool(netVarSettings);
    public NetworkVariable<GameState> gameState = new NetworkVariable<GameState>(netVarSettings);

    [Header("Client Side Variables")]
    public ulong lobbyBet;
    public GameObject foldGo;
    public GameObject card1;
    public GameObject card2;
    public GameObject betGo;
    public GameObject animation;
    public GameObject ownerGo;
    public GameObject win;
    public GameObject GameInvite;

    GameObject text;

    public GameObject buttonManager;

    public ulong[] clientSideRpc;

    void Start()
    {
        buttonManager = GameObject.Find("ButtonManager");
        text = betGo.transform.Find("Text").gameObject;
        this.name = "PL" + OwnerClientId;

        if(IsOwner){
            clientID.Value = OwnerClientId;

            Text t1 = GameObject.Find("CLIENT_TEXT").GetComponent<Text>();
            Text t2 = GameObject.Find("CLIENT_TEXT2").GetComponent<Text>();

            cash.Value = 0;

            t1.text = "Client ID: " + OwnerClientId;
            t2.text = "Client ID: " + OwnerClientId;
            ownerGo.SetActive(true);
            betState.Value = BetState.Fold;
            clientSideRpc = new ulong[]{NetworkManager.Singleton.LocalClientId };

        }

        GameObject.Find("Lobbies").GetComponent<Lobbies>().addPlayer(this.gameObject);
        this.transform.SetParent(GameObject.Find("Lobbies").transform);
        
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
  
            //this.transform.SetParent(GameObject.Find("Menu").transform, false);
            this.transform.position = GameObject.Find("PlayerPlaceHolder").transform.position;
        

        changeText("Name", ("Client ID: " + OwnerClientId.ToString()) );
    }

    public ulong getPlayerID () { return clientID.Value; }

    // Update is called once per frame
    void Update()
    {   
        if(IsOwner && currentLobby.Value != null)
        {
            currentLobby.Value.GetComponent<DataManager>().SyncObjectsServerRpc(clientSideRpc);
        }

        try {
                    if(pos.Value != null)
        {
            this.transform.position = GameObject.Find(pos.Value).transform.position; 
        }
            //this.transform = GameObject.Find(pos.Value).transform;
            lobbyBet = currentLobby.Value.GetComponent<DataManager>().currentBet.Value;
        }catch(NullReferenceException e) { }

        
        if(currentLobby.Value != null)
        {
            this.transform.SetParent(currentLobby.Value.transform);
            this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        try {
            if(IsOwner)
            {
                GameObject.Find("UsernameText").GetComponent<Text>().text = username.Value;
                GameObject.Find("CashText").GetComponent<Text>().text = "$" +cash.Value.ToString();
            }
        }catch(NullReferenceException e) { }


        if(IsClient) {

            changeText("Cash", "$" + cash.Value.ToString());

            switch (betState.Value) {  //pls make this less messy it do be really ugluyy
                case BetState.Fold: 
                    foldGo.SetActive(true);
                    betGoText.Value = "<sprite=2>$"+currentBet.Value;
                    betGo.SetActive(true);
                    card1.SetActive(false);
                    card2.SetActive(false);
                    win.SetActive(false);
                    break;
                case BetState.Call:
                    //betGoText.Value = "<sprite=3>$$$";
                    
                    if(currentBet.Value < 1)
                    {
                        betGoText.Value = "<sprite=1>Check";
                    }
                    betGo.SetActive(true);
                    foldGo.SetActive(false);
                    card1.SetActive(true);
                    card2.SetActive(true);  
                    win.SetActive(false);
                    break;
                case BetState.Raise:
                    //betGoText.Value = "<sprite=0>$$$";
                    betGo.SetActive(true);
                    foldGo.SetActive(false);
                    card1.SetActive(true);
                    card2.SetActive(true);
                    win.SetActive(false);
                    break;
                case BetState.Wait:
                    betGo.SetActive(false);
                    foldGo.SetActive(false);
                    card1.SetActive(true);
                    card2.SetActive(true);
                    win.SetActive(false);
                    break;
                case BetState.Win:
                    win.SetActive(true);
                    betGo.SetActive(true);
                    break;
            }
            
            if(isTurn.Value == true) {
                animation.SetActive(true);
                if (IsOwner)
                {
                    if(cash.Value < 1)
                    {
                        buttonManager.GetComponent<ButtonManager>().updateRaise(0, 0);
                        buttonManager.GetComponent<ButtonManager>().updateCall(0);
                        cash.Value = 0;
                    }
                    buttonManager.GetComponent<ButtonManager>().updateCall(lobbyBet - currentBet.Value);
                    if(currentLobby.Value.GetComponent<DataManager>().previousBet.Value == 0)
                    {
                        buttonManager.GetComponent<ButtonManager>().updateRaise(currentLobby.Value.GetComponent<DataManager>().bigBlind,
                        (lobbyBet - currentBet.Value) + currentLobby.Value.GetComponent<DataManager>().mainPot.Value);
                    }
                    else
                    {
                        buttonManager.GetComponent<ButtonManager>().updateRaise(currentLobby.Value.GetComponent<DataManager>().previousBet.Value,
                        (lobbyBet - currentBet.Value) + currentLobby.Value.GetComponent<DataManager>().mainPot.Value);
                    }
                }
            }
            else{
                animation.SetActive(false);
            }

            text.GetComponent<TextMeshProUGUI>().text = betGoText.Value;
        }

        if(IsOwner && betState.Value != BetState.Fold && card1v.Value != null)
        {
            card1.transform.localScale = new Vector3(1.4f, 1.4f, 1.0f);
            card1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" + card1v.Value);

            card2.transform.localScale = new Vector3(1.4f, 1.4f, 1.0f);
            card2.GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" + card2v.Value);
        }

        if(!IsOwner && betState.Value != BetState.Fold && end.Value)
        {
            card1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" + card1v.Value);
            card2.GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" + card2v.Value);
            card1.transform.localScale = new Vector3(1.4f, 1.4f, 1.0f);
            card2.transform.localScale = new Vector3(1.4f, 1.4f, 1.0f);
        }
        else if (!IsOwner && betState.Value != BetState.Fold && !end.Value)
        {
            card1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/Back");
            card2.GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/Back");  
            card1.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            card2.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }
        if(currentLobby.Value == null)
        {
            gameState.Value = GameState.Menu;
        }
        else{
            gameState.Value = GameState.Ingame;
        }
        changeText("Name", username.Value );

    }

    public void resetState()
    {
    }

    public void updateCash(int newCash)
    {
        cash.Value = (ulong)newCash;
    }

    public void setNetId(string id)
    {
        netId.Value = id;
    }

    public void setUsername(string _username)
    {
        username.Value = _username;
    }

    public void setCash(ulong newCash)
    {
        cash.Value = newCash;
    }

    void changeText(string objectName, string newText)
    {
        Transform objectTransform = gameObject.transform.Find(objectName);
        GameObject objectReference = objectTransform.gameObject;

        Text objectText = objectReference.GetComponent<Text>();
        objectText.text = newText;
    }

    public void dealCards(string c1, string c2) {
        if(cash.Value < currentLobby.Value.GetComponent<DataManager>().bigBlind)
        {
            sitUp();
            return;
        }

        LoginManager.instance.updateHandsPlayed(netId.Value, 1);

        card1.transform.localScale = new Vector3(1.4f, 1.4f, 1.0f);
        card1.GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" + c1);

        card2.transform.localScale = new Vector3(1.4f, 1.4f, 1.0f);
        card2.GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" + c2);
        //currentBet.Value = currentLobby.Value.GetComponent<DataManager>().currentBet.Value;
        card1v.Value = c1;
        card2v.Value = c2;

        folded.Value = false;
        betState.Value = BetState.Wait;
    }

    public void changeLobby(GameObject lobby) 
    {
        acceptInvite(lobby.name);
    }

    //---------------------------------------------------------------
    //Refactored

    public void sit(string name) {
        if(IsOwner) {
            currentLobby.Value.GetComponent<DataManager>().addPlayerServerRpc(OwnerClientId);

            GameObject seat = GameObject.Find(name);
            Position.Value = seat.transform.position;
            currentSeat.Value = seat.GetComponent<Seat>().seatNo;
            pos.Value = name;
        }
    }

    public void sitUp()
    {
        if(currentLobby.Value != null)
        {
            currentLobby.Value.GetComponent<DataManager>().sitUpServerRpc(OwnerClientId);
            // foreach(GameObject card in currentLobby.Value.GetComponent<DataManager>().river)
            // {
            //     card.SetActive(false);
            // }

        }
        Position.Value = GameObject.Find("PlayerPlaceHolder").transform.position;
        currentSeat.Value = 0;
        pos.Value = "PlayerPlaceHolder";
        betState.Value = BetState.Fold;
        isTurn.Value = false;
        folded.Value = true;
    }

    ////GAME 
    //sprites: 0 = raise, 1 = check, 2 = fold, 3 = call
    public void fold() {
        folded.Value = true;
        currentLobby.Value.GetComponent<DataManager>().playerFoldServerRpc(OwnerClientId);

        betState.Value = BetState.Fold;
    }

    public void check() {
                isTurn.Value = false;

        betState.Value = BetState.Check;
    }

    public void call() {
        isTurn.Value = false;

        currentLobby.Value.GetComponent<DataManager>().playerCallServerRpc(OwnerClientId, lobbyBet - currentBet.Value);
        cash.Value -= lobbyBet - currentBet.Value;
        currentBet.Value = lobbyBet;

        betGoText.Value = "<sprite=3>"+(currentBet.Value);
        betState.Value = BetState.Call;
        dbUpdateCash();
    }

    public void raise(ulong bet) {
        isTurn.Value = false;

        currentLobby.Value.GetComponent<DataManager>().playerRaiseServerRpc(OwnerClientId, lobbyBet + bet - currentBet.Value, bet);
        cash.Value -= lobbyBet + bet - currentBet.Value;
        currentBet.Value = lobbyBet + bet;

        betGoText.Value = "<sprite=0>$"+ (lobbyBet + bet);
        betState.Value = BetState.Raise;
        dbUpdateCash();
    }

    public void resetBetState() {
        currentBet.Value = 0;

        if(betState.Value != BetState.Fold && betState.Value != BetState.Win)
        {
            betState.Value = BetState.Wait;
        }  
    }

    public void callBlind(ulong bet)
    {
        currentBet.Value = bet;
        cash.Value -= bet;
        betGoText.Value = "<sprite=3>$"+currentBet.Value;
        currentLobby.Value.GetComponent<DataManager>().playerCall(OwnerClientId, bet);
        betState.Value = BetState.Call;
        dbUpdateCash();
    }

    public void turn(bool turn) {
        isTurn.Value = turn;
    }

    public void winner(ulong wcash)
    {
        LoginManager.instance.updateHandsWon(netId.Value, 1);
        LoginManager.instance.updateBiggestWin(netId.Value, (int)wcash);
        LoginManager.instance.updateXp(netId.Value, (int)currentLobby.Value.GetComponent<DataManager>().smallBlind/10);

        betGoText.Value = "<sprite=0>+$"+ wcash;
        cash.Value += wcash;
        betState.Value = BetState.Win;
        dbUpdateCash();
    }

    public void gameInvite(string lobbyName, string username)
    {
        GameInvite.GetComponent<GameInvite>().setValues(username, lobbyName);
        GameInvite.transform.position = GameObject.Find("GameInvitePlaceholder").transform.position;
        GameInvite.SetActive(true);
    }

    public void acceptInvite(string lobbyName)
    {

        //sitUp();
        ButtonManager.instance.showSeats();
            //Position.Value = GameObject.Find("PlayerPlaceHolder").transform.position;
            //currentSeat.Value = 0;
            //pos.Value = "PlayerPlaceHolder";
        StartCoroutine(poo(lobbyName));
    }

    [ServerRpc(RequireOwnership = false)]
    public void leaveLobbyServerRpc()
    {
        //resetState();

        //currentLobby.Value.GetComponent<DataManager>().addPlayerPL(OwnerClientId);
        leaveLobbyClientRpc();
    }

    [ClientRpc]
    public void leaveLobbyClientRpc()
    {
        this.transform.SetParent(GameObject.Find("Lobbies").transform);
        this.transform.SetParent(currentLobby.Value.transform);
    }

    public IEnumerator poo(string lobbyName)
    {
        ButtonManager bm = GameObject.Find("ButtonManager").GetComponent<ButtonManager>();
        bm.setLoadingScreen();
        //changeLobbyServerRpc(lobbyName, OwnerClientId);
        this.transform.SetParent(GameObject.Find("Lobbies").transform);
                        currentLobby.Value = GameObject.Find("Lobbies");



        Lobbies lobbyManager = GameObject.Find("Lobbies").GetComponent<Lobbies>();

        foreach(GameObject room in lobbyManager.lobbies)
        {
            if(!room.active)
            {
                room.SetActive(true);
            }
            if(room.name == lobbyName)
            {
                currentLobby.Value = room;
                Debug.Log("FOund room: " + room.name);
            }
        }


        yield return new WaitForSeconds(3);
        bm.setLoadingScreen();


        foreach(GameObject room in lobbyManager.lobbies)
        {
            if(room.name != lobbyName)
            {
                room.SetActive(false);
            }
        }

        leaveLobbyServerRpc();
    }

    void dbUpdateCash()
    {
        StartCoroutine(LoginManager.instance.UpdateCashClient(netId.Value, (int)cash.Value));
    }

    public void showProfile()
    {
        if(IsOwner)
        {
            return;
        }
        LoginManager.instance.showProfileClient(netId.Value, GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().netId.Value, currentLobby.Value.name);
    }

    public void showProfileOwner()
    {
        LoginManager.instance.showProfileClient(netId.Value, GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().netId.Value, "Menu");

    }

}
