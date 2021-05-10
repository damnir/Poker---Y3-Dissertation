using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

namespace Tests
{

    public class UserAuthentication
    {
        [UnityTest]
        public IEnumerator CorrectLoginDetails()
        {
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;

            var LoginTask = auth.SignInWithEmailAndPasswordAsync("damir@admin.com", "admin123");

            yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

            Assert.AreEqual(LoginTask.Exception, null);
        }

        [UnityTest]
        public IEnumerator IncorrectEmail()
        {
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;

            var LoginTask = auth.SignInWithEmailAndPasswordAsync("notrealemail@admin.com", "admin123");

            yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

            Assert.AreNotEqual(LoginTask.Exception, null);
        }
        [UnityTest]
        public IEnumerator IncorrectPassword()
        {
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;

            var LoginTask = auth.SignInWithEmailAndPasswordAsync("damir@admin.com", "admin1234567");

            yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

            Assert.AreNotEqual(LoginTask.Exception, null);
        }
        [UnityTest]
        public IEnumerator RegisterUsernameTaken()
        {
             FirebaseAuth auth = FirebaseAuth.DefaultInstance;

            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync("damir@admin.com", "admin1234567");

            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            Assert.AreNotEqual(RegisterTask.Exception, null);
        }
        [UnityTest]
        public IEnumerator RegisterEmailTaken()
        {
            yield return null;
        }
        [UnityTest]
        public IEnumerator RegisterWeakPassword()
        {
            FirebaseAuth auth = FirebaseAuth.DefaultInstance;

            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync("newuser@mail.com", "123");

            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            Assert.AreNotEqual(RegisterTask.Exception, null);
            yield return null;
        }

    }
}
