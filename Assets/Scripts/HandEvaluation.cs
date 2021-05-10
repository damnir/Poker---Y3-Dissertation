using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class HandEvaluation : MonoBehaviour
{
    public struct Hand
    {
        public int rank; //1# royal flush, 2# straight flush, 3# four of a kind, 4# full house, #5 flush, #6 straight, #7 three of a kind, #8 two pair, #9 pair, #10 high card
        public int strength; //2-14 (2-A)
        public ulong pId;
        public string text;
    }

    public List<Hand> playerHands = new List<Hand>();

    public struct Rank
    {
        public List<int> pairs;
        public List<int> threeof;
        public int singlePairInt;
        public int pairsV;
        public int threeofV;
        public int fourof;
        public int straight;
        public int flush;
        public int straightflush;
        public int fullhouse;
        public int highCard;
    }

    public Rank newRank()
    {
        Rank rank = new Rank();
        rank.pairs = new List<int>();
        rank.threeof = new List<int>();
        rank.fourof = 0;
        return rank;
    }

    public Rank evaluateHand(List<string> cards) {

        int len = cards.Count; //no. of cards
        int[] v = new int[len]; //values
        string[] s = new string[len]; //suits

        for(int i = 0; i < len; i++)
        {
            string[] split = cards[i].Split('-');
            s[i] = split[0];
            int val = Int32.Parse(split[1]);
            if(val == 1)
            {
                v[i] = 14;
            }
            else
            {
                v[i] = val;
            }

        }

        int highCard = v.Max();

        string[] sOg = (string[])s.Clone(); //for straight and royal flush
        Array.Sort(sOg);
        Array.Sort(v, s);

        Rank rank = newRank();
        rank.highCard = highCard;

        int count = 0; int countS = 0; int countF = 0;
        //pairs         suits           flush
        if(v[0] == 2 && highCard == 14)
        {
            countS++; //for straight check...
        }

        for(int i = 1; i < len; i++)
        {
            //------ checking for duplicate values
            if(v[i] == v[i-1]){
                count++;
            }
            if(v[i] != v[i-1] || i == len-1)
            {
                if(count != 0)
                {
                    switch(count)
                    {
                        case 1: //one pair
                            rank.pairs.Add(v[i-1]);
                            break;
                        case 2: //three of a kind
                            rank.threeof.Add(v[i-1]);
                            break;
                        case 3: //four of a kind
                            rank.fourof = (v[i-1]);
                            break;
                    }
                    count = 0;
                }
            }
            //------- checking for flush
            if(sOg[i] == sOg[i-1]) //FLUSH FLAW
            {
                countF++;
                if(countF >= 4)
                {
                    Debug.Log("FLUSH!");
                    rank.flush = countF;
                }
            }
            else
                countF = 0;
            
            //------- checking for straight 
            if( (v[i-1]+1) == v[i] )
            {
                countS++;
                if(countS >= 4)
                {
                    Debug.Log("STRAIGHT!");
                    rank.straight = v[i]*5-10;;
                    //check for royalflush
                    if(countF >= 4)
                    {
                        string[] royal = new string[5];
                        Array.Copy(s, i-5, royal, 0, 5);

                        if(royal.All(ss => string.Equals(s[i-1], ss, StringComparison.InvariantCultureIgnoreCase)))
                        {
                            Debug.Log("STRAIGHT FLUSH!!");
                            //add all values together for strength
                            rank.straightflush = v[i]*5-10;
                            //45678
                        }

                    }
                }
            }
            else{
                if(count == 0)
                {
                    countS = 0;
                }
            }
        }

        //check for full house
        if(rank.pairs.Count >= 1 && rank.threeof.Count >= 1)
        {
            int value = 0;
            if(rank.pairs.Count > 1) //if multiple pairs, pick the highest value
            {
                rank.pairs.Sort((a, b) => b.CompareTo(a));
                value += rank.pairs[0];
            }
            if(rank.threeof.Count > 1)//if multiple pairs, pick the highest value
            {
                rank.threeof.Sort((a, b) => b.CompareTo(a));
                value += rank.threeof[0];
            }
            if(rank.pairs.Count == 1 && rank.threeof.Count == 1)
            {
                value += rank.pairs[0] + rank.threeof[0];
            }
            rank.fullhouse = value;
            
            Debug.Log("FULL HOUSE!");
        }

        if(rank.pairs.Count > 1) //if multiple pairs, pick the highest value
        {
            rank.pairs.Sort((a, b) => b.CompareTo(a));
        }
        else if(rank.pairs.Count == 1)
        {
            rank.singlePairInt = rank.pairs[0];
        }

        if(rank.threeof.Count > 1)//if multiple pairs, pick the highest value
        {
            rank.threeof.Sort((a, b) => b.CompareTo(a));
        }
        else if(rank.threeof.Count == 1)
        {
            rank.threeofV = rank.threeof[0];
        }

        //check for 2 pairs
        if(rank.pairs.Count > 1)
        {
            int value2 = 0;
            if(rank.pairs.Count > 2)
            {
                rank.pairs.Sort((a, b) => b.CompareTo(a));
            }
            value2 = rank.pairs[0] + rank.pairs[1];
            rank.pairsV = value2;
            Debug.Log("TWO PAIRS!");
        }
        else if(rank.pairs.Count == 1)
        {
            rank.singlePairInt = rank.pairs[0];
        }
        //check if new best hand DB

        return rank;
    }
}
