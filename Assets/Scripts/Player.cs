using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;


public class Player : NetworkedBehaviour
{
    public int clientID;
    string currentSeat;

    DataManager dataManager;
    Lobbies lobbies;

    public string Channel = "MLAPI_DEFAULT_MESSAGE";

    void Start()
    {

    }

    public override void NetworkStart() {
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
        //InvokeServerRpc(syncPositionServer, Channel)
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
        int r = Random.Range(0, 51);
        string randomCardc = dataManager.deck[r];
        objectImage.sprite = Resources.Load<Sprite>("Cards/" + randomCardc);
        Debug.Log("Random Card: " + randomCardc);
    }

    public void changeLobby(GameObject lobby) 
    {
        if (IsClient)
            {
                // If we are a client. (A client can be either a normal client or a HOST), we want to send a ServerRPC. ServerRPCs does work for host to make code consistent.
                InvokeServerRpc(changeLobbyServer, lobby, Channel);
            }
        else if (IsServer)
            {
                // This is a strict server with no client attached. We can thus send the ClientRPC straight away without the server inbetween.
                InvokeClientRpcOnEveryone(changeLobbyClient, lobby, Channel);
            }
    }

    [ServerRPC]
    public void syncPositionServer() {
        this.transform.position = this.transform.parent.position;
        InvokeClientRpcOnEveryone(syncPositionClient, Channel);
    }

    [ClientRPC]
    public void syncPositionClient() {
        this.transform.position = this.transform.parent.position;
    }


    [ServerRPC(RequireOwnership = true)]
    public void changeLobbyServer(GameObject lobby)
    {
        dataManager = lobby.GetComponent<DataManager>();

        dataManager.addPlayer(this.gameObject);
        string seatName = dataManager.nextFreeSeat();
        //currentSeat = seatName;

        sync(seatName, lobby);
        // Tell every client
        InvokeClientRpcOnEveryone(changeLobbyClient, lobby, seatName, Channel);
    }

    [ClientRPC]
    public void changeLobbyClient(GameObject lobby, string seatName) { sync(seatName, lobby); }

    private void sync(string seatName, GameObject lobby)
    {
        GameObject seat = GameObject.Find(seatName);
        this.transform.SetParent(lobby.transform);
        this.transform.position = seat.transform.position;
        this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
    }
    
}
