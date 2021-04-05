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
        Call
    }

    enum GameState
    {
        Menu,
        Spec,
        Ingame
    }

    public NetworkVariableInt clientID = new NetworkVariableInt(new NetworkVariableSettings
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

    public GameObject foldGo;
    public GameObject card1;
    public GameObject card2;
    public GameObject betGo;
    public GameObject animation;
    public GameObject ownerGo;
    GameObject text;

    public GameObject buttonManager;



    public string Channel = "MLAPI_DEFAULT_MESSAGE";

    void Start()
    {
        buttonManager = GameObject.Find("ButtonManager");
        text = betGo.transform.Find("Text").gameObject;
        this.name = "PL" + OwnerClientId;

        if(IsOwner){
            clientID.Value = (int)OwnerClientId;

            Text t1 = GameObject.Find("CLIENT_TEXT").GetComponent<Text>();
            Text t2 = GameObject.Find("CLIENT_TEXT2").GetComponent<Text>();

            cash.Value = 100000;

            t1.text = "Client ID: " + OwnerClientId;
            t2.text = "Client ID: " + OwnerClientId;
            ownerGo.SetActive(true);
        }

        GameObject.Find("Lobbies").GetComponent<Lobbies>().addPlayer(this.gameObject);
        
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        changeText("Name", ("Client ID: " + OwnerClientId.ToString()) );
    }

    public int getPlayerID () { return clientID.Value; }

    // Update is called once per frame
    void Update()
    {   
        if(IsClient) {
            try {
                this.transform.position = GameObject.Find(pos.Value).transform.position; 
            }catch(NullReferenceException e) { }
            changeText("Cash", "$" + cash.Value.ToString());

            switch (betState.Value) {
                case BetState.Fold: 
                    foldGo.SetActive(true);
                    betGoText.Value = "<sprite=2>Fold";
                    betGo.SetActive(true);
                    card1.SetActive(false);
                    card2.SetActive(false);
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
                    break;
                case BetState.Raise:
                    betGoText.Value = "<sprite=0>$$$";
                    betGo.SetActive(true);
                    foldGo.SetActive(false);
                    card1.SetActive(true);
                    card2.SetActive(true);
                    break;
                case BetState.Wait:
                    betGo.SetActive(false);
                    foldGo.SetActive(false);
                    card1.SetActive(true);
                    card2.SetActive(true);
                    break;
            }
            
            if(isTurn.Value == true) {
                animation.SetActive(true);
                if (IsOwner)
                {
                    buttonManager.GetComponent<ButtonManager>().updateCall(currentLobby.Value.GetComponent<DataManager>().currentBet.Value);
                }
            }
            else{
                animation.SetActive(false);
            }

            text.GetComponent<TextMeshProUGUI>().text = betGoText.Value;
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

        folded.Value = false;
    }

    public void changeLobby(GameObject lobby) 
    {
        currentLobby.Value = lobby;

        if (IsClient)
        {
            changeLobbyServerRpc(lobby.name);
        }
    }

    [ServerRpc]
    public void changeLobbyServerRpc(string name)
    {
        dataManager = currentLobby.Value.GetComponent<DataManager>();

        dataManager.addPlayer(this.gameObject);

        sync(name);
        changeLobbyClientRpc(name);
    }

    [ClientRpc]
    public void changeLobbyClientRpc(string name) { 
        dataManager = currentLobby.Value.GetComponent<DataManager>();
        sync(name); }

    private void sync( string name)
    {
        GameObject lobby = currentLobby.Value;
        this.transform.SetParent(lobby.transform);
        this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    //---------------------------------------------------------------
    //Refactored

    public void sit(string name) {
        if(IsClient) {
            GameObject seat = GameObject.Find(name);
            this.transform.position = seat.transform.position;
            this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            sitServerRpc(name);
        }
    }

    [ServerRpc]
    public void sitServerRpc(string name) {
        GameObject seat = GameObject.Find(name);
        sitClientRpc(name);
    }

    [ClientRpc]
    public void sitClientRpc(string name) {
        GameObject seat = GameObject.Find(name);
        this.transform.position = seat.transform.position;
        this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        if(IsOwner) {
            Position.Value = seat.transform.position;
            currentSeat.Value = seat.GetComponent<Seat>().seatNo;
        }
        pos.Value = name;
    }

    ////GAME 
    //sprites: 0 = raise, 1 = check, 2 = fold, 3 = call
    public void fold() {
        folded.Value = true;
        currentLobby.Value.GetComponent<DataManager>().playerFoldServerRpc(OwnerClientId);

        betState.Value = BetState.Fold;
    }

    public void check() {
        betState.Value = BetState.Check;
    }

    public void call() {
        currentBet.Value = currentLobby.Value.GetComponent<DataManager>().currentBet.Value;
        cash.Value -= (ulong)currentBet.Value;
        betGoText.Value = "<sprite=3>"+currentBet.Value;
        currentLobby.Value.GetComponent<DataManager>().playerCallServerRpc(OwnerClientId, currentBet.Value);
        betState.Value = BetState.Call;
    }

    public void raise() {
        betGoText.Value = "<sprite=0>$$$";
        betState.Value = BetState.Raise;
    }

    public void resetBetState() {
        if(betState.Value != BetState.Fold)
        {
            betState.Value = BetState.Wait;
        }
    }

    public void turn(bool turn) {
        //currentBet.Value = currentLobby.Value.GetComponent<DataManager>().currentBet.Value;

        isTurn.Value = turn;
    }


    
}
