using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DataManager : MonoBehaviour
{
  
    public class Game
    {
        public class Player
        {
            public string netId;
            public string card1;
            public string card2;
            public int seat; 

            public Player(string _netId, string _card1, string _card2, int _seat)
            {
                netId = _netId;
                card1 = _card1;
                card2 = _card2;
                seat = _seat;
            }
        }

        public class Turn
        {
            public string netId;
            public string action;
            public int bet;

            public Turn(string _netId, string _action, int _bet)
            {
                netId = _netId;
                action = _action;
                bet = _bet;
            }
        }

        public List<string> players = new List<string>();
        public List<string> river = new List<string>();
        public List<string> round = new List<string>();
        public List<string> winners = new List<string>();
        public string handStength;
        public int win;
        public string time = DateTime.Now.ToString();

        public Game() 
        {
        }

        public static Game CreateFromJSON(string json)
        {
            return JsonUtility.FromJson<Game>(json);
        }

        public void addTurn(string _id, string action, int bet)
        {
            Turn turn = new Turn(_id, action, bet);
            string json = JsonUtility.ToJson(turn);

            round.Add(json);
        }

        public void addPlayer(string _id, string _card1, string _card2, int _seat)
        {
            Player player = new Player(_id, _card1, _card2, _seat);
            string json = JsonUtility.ToJson(player);

            players.Add(json);
        }

        public void addRiverCard(string _card)
        {
            river.Add(_card);
        }

        public void addWinner(string winner)
        {
            winners.Add(winner);
        }

        public void setWin(int _win, string hand)
        {
            win = _win;
            handStength = hand;
        }

        public void clearGame()
        {
            players.Clear();
            round.Clear();
            river.Clear();
            winners.Clear();
            win = 0;
            handStength = "";
            time = DateTime.Now.ToString();
        }
    }

}
