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
using MLAPI.Messaging;
using Newtonsoft.Json;

public class LoginManager : NetworkBehaviour
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

    public static LoginManager instance;
    [Header("Friends List")]
    public TMP_InputField friendUsernameField;
    public GameObject friendRequestGo;
    public GameObject friendRequestsListGo;
    public GameObject friendsGo;
    public GameObject friendsListGo;
    public TMP_Text sendFriendText;
    public GameObject friendIgGo;
    public GameObject friendsListInGameGo;
    public GameObject gameInviteGo;
    public GameObject profileGo;

    [Header("Leaderboard")]
    public GameObject leadPlayerGo;
    public GameObject leaderboardList;

    [Header("Replay")]
    public GameObject replayGo;
    public GameObject replayList;
    public GameObject replayScene;

    void Awake()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //Initialize Firebase
                InitializeFirebase();
            }
        });

        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(this);
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += networkDisconnect;
    }

    public void networkDisconnect(ulong id)
    {
        StartCoroutine(clientDisconnectDB(id.ToString()));
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);

        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
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
            User = LoginTask.Result;
            warningLoginText.text = "";
            confirmLoginText.text = "Welcome Back: " + User.DisplayName + "!";

            StartCoroutine(LoadUserData());
            StartCoroutine(SetOnlineStatus(User.UserId, true));
            
            StartCoroutine(LoadFriendRequests());
            StartCoroutine(LoadFriendsList());
            StartCoroutine(clientConnected());

            yield return new WaitForSeconds(2);

            ButtonManager.instance.MenuScreen();
            confirmLoginText.text = "";
            GameObject.Find("Menu").GetComponent<Canvas>().enabled = true;

        }
    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            warningRegisterText.text = "Missing Username";
        }
        else if(passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            warningRegisterText.text = "Password Does Not Match!";
        }
        else 
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

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
                //User created
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile{DisplayName = _username};

                    var ProfileTask = User.UpdateUserProfileAsync(profile);

                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    StartCoroutine(UpdateUsernameDatabase(_username));
                    StartCoroutine(UpdateCash(100000));
                    StartCoroutine(InitStats(User.UserId));
                    StartCoroutine(SetOnlineStatus(User.UserId, false));

                    //UpdateCash(100000);
                    ButtonManager.instance.LoginScreen();                        
                    warningRegisterText.text = "";
                    
                }
            }
        }
    }

    private IEnumerator InitStats(string _id)
    {
        var DBTask = DBreference.Child("users").Child(_id).Child("xp").SetValueAsync(0);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        var DBTask2 = DBreference.Child("users").Child(_id).Child("xp").SetValueAsync(0);

        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

        var DBTask3 = DBreference.Child("users").Child(_id).Child("hands_played").SetValueAsync(0);

        yield return new WaitUntil(predicate: () => DBTask3.IsCompleted);

        var DBTask4 = DBreference.Child("users").Child(_id).Child("hands_won").SetValueAsync(0);

        yield return new WaitUntil(predicate: () => DBTask4.IsCompleted);
        
        var DBTask5 = DBreference.Child("users").Child(_id).Child("biggest_win").SetValueAsync(0);

        yield return new WaitUntil(predicate: () => DBTask5.IsCompleted);

    }

    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        //Set the username from current login
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
    }

    private IEnumerator UpdateCash(int _cash)
    {
        //Set the currently logged in user deaths
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("cash").SetValueAsync(_cash);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
    }

    private IEnumerator LoadUserData()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        
        if (DBTask.Result.Value == null) //If logged in for the first time set cash to 100000
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
                newFriendRequest.transform.SetParent(friendRequestsListGo.transform, false);
            }
        }
    }

    
    private IEnumerator LoadFriendsList()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("friends").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }

        else
        {
            Debug.Log("Load Friend Request");
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            foreach(DataSnapshot dataSnapshot in snapshot.Children)
            {
                Debug.Log("Friend request from: " + dataSnapshot.Child("username").Value);
                GameObject newFriendRequest = Instantiate(friendsGo, new Vector3(transform.position.x,transform.position.y, transform.position.z) , Quaternion.identity);
                newFriendRequest.GetComponent<FriendRequest>().init(dataSnapshot.Child("username").Value.ToString(), dataSnapshot.Child("id").Value.ToString());
                newFriendRequest.transform.SetParent(friendsListGo.transform, false);
            }
        }
    }

    public IEnumerator UpdateCashClient(string id, int _cash)
    {
        var DBTask = DBreference.Child("users").Child(id).Child("cash").SetValueAsync(_cash);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
    }

    public IEnumerator AddNewGame(DataManager.Game _game)
    {
        string key = DBreference.Child("games").Push().Key; //create a new unique ID for the game
        string json = JsonUtility.ToJson(_game); //covert game object to JSON

        var DBTask = DBreference.Child("games").Child(key).SetRawJsonValueAsync(json);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        foreach(string player in _game.players) //Add game ID to all players that were in that game session
        {
            var playerD = JsonConvert.DeserializeObject<Dictionary<string, string>>(player);
            var DBTaskP = DBreference.Child("users").Child(playerD["netId"]).Child("games").Child(key).SetValueAsync(DateTime.Now.ToString());
            yield return new WaitUntil(predicate: () => DBTaskP.IsCompleted);

        }
    }

    public void replay(string _id, bool winsOnly, bool pot)
    {
        StartCoroutine(PopulateReplay(_id, winsOnly, pot));
    }

    public IEnumerator PopulateReplay(string _id, bool orderByWins, bool orderByPot)
    {
        foreach(Transform item in replayList.transform)
        {
            Destroy(item.gameObject);
        }
        var DBTask = DBreference.Child("users").Child(_id).Child("games").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        DataSnapshot userGamesResult = DBTask.Result;

        List<string> userGames = new List<string>();

        foreach(DataSnapshot game in userGamesResult.Children)
        {
            var DBTask2 = DBreference.Child("games").Child(game.Key).GetValueAsync();
            
            yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

            userGames.Add(DBTask2.Result.GetRawJsonValue());
        }

        foreach(string json in userGames)
        {
            DataManager.Game game = DataManager.Game.CreateFromJSON(json);

            if(orderByPot)
            {
                //do nothing
            }
            else if(orderByWins)
            {
                if(!game.winners.Contains(_id))
                {
                    continue;
                }
            }
            else if(!orderByWins)
            {
                if(game.winners.Contains(_id))
                {
                    continue;
                }
            }

            GameObject newGame = Instantiate(replayGo, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
            
            string win;
            if(game.winners.Contains(_id))
            {
                win = "( - WON - )";
            }
            else
            {
                win = "( - LOST - )";
            }
            newGame.GetComponent<ReplayGo>().setValues(game, String.Format("{0} |  Pot: ${1} | Players: {2} | {3}", win, game.win, game.players.Count, game.time));
            newGame.transform.SetParent(replayList.transform, false);
        }

    }

    public IEnumerator SetOnlineStatus(string _id, bool _status)
    {
        //Set the currently logged in user deaths
        var DBTask = DBreference.Child("users").Child(_id).Child("status").Child("online").SetValueAsync(_status);
        //set users online status
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        DBTask = DBreference.Child("users").Child(_id).Child("status").Child("ClientID").SetValueAsync(NetworkManager.Singleton.LocalClientId.ToString());

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

    }

    public void quickAdd(string _username, string _id, string targetId)
    {
        Friend friend = new Friend(_username, _id);

        StartCoroutine(sendFriendRequest(targetId, friend));
    }

    public IEnumerator addFriend(string _username)
    {
        var DBTask = DBreference.Child("users").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);
        bool playerFound = false;
        if (DBTask.Exception != null)
        {
            Debug.Log(DBTask.Exception);
        }
        else
        {
            DataSnapshot snapshot = DBTask.Result;

            //Loop through every users UID
            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                if(childSnapshot.Child("username").Value == null)
                {
                    continue;
                }
                if(childSnapshot.Child("username").Value.ToString() == _username) //find the username
                {
                    Friend friend = new Friend(User.DisplayName, User.UserId);
                    StartCoroutine(sendFriendRequest(childSnapshot.Key, friend));
                    sendFriendText.text = "Request sent to: " + _username;
                    yield return new WaitForSeconds(2);
                    sendFriendText.text = "";
                    playerFound = true;
                }
            }
            if(!playerFound) //username not found, set error message
            {
                sendFriendText.text = "No player w/ username '" + _username +"' found.";
                yield return new WaitForSeconds(2);
                sendFriendText.text = "";
            }
            friendUsernameField.text = "";
        }
    }

    public IEnumerator sendFriendRequest(string _userId, Friend _friend)
    {
        string json = JsonUtility.ToJson(_friend);

        var DBTask = DBreference.Child("users").Child(_userId).Child("friend_requests").Child(_friend.id).SetRawJsonValueAsync(json);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

    }

    public IEnumerator acceptFriendRequest(string _userId, Friend _friend, Friend _senderFriend)
    {
        //add each others user ID to the 'friends' field
        string json = JsonUtility.ToJson(_friend);

        var DBTask = DBreference.Child("users").Child(_userId).Child("friends").Child(_friend.id).SetRawJsonValueAsync(json);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        json = JsonUtility.ToJson(_senderFriend);

        DBTask = DBreference.Child("users").Child(_friend.id).Child("friends").Child(_senderFriend.id).SetRawJsonValueAsync(json);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

    }

    public IEnumerator removeFriendRequest(string _userId, string _FRID)
    {
        //remove friends requests from users DB field
        var DBTask = DBreference.Child("users").Child(_userId).Child("friend_requests").Child(_FRID).RemoveValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

    }

