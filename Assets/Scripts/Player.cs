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
    DataManager dataManager;
    Lobbies lobbies;

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

    public string Channel = "MLAPI_DEFAULT_MESSAGE";

    void Start()
    {
        this.name = "PL" + OwnerClientId;

        if(IsOwner){
            clientID.Value = (int)OwnerClientId;

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
            //randomCard("Card1");
            //randomCard("Card2");
        }
    }

    public int getPlayerID () { return clientID.Value; }

    // Update is called once per frame
    void Update()
    {   
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

    public void dealCards(string card1, string card2) {
        Transform card1transform = gameObject.transform.Find("Card1");
        GameObject card1go = card1transform.gameObject;
        card1transform.localScale = new Vector3(1.4f, 1.4f, 1.0f);
        Image card1image = card1go.GetComponent<Image>();
        card1image.sprite = Resources.Load<Sprite>("Cards/" + card1);

        Transform card2transform = gameObject.transform.Find("Card2");
        GameObject card2go = card2transform.gameObject;
        card2transform.localScale = new Vector3(1.4f, 1.4f, 1.0f);
        Image card2image = card2go.GetComponent<Image>();
        card2image.sprite = Resources.Load<Sprite>("Cards/" + card2);
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
    
}
