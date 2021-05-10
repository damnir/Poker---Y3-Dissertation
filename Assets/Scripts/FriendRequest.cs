using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MLAPI;
using static MLAPI.Spawning.NetworkSpawnManager;

public class FriendRequest : MonoBehaviour
{
    public TMP_Text usernameTMP;
    public string netId;
    public string username;
    //set all data
    public void init(string _username, string _netId)
    {
        netId = _netId;
        username = _username;
        usernameTMP.text = _username;
    }

    //on accept, make a db call to accept the friend request
    public void onAcceptClick()
    {
        string cId = GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().netId.Value;
        string cUserName = GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().username.Value;

        LoginManager.instance.acceptFriend(username, netId, cId, cUserName);
        //StartCoroutine(LoginManager.instance.acceptFriendRequest(netId, username));
        Destroy(this);
    }

    //show object infro as a profile
    public void onViewClick()
    {
        LoginManager.instance.showProfileClient(netId, GetLocalPlayerObject().GetComponent<Player>().netId.Value, "Menu");
    }

    public void onViewClickInGame()
    {
        LoginManager.instance.showProfileClient(netId, GetLocalPlayerObject().GetComponent<Player>().netId.Value, GetLocalPlayerObject().GetComponent<Player>().currentLobby.Value.name);

    }
}
