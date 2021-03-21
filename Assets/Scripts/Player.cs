using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;


public class Player : NetworkedBehaviour
{
    private static string username {get; set;}
    public int clientID;
    private static int cash {get; set;}
    
    public GameObject data {get; set;}
    public GameObject card1 {get; set;}
    public GameObject card2 {get; set;}

    DataManager dataManager;
    Lobbies lobbies;

    public GameObject objectReference;
    public Transform objectTransform;
    public Text objectText;

    public GameObject po;
    //public int poo;

    GameObject lobby01;
    GameObject lobby02;

            public string Channel = "MLAPI_DEFAULT_MESSAGE";



    // Start is called before the first frame update
    void Start()
    {

        //this.gameObject.SetActive(false);

        //clientID = OwnerClientId;
        this.name = "PL" + OwnerClientId;

        if(isOwner){

            Text t1 = GameObject.Find("CLIENT_TEXT").GetComponent<Text>();
            Text t2 = GameObject.Find("CLIENT_TEXT2").GetComponent<Text>();

            t1.text = "Client ID: " + OwnerClientId;
            t2.text = "Client ID: " + OwnerClientId;
        }

        //data = GameObject.Find("Lobby01");
        //dataManager = data.GetComponent<DataManager>();

        
        GameObject kk = GameObject.Find("Lobbies");
        lobbies = kk.GetComponent<Lobbies>();

        dataManager = lobbies.getLobby(0).GetComponent<DataManager>();

        string bb = dataManager.nextFreeSeat();

        GameObject seat = GameObject.Find(bb);

        this.transform.SetParent(GameObject.Find("Lobby01").transform);
        this.transform.position = seat.transform.position;
        //this.transform.localScale = seat.transform.localScale;

        this.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

        GameObject.Find("Lobbies").GetComponent<Lobbies>().addPlayer(this.gameObject);

        changeText("Name", ("Client ID: " + OwnerClientId.ToString()) );
        if(isOwner){
            /////randomCard("Card1");
            //////randomCard("Card2");
            //GameObject buttons = GameObject.Find("Buttons");
            //buttons.SetActive(true);
            //GameObject.Find("Lobbies").GetComponent<Lobbies>().addPlayer(this.gameObject.GetComponent<NetworkedObject>());
        }

         //add player to connected players list in lobbies GO


    }

    public ulong getPlayerID () { return OwnerClientId; }

    // Update is called once per frame
    void Update()
    {
        
    }

    void changeText(string objectName, string newText)
    {
        objectTransform = gameObject.transform.Find(objectName);
        objectReference = objectTransform.gameObject;

        objectText = objectReference.GetComponent<Text>();
        objectText.text = newText;
    }

    void randomCard(string objectName)
    {
        objectTransform = gameObject.transform.Find(objectName);
        objectReference = objectTransform.gameObject;

        objectTransform.localScale = new Vector3(1.4f, 1.4f, 1.0f);

        Image objectImage = objectReference.GetComponent<Image>();
        int r = Random.Range(0, 51);
        string randomCardc = dataManager.deck[r];
        objectImage.sprite = Resources.Load<Sprite>("Cards/" + randomCardc);
        Debug.Log("Random Card: " + randomCardc);
    }

    public void poo(GameObject loo) {
        if (IsClient)
            {
                // If we are a client. (A client can be either a normal client or a HOST), we want to send a ServerRPC. ServerRPCs does work for host to make code consistent.
                InvokeServerRpc(SendPositionToServer, loo, Channel);
            }
        else if (IsServer)
            {
                // This is a strict server with no client attached. We can thus send the ClientRPC straight away without the server inbetween.
                InvokeClientRpcOnEveryone(changeLobby, loo, Channel);
            }
    }

    [ServerRPC(RequireOwnership = false)]
    public void SendPositionToServer(GameObject position)
    {
        // This code gets ran on the server at the request of clients or the host
        this.gameObject.GetComponent<NetworkedObject>().NetworkShow(NetworkingManager.Singleton.LocalClientId);
        //this.gameObject.GetComponent<NetworkedObject>().NetworkShow(clientId);

        // Tell every client EXCEPT the owner (since they are the ones that actually send the position) to apply the new position
        InvokeClientRpcOnEveryone(changeLobby, position, Channel);
    }

    //public void changeLobby(transform newLobby)
    [ClientRPC]
    public void changeLobby(GameObject position)
    {
        // This code gets ran on the clients at the request of the server.
        this.gameObject.SetActive(true);
        this.transform.SetParent(position.transform);

        position.GetComponent<DataManager>().addPlayer(this.gameObject);
    }
    
}
