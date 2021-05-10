using UnityEngine;
using MLAPI.Messaging;
using static MLAPI.Spawning.NetworkSpawnManager;


public class SendGameInvite : MonoBehaviour
{
    public GameObject inviteGo;
    ClientRpcParams clientRpcParams = new ClientRpcParams();


    public void sendInvite()
    {

        Debug.Log("Send invite called Button");
        string targetId = this.GetComponent<FriendRequest>().netId;
        string username = GetLocalPlayerObject().GetComponent<Player>().username.Value;
        GameObject lobby = GetLocalPlayerObject().GetComponent<Player>().currentLobby.Value;

        LoginManager.instance.SendGameInvite(lobby, username, targetId);

    }
}
