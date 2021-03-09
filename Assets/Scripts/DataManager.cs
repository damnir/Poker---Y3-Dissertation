using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;

public class DataManager : NetworkedBehaviour
{
    public GameObject seat1;
    public GameObject seat2;
    public GameObject seat3;


    private static int maxPlayers;
    //keep gameobject references of all players that join the game
    private static GameObject[] players = new GameObject[maxPlayers];
    private static GameObject[] seats = new GameObject[maxPlayers];
    //reference to script component of player
    private static Player player; 

    private static GameObject[] river = new GameObject[5];
    public List<string> deck;

    public static readonly string[] suits = new string[] { "Heart", "Spade", "Diamond", "Club"};
    public static readonly string[] values = new string[] { "01", "02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "12", "13"};
    //public List<string> deck;

    Seat seat1S;
    Seat seat2S;
    Seat seat3S;

    // Start is called before the first frame update
    void Start()
    {
        seat1S = seat1.GetComponent<Seat>();
        seat2S = seat2.GetComponent<Seat>();
        seat3S = seat3.GetComponent<Seat>();

        Debug.Log("Seat 1 taken? " + seat1S.isTaken());
        Debug.Log("Seat 1 taken? " + seat1S.isTaken());

        generateDeck();
        shuffleDeck();

        foreach (string c in deck)
        {
            Debug.Log(c);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void test(){
        //Debug.Log(seat1.isTaken());
    }

    public string nextFreeSeat(){
        if (!seat1S.isTaken())
        {
            seat1S.setTaken(true);
            return ("Seat");
        }
        if (!seat2S.isTaken())
        {
            seat2S.setTaken(true);
            return ("Seat (1)");
        }
        if (!seat3S.isTaken())
        {
            return ("Seat (2)");
        }
        else
            return("na");

    }

    public void setSeatTaken(bool newtaken, GameObject seat)
    {
        Seat temp = seat.GetComponent<Seat>();
        temp.setTaken(newtaken);

    }

/*
    public string getNameField()
    {
        string text = nameField.GetComponent<InputField>().text;
        //nameField.GetComponent<InputField>().text = "";
        return text;

    }*/

    public void generateDeck()
    {
        foreach (string s in suits)
        {
            foreach (string v in values)
            {
                deck.Add (s + v);
            }
        }
    }

    public void shuffleDeck()
    {
        for(int i = 51; i > 1; i--)
        {
            int j = Random.Range(0, 51);
            string temp1 = deck[i];
            string temp2 = deck[j];
            deck[i] = temp2;
            deck[j] = temp1;
        }
    }

}
