using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;
using static MLAPI.Spawning.NetworkSpawnManager;


public class SendGameInvite : MonoBehaviour
{
    public GameObject inviteGo;
    ClientRpcParams clientRpcParams = new ClientRpcParams();


    public void sendInvite()
    {
        //clientRpcParams.Send.TargetClientIds = new ulong[1];
        //clientRpcParams.Send.TargetClientIds[0] = 

        Debug.Log("Send invite called Button");
        string targetId = this.GetComponent<FriendRequest>().netId;
        string username = GetLocalPlayerObject().GetComponent<Player>().username.Value;
        GameObject lobby = GetLocalPlayerObject().GetComponent<Player>().currentLobby.Value;

        LoginManager.instance.SendGameInvite(lobby, username, targetId);

    }
}
