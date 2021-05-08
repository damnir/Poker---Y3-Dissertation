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
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
                        [UnityTest]
        public IEnumerator RegisterUsernameTaken()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
                                [UnityTest]
                public IEnumerator RegisterEmailTaken()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
                        [UnityTest]
                        public IEnumerator RegisterWeakPassword()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
                        [UnityTest]
                                public IEnumerator RegisterPasswordsDontMatch()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
