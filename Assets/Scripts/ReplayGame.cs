using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using MLAPI;
using TMPro;
using static MLAPI.Spawning.NetworkSpawnManager;

public class ReplayGame : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject playerObejcts;
    public GameObject playerGo;
    public DataManager.Game game;
    public List<GameObject> players = new List<GameObject>();
    public GameObject ownPlayer;
    public TMP_Text rankText;
    public GameObject player;
    public int stage = 0;

    [Header("Scene Objects")]
    public GameObject[] river = new GameObject[5];
    public GameObject[] seats = new GameObject[7];
    public GameObject pot;

    [Header("Buttons")]
    public GameObject startButton;
    public GameObject nextButton;
    public GameObject finishButton;


    public void onStartClicked()
    {
        string ownNetId = GetLocalPlayerObject().GetComponent<Player>().netId.Value;
        //string ownNetId = "poo";
        //initialise players
        foreach(string value in game.players)
        {
            var player = JsonConvert.DeserializeObject<Dictionary<string, string>>(value);

            GameObject newPlayer = Instantiate(playerGo, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
           
            newPlayer.GetComponent<PlayerRe>().setData(player["netId"], player["seat"], player["card1"], player["card2"]);

            newPlayer.transform.SetParent(playerObejcts.transform, false);
           
            newPlayer.transform.position = seats[Int32.Parse(player["seat"])].transform.position;

            players.Add(newPlayer);

            if(player["netId"] == ownNetId)
            {
                ownPlayer = newPlayer;
                newPlayer.GetComponent<PlayerRe>().setOwn();
                //newPlayer.
            }
        }
        startButton.SetActive(false);
        nextButton.SetActive(true);

    }

    public void onNextClicked()
    {
        var turn = JsonConvert.DeserializeObject<Dictionary<string, string>>(game.round[stage]);

        string netId = turn["netId"];

        if(player != null)
        {
            player.GetComponent<PlayerRe>().endTurn();
        }

        player = getPlayer(netId);

        if(netId != "server")
        {
            player.GetComponent<PlayerRe>().updateState(turn["action"], Int32.Parse(turn["bet"]));
        }
        else if(netId == "server")
        {
            pot.GetComponent<Text>().text = "$"+turn["bet"];
            
            foreach(GameObject player in players)
            {
                player.GetComponent<PlayerRe>().newRound();
            }

            switch(turn["action"])
            {
                case "flop1":
                    river[0].GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" +game.river[0]);
                    river[1].GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" +game.river[1]);
                    river[2].GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" +game.river[2]);
                    river[0].SetActive(true);
                    river[1].SetActive(true);
                    river[2].SetActive(true);
                    break;
                case "flop2":
                    river[3].GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" +game.river[3]);
                    river[3].SetActive(true);
                    break;
                case "flop3":
                    river[4].GetComponent<Image>().sprite = Resources.Load<Sprite>("Cards/" +game.river[4]);
                    river[4].SetActive(true);
                    break;
                case "win":
                    foreach(string winner in game.winners)
                    {
                        getPlayer(winner).GetComponent<PlayerRe>().updateState("win", game.win);
                    }
                    rankText.text = game.handStength;
                    break;
            }            

        }
        stage++;
        if(stage == game.round.Count)
        {
            nextButton.SetActive(false);
            finishButton.SetActive(true);
        }
    }

    public GameObject getPlayer(string netId)
    {
        foreach(GameObject player in players)
        {
            if(player.GetComponent<PlayerRe>().netId == netId)
            {
                return player;
            }
        }

        return null;
    }

    public void onLeaveClick()
    {
        
        this.gameObject.SetActive(false);
        reset();

    }

    public void reset()
    {
        stage = 0;
        foreach(GameObject card in river)
        {
            card.SetActive(false);
        }

        foreach(GameObject player in players)
        {
            Destroy(player);
        }

        startButton.SetActive(true);
        nextButton.SetActive(false);
        finishButton.SetActive(false);
        players.Clear();
        rankText.text = "";
        pot.GetComponent<Text>().text = "$0";

        

    }



}
