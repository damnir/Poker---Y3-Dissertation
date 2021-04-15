using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
using TMPro;
//using MLAPI.NetworkedVar;

public class Player : NetworkBehaviour
{
    DataManager dataManager;
    Lobbies lobbies;

    public enum BetState 
    {
        Wait,
        Fold,
        Check,
        Raise,
        Call,
        Win
    }

    enum GameState
    {
        Menu,
        Spec,
        Ingame
    }

    public NetworkVariable<ulong> clientID = new NetworkVariable<ulong>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.OwnerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariableVector3 Position = new NetworkVariableVector3(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.OwnerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariableString pos = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.OwnerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariable<GameObject> currentLobby = new NetworkVariable<GameObject>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.OwnerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariableInt currentSeat = new NetworkVariableInt(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.OwnerOnly,
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariable<ulong> cash = new NetworkVariable<ulong>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER ONLY LATER
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariable<ulong> currentBet = new NetworkVariable<ulong>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariable<bool> folded = new NetworkVariable<bool>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariable<bool> isTurn = new NetworkVariable<bool>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariableString betGoText = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariable<BetState> betState = new NetworkVariable<BetState>(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariableString card1v = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public NetworkVariableString card2v = new NetworkVariableString(new NetworkVariableSettings
        {
            WritePermission = NetworkVariablePermission.Everyone, //CHANGE THIS PERMISSION TO SERVER/OWNER ONLY LATER
            ReadPermission = NetworkVariablePermission.Everyone
        });

    public ulong lobbyBet;

    

    public GameObject foldGo;
    public GameObject card1;
    public GameObject card2;
    public GameObject betGo;
    public GameObject animation;
    public GameObject ownerGo;
    public GameObject win;
    GameObject text;

    public GameObject buttonManager;



    public string Channel = "MLAPI_DEFAULT_MESSAGE";

    void Start()
    {
        buttonManager = GameObject.Find("ButtonManager");
        text = betGo.transform.Find("Text").gameObject;
        this.name = "PL" + OwnerClientId;

        if(IsOwner){
            clientID.Value = OwnerClientId;

            Text t1 = GameObject.Find("CLIENT_TEXT").GetComponent<Text>();
            Text t2 = GameObject.Find("CLIENT_TEXT2").GetComponent<Text>();

            cash.Value = 100000;

            t1.text = "Client ID: " + OwnerClientId;
            t2.text = "Client ID: " + OwnerClientId;
            ownerGo.SetActive(true);
            betState.Value = BetState.Fold;
        }

        GameObject.Find("Lobbies").GetComponent<Lobbies>().addPlayer(this.gameObject);
        
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        changeText("Name", ("Client ID: " + OwnerClientId.ToString()) );
    }

    public ulong getPlayerID () { return clientID.Value; }

    // Update is called once per frame
    void Update()
    {   
        try {
            this.transform.position = GameObject.Find(pos.Value).transform.position; 
            this.transform.SetParent(currentLobby.Value.transform);
            this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            //this.transform = GameObject.Find(pos.Value).transform;
            lobbyBet = currentLobby.Value.GetComponent<DataManager>().currentBet.Value;
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
                 
                    buttonManager.GetComponent<ButtonManager>().updateCall(lobbyBet - currentBet.Value);
                    if(currentLobby.Value.GetComponent<DataManager>().previousBet.Value == 0)
                    {
                        buttonManager.GetComponent<ButtonManager>().updateRaise(currentLobby.Value.GetComponent<DataManager>().bigBlind, (lobbyBet - currentBet.Value) + currentLobby.Value.GetComponent<DataManager>().mainPot.Value);
                    }
                    else
                    {
                        buttonManager.GetComponent<ButtonManager>().updateRaise(currentLobby.Value.GetComponent<DataManager>().previousBet.Value, (lobbyBet - currentBet.Value) + currentLobby.Value.GetComponent<DataManager>().mainPot.Value);

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
    }

    void changeText(string objectName, string newText)
    {
        Transform objectTransform = gameObject.transform.Find(objectName);
        GameObject objectReference = objectTransform.gameObject;

        Text objectText = objectReference.GetComponent<Text>();
        objectText.text = newText;
    }

    public void dealCards(string c1, string c2) {

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
        lobby.GetComponent<DataManager>().addPlayerServerRpc(OwnerClientId);
        currentLobby.Value = lobby;
        this.transform.SetParent(lobby.transform);
        this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    //---------------------------------------------------------------
    //Refactored

    public void sit(string name) {
        if(IsOwner) {
            GameObject seat = GameObject.Find(name);
            Position.Value = seat.transform.position;
            currentSeat.Value = seat.GetComponent<Seat>().seatNo;
            pos.Value = name;
        }
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
    }

    public void raise(ulong bet) {
        isTurn.Value = false;

        currentLobby.Value.GetComponent<DataManager>().playerRaiseServerRpc(OwnerClientId, lobbyBet + bet - currentBet.Value, bet);
        cash.Value -= lobbyBet + bet - currentBet.Value;
        currentBet.Value = lobbyBet + bet;

        betGoText.Value = "<sprite=0>$"+ (lobbyBet + bet);
        betState.Value = BetState.Raise;
    }

    public void resetBetState() {
        currentBet.Value = 0;

        if(betState.Value != BetState.Fold)
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
    }

    public void turn(bool turn) {
        isTurn.Value = turn;
    }

    public void winner(ulong wcash)
    {
        betGoText.Value = "<sprite=0>+$"+ wcash;
        cash.Value += wcash;
        betState.Value = BetState.Win;
    }


}
