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

    public void init(string _username, string _netId)
    {
        netId = _netId;
        username = _username;
        usernameTMP.text = _username;
    }

    public void onAcceptClick()
    {
        string cId = GetPlayerNetworkObject(NetworkManager.Singleton.LocalClientId).GetComponent<Player>().netId.Value;
        LoginManager.instance.acceptFriend(username, netId, cId);
        //StartCoroutine(LoginManager.instance.acceptFriendRequest(netId, username));
        Destroy(this);
    }
}
