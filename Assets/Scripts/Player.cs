using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

public class Player : NetworkedBehaviour
{
    private static string username {get; set;}
    private static int clientID {get; set;}
    private static int cash {get; set;}
    
    public GameObject data {get; set;}
    public GameObject card1 {get; set;}
    public GameObject card2 {get; set;}

    DataManager dataManager;
    Lobbies lobbies;

    public GameObject objectReference;
    public Transform objectTransform;
    public Text objectText;

    // Start is called before the first frame update
    void Start()
    {
        //data = GameObject.Find("Lobby01");
        //dataManager = data.GetComponent<DataManager>();


        GameObject kk = GameObject.Find("Lobbies");
        lobbies = kk.GetComponent<Lobbies>();

        dataManager = lobbies.getLobby(0).GetComponent<DataManager>();

        string bb = dataManager.nextFreeSeat();

        GameObject seat = GameObject.Find(bb);

        this.transform.SetParent(GameObject.Find("Canvas").transform);
        this.transform.position = seat.transform.position;
        this.transform.localScale = seat.transform.localScale;

        changeText("Name", ("Client ID: " + OwnerClientId.ToString()) );
        if(isOwner){
            randomCard("Card1");
            randomCard("Card2");
            //GameObject buttons = GameObject.Find("Buttons");
            //buttons.SetActive(true);
        }

    }

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


    
}
