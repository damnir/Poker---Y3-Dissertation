using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Connection;
using MLAPI.Messaging;
using MLAPI.Transports.UNET;
using MLAPI.Serialization.Pooled;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using static MLAPI.Spawning.NetworkSpawnManager;


public class ButtonManager : NetworkBehaviour
{
    public GameObject buttonServer;
    public GameObject buttonClient;
    public GameObject buttonLobby2;
    public GameObject lobbyList;
    public GameObject data;
    public GameObject foldButton;
    public GameObject callButton;
    public GameObject raiseButton;
    public GameObject slider;
    public GameObject callText;
    public GameObject raiseText;
    public GameObject checkText;
    public GameObject loginScreen;
    public GameObject friendsList;
    public GameObject friendsListInGame;
    public GameObject loadingScreen;
    public GameObject seats;
    public GameObject sitUpButton;
    public GameObject leaveButton;
    public GameObject menu;
    public GameObject chatBox;
    public GameObject messageBox;
    public GameObject messageGo;
    public TMP_InputField messageInput;
    public GameObject bing;
    public GameObject leaderboard;
    public GameObject playButton;
    public GameObject replayButton;
    public GameObject replayScene;
    public GameObject quitButton;


    //leaderboard vars
    private bool leadFriendsOnly;
    private string sortBy;

    void Start() {
        //NetworkManager.Singleton.StartServer();
        leadFriendsOnly = false;
        sortBy = "cash";
    }

    public void onServerClicked()
    {
        NetworkManager.Singleton.StartServer();
        buttonServer.SetActive(false);
        buttonClient.SetActive(false);
    }

    public void onClientClicker()
    {
        loginScreen.SetActive(true);
        //lobbyList.SetActive(true);

        NetworkManager.Singleton.StartClient();
        buttonClient.SetActive(false);
        buttonServer.SetActive(false);

    }

    public void onFoldClicked()
    {
        GetLocalPlayerObject().GetComponent<Player>().fold();
    }

    public void onCallClicked()
    {
        GetLocalPlayerObject().GetComponent<Player>().call();

    }

    public void onRaiseClicked() 
    {
        GetLocalPlayerObject().GetComponent<Player>().raise((ulong)slider.GetComponent<Slider>().value);
    }

    public void updateCall(ulong value)
    {
        if (value > 0)
        {
            checkText.GetComponent<TextMeshProUGUI>().text = "<sprite=3>Call";
        }
        else
        {
            checkText.GetComponent<TextMeshProUGUI>().text = "<sprite=1>Check";
        }
        if(value < 1)
        {
            value = 0;
        }
        callText.GetComponent<Text>().text = "$" + value;
    }

    public void updateRaise(ulong min, ulong max)
    {
        if(min < 1 || max < 1)
        {
            min = 0;
            max = 0;
        }
        //slider.GetComponent<Slider>().value = min;
        slider.GetComponent<Slider>().minValue = min;
        slider.GetComponent<Slider>().maxValue = max;
        raiseText.GetComponent<Text>().text ="$"+(ulong)slider.GetComponent<Slider>().value;
    }

    public void onSliderValueChanged() {
        raiseText.GetComponent<Text>().text ="$"+(ulong)slider.GetComponent<Slider>().value;
    }

    public static ButtonManager instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;

    private void Awake()
    {
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

    //Functions to change the login screen UI
    public void LoginScreen() //Back button
    {
        loginUI.SetActive(true);
        registerUI.SetActive(false);
    }
    public void RegisterScreen() // Regester button
    {
        loginUI.SetActive(false);
        registerUI.SetActive(true);
    }

    public void MenuScreen()
    {
        loginScreen.SetActive(false);
    }

    public void onFriendsListClick()
    {
        if(friendsList.active)
        {
            friendsList.SetActive(false);
        }
        else{
            LoginManager.instance.UpdateFriends(GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().netId.Value);
            friendsList.SetActive(true);
        }
    }

    public void onFriendsListGameClick()
    {
        if(friendsListInGame.active)
        {
            friendsListInGame.SetActive(false);
        }
        else{
            LoginManager.instance.UpdateFriendsListIg(GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().netId.Value);
            friendsListInGame.SetActive(true);
        }
    }

    public void setLoadingScreen()
    {
        if(loadingScreen.active)
        {
            loadingScreen.SetActive(false);
        }
        else{
            loadingScreen.SetActive(true);
        }  
    }

    public void hideSeats()
    {
        foreach(Transform seat in seats.transform)
        {
            seat.GetComponent<Image>().sprite = Resources.Load<Sprite>("blank");
        }
        sitUpButton.SetActive(true);
        leaveButton.SetActive(false);
    }

    public void showSeats()
    {
        foreach(Transform seat in seats.transform)
        {
            seat.GetComponent<Image>().sprite = Resources.Load<Sprite>("emptyseat");
        }  
        sitUpButton.SetActive(false);
        leaveButton.SetActive(true);

        GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().sitUp();
    }

    public void onLeaveClick()
    {
        menu.SetActive(true);
        Player player = GetLocalPlayerObject().GetComponent<Player>();
        //GetLocalPlayerObject().GetComponent<Player>().changeLobby(lobby);
        player.currentLobby.Value = null;
    }

    public void onChatClick()
    {
        if(chatBox.active)
        {
            chatBox.SetActive(false);
        }
        else{
            chatBox.SetActive(true);
            //updateMessages();
        }  
        bing.SetActive(false);
    }

    public void onMessageSendClick()
    {
        if(messageInput.text == "")
        {
            return;
        }

        DataManager lobbyData = GetLocalPlayerObject().GetComponent<Player>().currentLobby.Value.GetComponent<DataManager>();
        lobbyData.messages.Add(GetLocalPlayerObject().GetComponent<Player>().username.Value + ": " + messageInput.text);
        messageInput.text = "";
    }

    public void updateMessages(string message)
    {

        GameObject newMessage = Instantiate(messageGo, new Vector3(transform.position.x,transform.position.y, transform.position.z) , Quaternion.identity);
        newMessage.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        newMessage.transform.SetParent(messageBox.transform, false);
        bing.SetActive(true);

    }

    public void onLeaderboardClick()
    {
        if(leaderboard.active)
        {
            leaderboard.SetActive(false);
        }
        else{
            LoginManager.instance.populateLeaderboard(GetLocalPlayerObject().GetComponent<Player>().netId.Value, "cash", false);
            leaderboard.SetActive(true);
            //updateMessages();
        }       
    }

    void updateLB()
    {
        LoginManager.instance.populateLeaderboard(GetLocalPlayerObject().GetComponent<Player>().netId.Value, sortBy, leadFriendsOnly);     
    }

    public void SortByCash()
    {
        sortBy = "cash";
        updateLB();
    }

    public void SortByXp()
    {
        sortBy = "xp";
        updateLB();
    }

    public void LBFriendsOnly()
    {
        leadFriendsOnly = true;
        updateLB();
    }

    public void LBGloval()
    {
        leadFriendsOnly = false;
        updateLB();
    }

    public void onPlayClicked()
    {
        lobbyList.SetActive(true);
        playButton.SetActive(false);
        replayButton.SetActive(false);
        quitButton.SetActive(true);
    }

    public void onReplayClicked()
    {
        lobbyList.SetActive(false);
        playButton.SetActive(false);
        replayButton.SetActive(false);
        quitButton.SetActive(true);  
    }


}