private IEnumerator UpdateFriendRequests(string _id)
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(_id).Child("friend_requests").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(DBTask.Exception);
        }

        else if (DBTask.Result.Value == null)
        {
            foreach (Transform child in friendRequestsListGo.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.Log("Load Friend Request");
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            foreach (Transform child in friendRequestsListGo.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach(DataSnapshot dataSnapshot in snapshot.Children)
            {
                Debug.Log("Friend request from: " + dataSnapshot.Child("username").Value);
                GameObject newFriendRequest = Instantiate(friendRequestGo, new Vector3(transform.position.x,transform.position.y, transform.position.z) , Quaternion.identity);
                newFriendRequest.GetComponent<FriendRequest>().init(dataSnapshot.Child("username").Value.ToString(), dataSnapshot.Child("id").Value.ToString());
                newFriendRequest.transform.SetParent(friendRequestsListGo.transform, false);
            }
        }
    }

    
    private IEnumerator UpdateFriendsList(string _id)
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(_id).Child("friends").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(DBTask.Exception);
        }

        else
        {
            Debug.Log("Load Friend Request");
            DataSnapshot snapshot = DBTask.Result;
            foreach (Transform child in friendsListGo.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach(DataSnapshot dataSnapshot in snapshot.Children)
            {
                Debug.Log("Friend request from: " + dataSnapshot.Child("username").Value);
                GameObject newFriendRequest = Instantiate(friendsGo, new Vector3(transform.position.x,transform.position.y, transform.position.z) , Quaternion.identity);
                newFriendRequest.GetComponent<FriendRequest>().init(dataSnapshot.Child("username").Value.ToString(), dataSnapshot.Child("id").Value.ToString());
                newFriendRequest.transform.SetParent(friendsListGo.transform, false);
            }
        }
    }

    public void updateXp(string _id, int _xp)
    {
        StartCoroutine(UpdateXpDB(_id, _xp));
    }

    private IEnumerator UpdateXpDB(string _id, int _xp)
    {
        var DBTask = DBreference.Child("users").Child(_id).Child("xp").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if(DBTask.Result == null)
        {
            yield break;
        }

        var DBTask2 = DBreference.Child("users").Child(_id).Child("xp").SetValueAsync(Int32.Parse(DBTask.Result.Value.ToString()) + _xp);

        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
    }

    public void updateHandsWon(string _id, int won)
    {
        StartCoroutine(UpdateHandsWonDB(_id, won));
    }

    private IEnumerator UpdateHandsWonDB(string _id, int won)
    {
        var DBTask = DBreference.Child("users").Child(_id).Child("hands_won").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if(DBTask.Result == null)
        {
            yield break;
        }

        var DBTask2 = DBreference.Child("users").Child(_id).Child("hands_won").SetValueAsync(Int32.Parse(DBTask.Result.Value.ToString()) + won);

        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
    }

    public void updateHandsPlayed(string _id, int played)
    {
        StartCoroutine(UpdateHandsPlayedDB(_id, played));
    }

    private IEnumerator UpdateHandsPlayedDB(string _id, int played)
    {
        var DBTask = DBreference.Child("users").Child(_id).Child("hands_played").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if(DBTask.Result == null)
        {
            yield break;
        }

        var DBTask2 = DBreference.Child("users").Child(_id).Child("hands_played").SetValueAsync(Int32.Parse(DBTask.Result.Value.ToString()) + played);

        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);
    }

    public void updateBiggestWin(string _id, int won)
    {
        StartCoroutine(UpdateBiggestWinDB(_id, won));
    }

    private IEnumerator UpdateBiggestWinDB(string _id, int won)
    {
        var DBTask = DBreference.Child("users").Child(_id).Child("biggest_win").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if(DBTask.Result == null)
        {
            yield break;
        }
        else if(Int32.Parse(DBTask.Result.Value.ToString()) > won)
        {
            yield break;
        }

        var DBTask2 = DBreference.Child("users").Child(_id).Child("biggest_win").SetValueAsync(won);

        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

    }

    public void UpdateFriends(string _id)
    {
        StartCoroutine(UpdateFriendRequests(_id));
        StartCoroutine(UpdateFriendsList(_id));

    }

    public void UpdateFriendsListIg(string _senderId)
    {
        StartCoroutine(UpdateFriendsListInGame(_senderId));
    }

    private IEnumerator UpdateFriendsListInGame(string _id)
    {
        var DBTask = DBreference.Child("users").Child(_id).Child("friends").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        var DBTask2 = DBreference.Child("online_players").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(DBTask.Exception);
        }
        else
        {
            Debug.Log("Load Friend Request");
            DataSnapshot snapshot = DBTask.Result;
            DataSnapshot snapshot2 = DBTask2.Result;

            foreach (Transform child in friendsListInGameGo.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach(DataSnapshot dataSnapshot in snapshot.Children)
            {
                foreach(DataSnapshot onlineSnapshot in snapshot2.Children)
                {
                    if(dataSnapshot.Key == onlineSnapshot.Key)
                    {
                        GameObject newFriendRequest = Instantiate(friendIgGo, new Vector3(transform.position.x,transform.position.y, transform.position.z) , Quaternion.identity);
                        newFriendRequest.GetComponent<FriendRequest>().init(dataSnapshot.Child("username").Value.ToString(), dataSnapshot.Child("id").Value.ToString());
                        newFriendRequest.transform.SetParent(friendsListInGameGo.transform, false);
                    }
                }   
            }
        }
    }

    public IEnumerator clientConnected()
    {
        string _clientId = NetworkManager.Singleton.LocalClientId.ToString();
        var DBTask = DBreference.Child("online_players").Child(User.UserId).Child("ClientID").SetValueAsync(_clientId);
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

    }

    public IEnumerator clientDisconnectDB(string _id)
    {
        var DBTask = DBreference.Child("online_players").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(DBTask.Exception);
        }

        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            foreach(DataSnapshot dataSnapshot in snapshot.Children)
            {
                if(dataSnapshot.Child("ClientID").Value.ToString() == _id)
                {
                    var DBTask2 = DBreference.Child("users").Child(dataSnapshot.Key).Child("status").Child("online").SetValueAsync(false);
                    yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);

                    if (DBTask2.Exception != null)
                    {
                        Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                    }
                    var DBTask3 = DBreference.Child("online_players").Child(dataSnapshot.Key).RemoveValueAsync();
                    yield return new WaitUntil(predicate: () => DBTask3.IsCompleted);

                    if (DBTask3.Exception != null)
                    {
                        Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                    }
                }
            }
        }
    }

    private IEnumerator SendGameInviteDB(string _id, string _lobbyName, string _username)
    {
        var DBTask = DBreference.Child("online_players").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        DataSnapshot snapshot = DBTask.Result;

        foreach(DataSnapshot dataSnapshot in snapshot.Children)
        {
            if(dataSnapshot.Key == _id)
            {
                sendInviteServerRpc(_lobbyName, _username, dataSnapshot.Child("ClientID").Value.ToString());
            }
        }
    }

    public void populateLeaderboard(string _id, string _sortBy, bool _friendOnly)
    {
        StartCoroutine(PopulateLeaderBoardDB(_id, _sortBy, _friendOnly));
    }

    private IEnumerator PopulateLeaderBoardDB(string _id, string _sortBy, bool _friendsOnly)
    {
        foreach(Transform item in leaderboardList.transform)
        {
            Destroy(item.gameObject);
        }

        var DBTask = DBreference.Child("users").OrderByChild(_sortBy).GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        DataSnapshot result = DBTask.Result;
        int i = 1;

        if(_friendsOnly)
        {
            var DBTaskF = DBreference.Child("users").Child(_id).Child("friends").GetValueAsync();
            yield return new WaitUntil(predicate: () => DBTaskF.IsCompleted);

            DataSnapshot friends = DBTaskF.Result;
            List<string> friendIds = new List<string>();

            foreach(DataSnapshot friend in friends.Children)
            {
                friendIds.Add(friend.Key.ToString());
            }
           
            foreach(DataSnapshot user in result.Children.Reverse<DataSnapshot>())
            {
                if(friendIds.Contains(user.Key.ToString()))
                {
                    GameObject leadPlayer = Instantiate(leadPlayerGo, new Vector3(transform.position.x,transform.position.y, transform.position.z) , Quaternion.identity);
                    leadPlayer.transform.Find("text").GetComponent<TextMeshProUGUI>().text = String.Format("{0} {1} | ${2} | XP: {3}xp | Hands Played: {4} | Wins: {5} | Biggest Win: ${6}",
                        i, user.Child("username").Value.ToString(), user.Child("cash").Value.ToString(), user.Child("xp").Value.ToString(), user.Child("hands_played").Value.ToString(),
                        user.Child("hands_won").Value.ToString(), user.Child("biggest_win").Value.ToString());
                    leadPlayer.transform.SetParent(leaderboardList.transform, false);
                    i++;
                }
            }
        }

        else
        {
            foreach(DataSnapshot user in result.Children.Reverse<DataSnapshot>())
            {
                GameObject leadPlayer = Instantiate(leadPlayerGo, new Vector3(transform.position.x,transform.position.y, transform.position.z) , Quaternion.identity);
                leadPlayer.transform.Find("text").GetComponent<TextMeshProUGUI>().text = String.Format("{0} {1} | ${2} | XP: {3}xp | Hands Played: {4} | Wins: {5} | Biggest Win: ${6}",
                    i, user.Child("username").Value.ToString(), user.Child("cash").Value.ToString(), user.Child("xp").Value.ToString(), user.Child("hands_played").Value.ToString(),
                    user.Child("hands_won").Value.ToString(), user.Child("biggest_win").Value.ToString());

                leadPlayer.transform.SetParent(leaderboardList.transform, false);
               i++;
            }
        }
    }

    public void showProfileClient(string _id, string senderId, string parent)
    {
        StartCoroutine(ShowProfile(_id, senderId, parent));
    }

    public IEnumerator ShowProfile(string _id, string senderId, string parent)
    {
        var DBTask = DBreference.Child("users").Child(_id).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);  
        DataSnapshot snapshot = DBTask.Result;

        var DBTask2 = DBreference.Child("users").Child(senderId).Child("friends").Child(_id).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask2.IsCompleted);  
         bool isFriend;

        if(DBTask2.Result.Value == null && _id != GetLocalPlayerObject().GetComponent<Player>().netId.Value)
        {
            isFriend = false;
        }
        else
        {
            isFriend = true;
        }
        
        GameObject profile = Instantiate(profileGo, new Vector3(transform.position.x,transform.position.y, transform.position.z) , Quaternion.identity);
            profile.GetComponent<ViewProfile>().setValues(snapshot.Child("username").Value.ToString(), snapshot.Child("cash").Value.ToString(),
            snapshot.Child("xp").Value.ToString(), snapshot.Child("hands_played").Value.ToString(), snapshot.Child("hands_won").Value.ToString(),
            snapshot.Child("biggest_win").Value.ToString(), isFriend, _id);

        if(parent == "Menu")
        {
            profile.transform.SetParent(GameObject.Find(parent).transform, false);
        }
        else
        {
            profile.transform.SetParent(GameObject.Find("GameInvitePlaceholder").transform, false);

        }

    }

    [ServerRpc(RequireOwnership = false)]
    public void sendInviteServerRpc(string lobbyName, string username, string id)
    {
        ClientRpcParams poo = new ClientRpcParams();
        poo.Send.TargetClientIds = new ulong[1];
        poo.Send.TargetClientIds[0] = UInt64.Parse(id);
        Debug.Log("TARGET ID: " + id);
        ulong[] yourClientIds = new ulong[]{UInt64.Parse(id)+1};
        sendInviteClientRpc(lobbyName, username, id, new ClientRpcParams {
        Send = new ClientRpcSendParams {
                    TargetClientIds = new ulong[]{UInt64.Parse(id)}
                }
            }
        );
    }

    [ClientRpc]
    public void sendInviteClientRpc(string lobbyName, string username, string id, ClientRpcParams clientRpcParams = default)
    {
        Debug.Log("Game Invite received - lobby: " + lobbyName + " username: " + username);
        GetLocalPlayerObject().GetComponent<Player>().gameInvite(lobbyName, username);
    }
    public void SendGameInvite(GameObject _lobby, string senderUsername, string targetId)
    {
        Debug.Log("Send invite - before coroutine");

        StartCoroutine(SendGameInviteDB(targetId, String.Format("{0} | Players: {1}/7 | ${2}/${3}", _lobby.name,
         _lobby.GetComponent<DataManager>().playerNumNet.Value, _lobby.GetComponent<DataManager>().smallBlind, 
         _lobby.GetComponent<DataManager>().bigBlind), senderUsername));
    }

    public void clientDisconnect(ulong _id)
    {
        StartCoroutine(clientDisconnectDB(_id.ToString()));
    }
    

    public void acceptFriend(string username, string id, string _senderId, string _senderUsername)
    {
        Friend friend = new Friend(username, id);
        Friend sender = new Friend(_senderUsername, _senderId);

        StartCoroutine(acceptFriendRequest(_senderId, friend, sender));
        StartCoroutine(removeFriendRequest(_senderId, id));

        StartCoroutine(UpdateFriendRequests(_senderId));
        StartCoroutine(UpdateFriendsList(_senderId));
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