using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class WinnerTest{

        //**Function taken from DataManager - can't be tested from the class itself as it has network behaviour
        public ulong determineWinner(List<HandEvaluation.Hand> playerHands)
        {
                playerHands.Sort((x,y) => x.rank.CompareTo(y.rank));
                List<HandEvaluation.Hand> winners = new List<HandEvaluation.Hand>();

                winners.Add(playerHands[0]);

                for (int i = 1; i < playerHands.Count; i++)
                {
                //Debug.Log("ID: " + playerHands[i].pId + " Rank: " + playerHands[i].rank + " Strength: " + playerHands[i].strength);
                if(playerHands[i].strength == playerHands[i-1].strength && playerHands[i].rank == playerHands[i-1].rank)
                {
                        winners.Add(playerHands[i]);
                }
                else if(playerHands[i].strength > playerHands[i-1].strength && playerHands[i].rank == playerHands[i-1].rank)
                {
                        winners.Clear();
                        winners.Add(playerHands[i]);
                }
                else if(playerHands[i].rank != playerHands[i-1].rank)
                {
                        break;
                }
                }

                foreach (HandEvaluation.Hand hand in winners)
                {
                        Debug.Log("WINNER- ID: " + hand.pId + " Rank: " + hand.rank + " Strength: " + hand.strength);
                }

                return winners[0].pId;
        }

        //ranks - 1# royal flush, 2# straight flush, 3# four of a kind, 4# full house, #5 flush, #6 straight, #7 three of a kind, #8 two pair, #9 pair, #10 high card
        [Test]
        public void WinnerTest1()
        {
                List<HandEvaluation.Hand> hands = new List<HandEvaluation.Hand>();

                HandEvaluation.Hand hand1 = new HandEvaluation.Hand();
                hand1.rank = 3;
                hand1.strength = 11;
                hand1.pId = 1;
                hands.Add(hand1);

                HandEvaluation.Hand hand2 = new HandEvaluation.Hand();
                hand2.rank = 4;
                hand2.strength = 20;
                hand2.pId = 2;
                hands.Add(hand2);

                HandEvaluation.Hand hand3 = new HandEvaluation.Hand();
                hand3.rank = 3;
                hand3.strength = 12;
                hand3.pId = 3;
                hands.Add(hand3);

                Assert.AreEqual(3, determineWinner(hands));
        }
                [Test]
        public void WinnerTest2()
        {
                List<HandEvaluation.Hand> hands = new List<HandEvaluation.Hand>();

                HandEvaluation.Hand hand1 = new HandEvaluation.Hand();
                hand1.rank = 8;
                hand1.strength = 20;
                hand1.pId = 1;
                hands.Add(hand1);

                HandEvaluation.Hand hand2 = new HandEvaluation.Hand();
                hand2.rank = 8;
                hand2.strength = 19;
                hand2.pId = 2;
                hands.Add(hand2);

                HandEvaluation.Hand hand3 = new HandEvaluation.Hand();
                hand3.rank = 7;
                hand3.strength = 2;
                hand3.pId = 3;
                hands.Add(hand3);

                Assert.AreEqual(3, determineWinner(hands));
        }
                [Test]
        public void WinnerTest3()
        {
                List<HandEvaluation.Hand> hands = new List<HandEvaluation.Hand>();

                HandEvaluation.Hand hand1 = new HandEvaluation.Hand();
                hand1.rank = 10;
                hand1.strength = 9;
                hand1.pId = 1;
                hands.Add(hand1);

                HandEvaluation.Hand hand2 = new HandEvaluation.Hand();
                hand2.rank = 10;
                hand2.strength = 9;
                hand2.pId = 2;
                hands.Add(hand2);

                HandEvaluation.Hand hand3 = new HandEvaluation.Hand();
                hand3.rank = 10;
                hand3.strength = 11;
                hand3.pId = 3;
                hands.Add(hand3);

                Assert.AreEqual(3, determineWinner(hands));
        }
                [Test]
        public void WinnerTest4()
        {
                List<HandEvaluation.Hand> hands = new List<HandEvaluation.Hand>();

                HandEvaluation.Hand hand1 = new HandEvaluation.Hand();
                hand1.rank = 4;
                hand1.strength = 10;
                hand1.pId = 1;
                hands.Add(hand1);

                HandEvaluation.Hand hand2 = new HandEvaluation.Hand();
                hand2.rank = 4;
                hand2.strength = 20;
                hand2.pId = 2;
                hands.Add(hand2);

                HandEvaluation.Hand hand3 = new HandEvaluation.Hand();
                hand3.rank = 4;
                hand3.strength = 15;
                hand3.pId = 3;
                hands.Add(hand3);

                Assert.AreEqual(2, determineWinner(hands));
        }
                [Test]
        public void WinnerTest5()
        {
                List<HandEvaluation.Hand> hands = new List<HandEvaluation.Hand>();

                HandEvaluation.Hand hand1 = new HandEvaluation.Hand();
                hand1.rank = 1;
                hand1.strength = 20;
                hand1.pId = 1;
                hands.Add(hand1);

                HandEvaluation.Hand hand2 = new HandEvaluation.Hand();
                hand2.rank = 1;
                hand2.strength = 20;
                hand2.pId = 2;
                hands.Add(hand2);

                HandEvaluation.Hand hand3 = new HandEvaluation.Hand();
                hand3.rank = 1;
                hand3.strength = 20;
                hand3.pId = 3;
                hands.Add(hand3);

                Assert.AreEqual(1, determineWinner(hands));
        }
                [Test]
        public void WinnerTest6()
        {
                List<HandEvaluation.Hand> hands = new List<HandEvaluation.Hand>();

                HandEvaluation.Hand hand1 = new HandEvaluation.Hand();
                hand1.rank = 4;
                hand1.strength = 30;
                hand1.pId = 1;
                hands.Add(hand1);

                HandEvaluation.Hand hand2 = new HandEvaluation.Hand();
                hand2.rank = 5;
                hand2.strength = 15;
                hand2.pId = 2;
                hands.Add(hand2);

                HandEvaluation.Hand hand3 = new HandEvaluation.Hand();
                hand3.rank = 5;
                hand3.strength = 12;
                hand3.pId = 3;
                hands.Add(hand3);

                Assert.AreEqual(1, determineWinner(hands));
        }
                [Test]
        public void WinnerTest7()
        {
                List<HandEvaluation.Hand> hands = new List<HandEvaluation.Hand>();

                HandEvaluation.Hand hand1 = new HandEvaluation.Hand();
                hand1.rank = 2;
                hand1.strength = 11;
                hand1.pId = 1;
                hands.Add(hand1);

                HandEvaluation.Hand hand2 = new HandEvaluation.Hand();
                hand2.rank = 3;
                hand2.strength = 20;
                hand2.pId = 2;
                hands.Add(hand2);

                HandEvaluation.Hand hand3 = new HandEvaluation.Hand();
                hand3.rank = 3;
                hand3.strength = 12;
                hand3.pId = 3;
                hands.Add(hand3);

                Assert.AreEqual(1, determineWinner(hands));
        }
                [Test]
        public void WinnerTest8()
        {
                List<HandEvaluation.Hand> hands = new List<HandEvaluation.Hand>();

                HandEvaluation.Hand hand1 = new HandEvaluation.Hand();
                hand1.rank = 11;
                hand1.strength = 5;
                hand1.pId = 1;
                hands.Add(hand1);

                HandEvaluation.Hand hand2 = new HandEvaluation.Hand();
                hand2.rank = 11;
                hand2.strength = 30;
                hand2.pId = 2;
                hands.Add(hand2);

                HandEvaluation.Hand hand3 = new HandEvaluation.Hand();
                hand3.rank = 5;
                hand3.strength = 25;
                hand3.pId = 3;
                hands.Add(hand3);

                Assert.AreEqual(3, determineWinner(hands));
        }
                [Test]
        public void WinnerTest9()
        {
                List<HandEvaluation.Hand> hands = new List<HandEvaluation.Hand>();

                HandEvaluation.Hand hand1 = new HandEvaluation.Hand();
                hand1.rank = 8;
                hand1.strength = 30;
                hand1.pId = 1;
                hands.Add(hand1);

                HandEvaluation.Hand hand2 = new HandEvaluation.Hand();
                hand2.rank = 8;
                hand2.strength = 28;
                hand2.pId = 2;
                hands.Add(hand2);

                HandEvaluation.Hand hand3 = new HandEvaluation.Hand();
                hand3.rank = 3;
                hand3.strength = 15;
                hand3.pId = 3;
                hands.Add(hand3);

                Assert.AreEqual(3, determineWinner(hands));
        }
                [Test]
        public void WinnerTest10()
        {
                List<HandEvaluation.Hand> hands = new List<HandEvaluation.Hand>();

                HandEvaluation.Hand hand1 = new HandEvaluation.Hand();
                hand1.rank = 4;
                hand1.strength = 24;
                hand1.pId = 1;
                hands.Add(hand1);

                HandEvaluation.Hand hand2 = new HandEvaluation.Hand();
                hand2.rank = 4;
                hand2.strength = 10;
                hand2.pId = 2;
                hands.Add(hand2);

                HandEvaluation.Hand hand3 = new HandEvaluation.Hand();
                hand3.rank = 11;
                hand3.strength = 30;
                hand3.pId = 3;
                hands.Add(hand3);

                Assert.AreEqual(1, determineWinner(hands));
        }

    }
}
