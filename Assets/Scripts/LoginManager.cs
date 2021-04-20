using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using MLAPI;
using System;
using static MLAPI.Spawning.NetworkSpawnManager;


public class LoginManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;    
    public FirebaseUser User;
    public DatabaseReference DBreference;

    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    //User Data variables
    [Header("UserData")]
    public TMP_InputField usernameField;
    public TMP_InputField xpField;
    public TMP_InputField killsField;
    public TMP_InputField deathsField;
    public GameObject scoreElement;
    public Transform scoreboardContent;

    public static LoginManager instance;
    public TMP_InputField friendUsernameField;
    public GameObject friendRequestGo;
    public GameObject friendRequestsListGo;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    public void ClearLoginFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }
    public void ClearRegisterFeilds()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    
    //Function for the save button
    public void SaveDataButton()
    {
        StartCoroutine(UpdateUsernameAuth(User.DisplayName));
        StartCoroutine(UpdateUsernameDatabase(User.DisplayName));

        //StartCoroutine(UpdateXp(50));
        //StartCoroutine(UpdateKills(100));
        //StartCoroutine(UpdateDeaths(2));
        StartCoroutine(UpdateCash(57382));
    }
    //Function for the scoreboard button
    public void ScoreboardButton()
    {        
        StartCoroutine(LoadScoreboardData());
    }

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";

            StartCoroutine(LoadUserData());
            StartCoroutine(SetOnlineStatus(User.UserId, true));
            
            StartCoroutine(LoadFriendRequests());



            yield return new WaitForSeconds(2);

            //usernameField.text = User.DisplayName;
            ButtonManager.instance.MenuScreen(); // Change to user data UI
            confirmLoginText.text = "";
            ClearLoginFeilds();
            ClearRegisterFeilds();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
        }
        else if(passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        }
        else 
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile{DisplayName = _username};

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen

                        StartCoroutine(UpdateUsernameDatabase(User.DisplayName));
                        StartCoroutine(UpdateCash(100000));
                        StartCoroutine(SetOnlineStatus(User.UserId, false));

                        //UpdateCash(100000);
                        ButtonManager.instance.LoginScreen();                        
                        warningRegisterText.text = "";
                        ClearRegisterFeilds();
                        ClearLoginFeilds();
                    }
                }
            }
        }
    }


    private IEnumerator UpdateUsernameAuth(string _username)
    {
        //Create a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _username };

        //Call the Firebase auth update user profile function passing the profile with the username
        var ProfileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            //Auth username is now updated
        }        
    }

    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        //Set the currently logged in user username in the database
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    private IEnumerator UpdateCash(int _cash)
    {
        //Set the currently logged in user deaths
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("cash").SetValueAsync(_cash);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Deaths are now updated
        }
    }

    private IEnumerator LoadUserData()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        
        else if (DBTask.Result.Value == null)
        {
            GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().updateCash(100000);
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().setUsername(User.DisplayName);

            GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().updateCash(Int32.Parse(snapshot.Child("cash").Value.ToString()));
            GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().setNetId(User.UserId);

            Debug.Log("Cash updated");
        }
    }

    private IEnumerator LoadFriendRequests()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("friend_requests").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }

        else if (DBTask.Result.Value == null)
        {
        }
        else
        {
            Debug.Log("Load Friend Request");
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            foreach(DataSnapshot dataSnapshot in snapshot.Children)
            {
                Debug.Log("Friend request from: " + dataSnapshot.Child("username").Value);
                GameObject newFriendRequest = Instantiate(friendRequestGo, new Vector3(transform.position.x,transform.position.y, transform.position.z) , Quaternion.identity);
                newFriendRequest.GetComponent<FriendRequest>().init(dataSnapshot.Child("username").Value.ToString(), dataSnapshot.Child("id").Value.ToString());
                //newFriendRequest.GetComponentInChildren<
                //newFriendRequest.transform.parent = friendRequestsListGo.transform;
                newFriendRequest.transform.SetParent(friendRequestsListGo.transform);
            }
        }
    }

    private IEnumerator LoadScoreboardData()
    {
        //Get all the users data ordered by kills amount
        var DBTask = DBreference.Child("users").OrderByChild("kills").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //Destroy any existing scoreboard elements
            foreach (Transform child in scoreboardContent.transform)
            {
                Destroy(child.gameObject);
            }

            //Loop through every users UID
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                string username = childSnapshot.Child("username").Value.ToString();
                int kills = int.Parse(childSnapshot.Child("kills").Value.ToString());
                int deaths = int.Parse(childSnapshot.Child("deaths").Value.ToString());
                int xp = int.Parse(childSnapshot.Child("xp").Value.ToString());

                //Instantiate new scoreboard elements
                GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
                //////scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, kills, deaths, xp);
            }

            //Go to scoareboard screen
            ///////ButtonManager.instance.ScoreboardScreen();
        }
    }

    public IEnumerator UpdateCashClient(string id, int _cash)
    {
        //Set the currently logged in user deaths
        var DBTask = DBreference.Child("users").Child(id).Child("cash").SetValueAsync(_cash);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Deaths are now updated
        }
    }

    public IEnumerator AddNewGame(DataManager.Game _game)
    {
                Debug.Log("addCalled");

        //Set the currently logged in user deaths
        //var DBTask = DBreference.Child("games").push;
        string key = DBreference.Child("games").Push().Key;
        string json = JsonUtility.ToJson(_game);

        var DBTask = DBreference.Child("games").Child(key).SetRawJsonValueAsync(json);


        //DBreference.UpdateChildrenAsync(_game);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Deaths are now updated
        }
    }
    public IEnumerator SetOnlineStatus(string _id, bool _status)
    {
        //Set the currently logged in user deaths
        var DBTask = DBreference.Child("users").Child(_id).Child("online").SetValueAsync(_status);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Deaths are now updated
        }
    }

    public IEnumerator addFriend(string _username)
    {
        var DBTask = DBreference.Child("users").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //Loop through every users UID
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                if(childSnapshot.Child("username").Value.ToString() == _username)
                {
                    Debug.Log("User found: " + _username);
                    Debug.Log("Sender: " + User.DisplayName + " ID" + User.UserId);
                    Friend friend = new Friend(User.DisplayName, User.UserId);
                    //Debug.Log("childSnapshot: " + childSnapshot.ToString());
                    StartCoroutine(sendFriendRequest(childSnapshot.Key, friend));

                }
            }
        }
    }

    public IEnumerator sendFriendRequest(string _userId, Friend _friend)
    {
        string json = JsonUtility.ToJson(_friend);
        //Set the currently logged in user deaths
        //string key = DBreference.Child("friend_requests").Push()._friend.;
        var DBTask = DBreference.Child("users").Child(_userId).Child("friend_requests").Child(_friend.id).SetRawJsonValueAsync(json);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            Debug.Log("Sent friend request to " + _friend.username);
            //Deaths are now updated
        }
    }

    public IEnumerator acceptFriendRequest(string _userId, Friend _friend)
    {

        string json = JsonUtility.ToJson(_friend);
        //Set the currently logged in user deaths
        //string key = DBreference.Child("friends").Push().Key;
        var DBTask = DBreference.Child("users").Child(_userId).Child("friends").Child(_friend.id).SetRawJsonValueAsync(json);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Debug.Log("Sent friend request to " + _friend.username);
            //Deaths are now updated
        }
    }

    public IEnumerator removeFriendRequest(string _userId, string _FRID)
    {
        var DBTask = DBreference.Child("users").Child(_userId).Child("friend_requests").Child(_FRID).RemoveValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Debug.Log("Sent friend request to " + _friend.username);
            //Deaths are now updated
        }
    }

    public void acceptFriend(string username, string id, string _senderId)
    {
        Friend friend = new Friend(username, id);
        StartCoroutine(acceptFriendRequest(_senderId, friend));
        StartCoroutine(removeFriendRequest(_senderId, id));
    }

    public void onSearchClick()
    {
        StartCoroutine(addFriend(friendUsernameField.text));
    }

    public class Friend
    {
        public string username;
        public string id;

        public Friend()
        {

        }

        public Friend(string _username, string _id)
        {
            this.username = _username;
            this.id = _id;
        }
    }


}