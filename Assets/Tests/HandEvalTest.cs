using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


namespace Tests
{
    public class HandEvalTest
    {
        HandEvaluation HE = new HandEvaluation();
        // A Test behaves as an ordinary method
        [Test]
        public void RoyalFlushTest1()
        {
                List<string> cards = new List<string>{"Spade-05", "Diamond-13", "Diamond-10", "Diamond-11", "Diamond-12", "Diamond-14", "Diamond-01"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.straightflush, 0);
        }
        [Test]
        public void RoyalFlushTest2()
        {
                List<string> cards = new List<string>{"Spade-06", "Heart-13", "Heart-10", "Heart-11", "Heart-12", "Heart-01", "Heart-14"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.straightflush, 0);        
        }
        [Test]
        public void RoyalFlushTest3()
        {
                List<string> cards = new List<string>{"Spade-06", "Heart-01", "Heart-10", "Heart-14", "Heart-12", "Heart-13", "Heart-11"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.straightflush, 0);
        }
        [Test]
        public void RoyalFlushTest4()
        {
                List<string> cards = new List<string>{"Diamond-05", "Spade-11", "Spade-10", "Spade-11", "Spade-12", "Spade-14", "Spade-13"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.straightflush, 0);
        }
        [Test]
        public void RoyalFlushTest5()
        {
                List<string> cards = new List<string>{"Diamond-05", "Club-01", "Club-14", "Club-11", "Club-12", "Club-10", "Club-13"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.straightflush, 0);
        }

        [Test]
        public void StraightFlushTest1()
        {
                List<string> cards = new List<string>{"Diamond-05", "Club-01", "Club-09", "Club-11", "Club-12", "Club-10", "Club-08"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.straightflush, 0);
        }
                [Test]
        public void StraightFlushTest2()
        {
                List<string> cards = new List<string>{"Diamond-05", "Club-01", "Club-09", "Club-11", "Club-07", "Club-10", "Club-08"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.straightflush, 0);
        }
                [Test]
        public void StraightFlushTest3()
        {
                List<string> cards = new List<string>{"Diamond-05", "Club-06", "Club-09", "Club-11", "Club-07", "Club-10", "Club-08"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.straightflush, 0);
        }
                [Test]
        public void StraightFlushTest4()
        {
                List<string> cards = new List<string>{"Heart-07", "Heart-08", "Heart-10", "Heart-12", "Heart-11", "Heart-09", "Spade-05"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.straightflush, 0);
        }
                [Test]
        public void StraightFlushTest5()
        {
                List<string> cards = new List<string>{"Diamond-07", "Diamond-08", "Diamond-10", "Diamond-12", "Diamond-11", "Diamond-09", "Spade-05"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.straightflush, 0);
        }

        [Test]
        public void FourOfaKindTest1()
        {
                List<string> cards = new List<string>{"Diamond-07", "Spade-12", "Heart-12", "Diamond-09", "Club-12", "Diamond-09", "Spade-12"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.fourof, 0);
        }

                [Test]
        public void FourOfaKindTest2()
        {
                List<string> cards = new List<string>{"Diamond-07", "Spade-12", "Heart-07", "Diamond-09", "Club-07", "Diamond-09", "Spade-07"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.fourof, 0);
        }

                [Test]
        public void FourOfaKindTest3()
        {
                List<string> cards = new List<string>{"Diamond-07", "Spade-02", "Heart-12", "Diamond-02", "Club-02", "Diamond-09", "Spade-02"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.fourof, 0);
        }

                [Test]
        public void FourOfaKindTest4()
        {
                List<string> cards = new List<string>{"Diamond-11", "Spade-11", "Heart-04", "Diamond-11", "Club-11", "Diamond-09", "Spade-08"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.fourof, 0);
        }

                [Test]
        public void FourOfaKindTest5()
        {
                List<string> cards = new List<string>{"Diamond-07", "Spade-03", "Heart-04", "Diamond-04", "Club-12", "Diamond-04", "Spade-04"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.fourof, 0);
        }

                        [Test]
        public void FullHouseTest1()
        {
                List<string> cards = new List<string>{"Diamond-07", "Spade-07", "Heart-04", "Diamond-07", "Club-12", "Diamond-04", "Spade-02"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.fullhouse, 0);
        }

                        [Test]
        public void FullHouseTest2()
        {
                List<string> cards = new List<string>{"Diamond-07", "Spade-08", "Heart-08", "Diamond-08", "Club-12", "Diamond-04", "Spade-12"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.fullhouse, 0);
        }

                        [Test]
        public void FullHouseTest3()
        {
                List<string> cards = new List<string>{"Diamond-12", "Spade-07", "Heart-04", "Diamond-13", "Club-12", "Diamond-13", "Spade-13"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.fullhouse, 0);
        }

                                [Test]
        public void FullHouseTest4()
        {
                List<string> cards = new List<string>{"Diamond-02", "Spade-02", "Heart-04", "Diamond-02", "Club-12", "Diamond-04", "Spade-07"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.fullhouse, 0);
        }

                                [Test]
        public void FullHouseTest5()
        {
                List<string> cards = new List<string>{"Diamond-14", "Spade-11", "Heart-12", "Diamond-11", "Club-12", "Diamond-11", "Spade-02"};
                
                HandEvaluation.Rank rank = HE.evaluateHand(cards);

                Assert.AreNotEqual(rank.fullhouse, 0);
        }




    }
}
