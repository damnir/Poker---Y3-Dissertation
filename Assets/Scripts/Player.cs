using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkVariable;
//using MLAPI.NetworkedVar;



public class Player : NetworkBehaviour
{
    public int clientID;
    string currentSeat;

    DataManager dataManager;
    Lobbies lobbies;
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

    public string Channel = "MLAPI_DEFAULT_MESSAGE";

    void Start()
    {
        //changeLobby(GameObject.Find("Lobby01"));
        this.name = "PL" + OwnerClientId;

        if(IsOwner){

            Text t1 = GameObject.Find("CLIENT_TEXT").GetComponent<Text>();
            Text t2 = GameObject.Find("CLIENT_TEXT2").GetComponent<Text>();

            t1.text = "Client ID: " + OwnerClientId;
            t2.text = "Client ID: " + OwnerClientId;
        }

        GameObject.Find("Lobbies").GetComponent<Lobbies>().addPlayer(this.gameObject);
        
        transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        changeText("Name", ("Client ID: " + OwnerClientId.ToString()) );


        //Just for testing purposes 
        if(IsOwner){
            randomCard("Card1");
            randomCard("Card2");
        }
    }


    public ulong getPlayerID () { return OwnerClientId; }

    // Update is called once per frame
    void Update()
    {
        //this.transform.position = Position.Value;
        //InvokeServerRpc(syncPositionServer, Channel)
        
        try {
            this.transform.position = GameObject.Find(pos.Value).transform.position; 
        }catch(NullReferenceException e) { }
    }

    void changeText(string objectName, string newText)
    {
        Transform objectTransform = gameObject.transform.Find(objectName);
        GameObject objectReference = objectTransform.gameObject;

        Text objectText = objectReference.GetComponent<Text>();
        objectText.text = newText;
    }

    // TODO: completely refactor this function - doesn't need to be in this script
    void randomCard(string objectName) 
    {
        dataManager = GameObject.Find("Lobby01").GetComponent<DataManager>();

        Transform objectTransform = gameObject.transform.Find(objectName);
        GameObject objectReference = objectTransform.gameObject;

        objectTransform.localScale = new Vector3(1.4f, 1.4f, 1.0f);

        Image objectImage = objectReference.GetComponent<Image>();
        int r = UnityEngine.Random.Range(0, 51);
        string randomCardc = dataManager.deck[r];
        objectImage.sprite = Resources.Load<Sprite>("Cards/" + randomCardc);
        Debug.Log("Random Card: " + randomCardc);
    }

    public void changeLobby(GameObject lobby) 
    {
        currentLobby.Value = lobby;

        if (IsClient)
            {
                // If we are a client. (A client can be either a normal client or a HOST), we want to send a ServerRPC. ServerRPCs does work for host to make code consistent.
                //InvokeServerRpc(changeLobbyServer, lobby, Channel);
                changeLobbyServerRpc(lobby.name);
            }
        else if (IsServer)
            {
                // This is a strict server with no client attached. We can thus send the ClientRPC straight away without the server inbetween.
                //InvokeClientRpcOnEveryone(changeLobbyClient, lobby, Channel);
                //changeLobbyClientRpc(lobby);
            }
    }

    [ServerRpc]
    public void changeLobbyServerRpc(string name)
    {
        //GameObject lobby = GameObject.Find(name);
        dataManager = currentLobby.Value.GetComponent<DataManager>();

        dataManager.addPlayer(this.gameObject);
        string seatName = dataManager.nextFreeSeat();
        //currentSeat = seatName;

        sync(seatName, name);
        // Tell every client
        //InvokeClientRpcOnEveryone(changeLobbyClient, lobby, seatName, Channel);
        changeLobbyClientRpc(name, seatName);
    }

    [ClientRpc]
    public void changeLobbyClientRpc(string name, string seatName) { 
        //GameObject lobby = GameObject.Find(name);
        dataManager = currentLobby.Value.GetComponent<DataManager>();
        dataManager.addPlayer(this.gameObject);
        sync(seatName, name); }

    private void sync(string seatName, string name)
    {
        //GameObject lobby = GameObject.Find(name);
        GameObject lobby = currentLobby.Value;
        GameObject seat = GameObject.Find(seatName);
        this.transform.SetParent(lobby.transform);
        //this.transform.position = seat.transform.position;
        this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }

    //---------------------------------------------------------------
    //Refactored

    public void sit(string name) {
        if(IsClient) {
            GameObject seat = GameObject.Find(name);
            this.transform.position = seat.transform.position;
            this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            //InvokeServerRpc(sitServer, seat, Channel);
            sitServerRpc(name);
        }
    }

    [ServerRpc]
    public void sitServerRpc(string name) {
        GameObject seat = GameObject.Find(name);
        //InvokeClientRpcOnEveryone(sitClient, seat, Channel);
        sitClientRpc(name);
    }

    [ClientRpc]
    public void sitClientRpc(string name) {
        GameObject seat = GameObject.Find(name);
        this.transform.position = seat.transform.position;
        this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        Position.Value = seat.transform.position;
        pos.Value = name;
    }
    
}
