using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PlayerOrderTest{
        
        //**Function taken from DataManager - can't be tested from the class itself as it has network behaviour
        List<ulong> playerOrder = new List<ulong>();
        List<ulong> seatOrder = new List<ulong>(){5, 0, 0, 8, 3, 2, 4};
        public void orderPlayers(bool reOrder, int dealer) {
                playerOrder.Clear();

                if(reOrder) {
                        foreach (ulong id in seatOrder)
                        {
                                if(id != 0){
                                        playerOrder.Add(id);
                                }
                        } 
                        playerOrder.Reverse();

                        for(int i = 0; i < dealer; i++)
                        {
                                ulong temp = playerOrder[0];
                                playerOrder.Add(temp);
                                playerOrder.RemoveAt(0);
                                }
                        }
                else
                {
                        foreach (ulong id in seatOrder)
                        {
                                if(id != 0) {
                                playerOrder.Add(id);
                                }
                        }
                        
                        playerOrder.Reverse();
                        
                        for(int i = 0; i < dealer; i++){
                                ulong temp = playerOrder[0];
                                playerOrder.Add(temp);
                                playerOrder.RemoveAt(0);
                        }
                }
        }

                [Test]
        public void PlayerOrderTest1()
        {
                orderPlayers(false, 0);

                List<ulong> expectedOrder = new List<ulong>(){4,2,3,8,5};
                
                Assert.AreEqual(expectedOrder, playerOrder);

        } 
                [Test]
        public void PlayerOrderTest2()
        {
                orderPlayers(false, 1);

                List<ulong> expectedOrder = new List<ulong>(){2,3,8,5,4};
                
                Assert.AreEqual(expectedOrder, playerOrder);

        } 
                [Test]
        public void PlayerOrderTest3()
        {
                orderPlayers(false, 2);

                List<ulong> expectedOrder = new List<ulong>(){3,8,5,4,2};
                
                Assert.AreEqual(expectedOrder, playerOrder);
        } 
                [Test]
        public void PlayerOrderTest4()
        {
                orderPlayers(false, 3);

                List<ulong> expectedOrder = new List<ulong>(){8,5,4,2,3};
                
                Assert.AreEqual(expectedOrder, playerOrder);
        } 
                [Test]
        public void PlayerOrderTest5()
        {
                orderPlayers(false, 4);

                List<ulong> expectedOrder = new List<ulong>(){5,4,2,3,8};
                
                Assert.AreEqual(expectedOrder, playerOrder);
        } 
                [Test]
        public void PlayerOrderTest6()
        {
                orderPlayers(false, 5);

                List<ulong> expectedOrder = new List<ulong>(){4,2,3,8,5};
                
                Assert.AreEqual(expectedOrder, playerOrder);
        } 
                [Test]
        public void PlayerOrderTest7()
        {
                orderPlayers(true, 5);

                List<ulong> expectedOrder = new List<ulong>(){4,2,3,8,5};
                
                Assert.AreEqual(expectedOrder, playerOrder);
        } 
                [Test]
        public void PlayerOrderTest8()
        {
                playerOrder.Remove(5);
                orderPlayers(true, 0);

                List<ulong> expectedOrder = new List<ulong>(){4,2,3,8,5};
                
                Assert.AreEqual(expectedOrder, playerOrder);
        } 
                [Test]
        public void PlayerOrderTest9()
        {
                orderPlayers(true, 1);

                List<ulong> expectedOrder = new List<ulong>(){2,3,8,5,4};
                
                Assert.AreEqual(expectedOrder, playerOrder);
        } 
                        [Test]
        public void PlayerOrderTest10()
        {
                orderPlayers(true, 2);

                List<ulong> expectedOrder = new List<ulong>(){3,8,5,4,2};
                
                Assert.AreEqual(expectedOrder, playerOrder);       
        } 
    }
}