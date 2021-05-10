using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Firebase;
using Firebase.Database;

namespace Tests
{
    public class ReplayGameDBTest{

        [UnityTest]
        public IEnumerator JSONTest1()
        {
            DatabaseReference DBreference = FirebaseDatabase.DefaultInstance.RootReference;
            var DBTask = DBreference.Child("games").Child("-MZAMN_kJPj2aITbnN2L").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            string jsonGame = DBTask.Result.GetRawJsonValue();

            DataManager.Game game = DataManager.Game.CreateFromJSON(jsonGame);

            Assert.AreEqual(game.winners[0], "BHOjBQKEi0OLaBg6Pb0hD18BPaA2");
            Assert.AreEqual(game.win, 4144);
        }

        [UnityTest]
        public IEnumerator JSONTest2()
        {
            DatabaseReference DBreference = FirebaseDatabase.DefaultInstance.RootReference;
            var DBTask = DBreference.Child("games").Child("-MZ_5HBsFx1Xl80Sa3lm").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            string jsonGame = DBTask.Result.GetRawJsonValue();

            DataManager.Game game = DataManager.Game.CreateFromJSON(jsonGame);

            Assert.AreEqual(game.winners[0], "O3vV7t2cI3dItSmQx0DEaq2qxzD3");
            Assert.AreEqual(game.win, 880);
        }

        [UnityTest]
        public IEnumerator JSONTest3()
        {
            DatabaseReference DBreference = FirebaseDatabase.DefaultInstance.RootReference;
            var DBTask = DBreference.Child("games").Child("-MZnn1wFGXf2xKy4ybS-").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            string jsonGame = DBTask.Result.GetRawJsonValue();

            DataManager.Game game = DataManager.Game.CreateFromJSON(jsonGame);

            Assert.AreEqual(game.winners[0], "Vlggn6WpjBYCQQ3VPrwj26NHZaw2");
            Assert.AreEqual(game.win, 400);
        }

        [UnityTest]
        public IEnumerator JSONTest4()
        {
            DatabaseReference DBreference = FirebaseDatabase.DefaultInstance.RootReference;
            var DBTask = DBreference.Child("games").Child("-M_7HHN-4hWYZHeZeVPv").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            string jsonGame = DBTask.Result.GetRawJsonValue();

            DataManager.Game game = DataManager.Game.CreateFromJSON(jsonGame);

            Assert.AreEqual(game.winners[0], "c25bD9kcRzeWoXuuYiYMhCW9szW2");
            Assert.AreEqual(game.win, 2400);
        }

        [UnityTest]
        public IEnumerator JSONTest5()
        {
            DatabaseReference DBreference = FirebaseDatabase.DefaultInstance.RootReference;
            var DBTask = DBreference.Child("games").Child("-M_7C1R-5bLCAgUMViYH").GetValueAsync();

            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            string jsonGame = DBTask.Result.GetRawJsonValue();

            DataManager.Game game = DataManager.Game.CreateFromJSON(jsonGame);

            Assert.AreEqual(game.winners[0], "O3vV7t2cI3dItSmQx0DEaq2qxzD3");
            Assert.AreEqual(game.win, 800);
        }

 
        
    }
}
