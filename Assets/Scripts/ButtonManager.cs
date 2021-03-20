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



public class ButtonManager : NetworkedBehaviour
{
    public GameObject buttonServer;
    public GameObject buttonClient;
    public GameObject buttonLobby2;
    public GameObject lobbyList;

    public GameObject data;
    int test = 0;

    public void onServerClicked()
    {
        NetworkingManager.Singleton.StartServer();
        buttonServer.SetActive(false);
        buttonClient.SetActive(false);
        //data.SetActive(true);

        //GameObject.Find("Menu").SetActive(false);

    }

    public void onClientClicker()
    {
        lobbyList.SetActive(true);

        NetworkingManager.Singleton.StartClient();
        //test ++;
        //Debug.Log("Clients connected: " + clientId);
        buttonClient.SetActive(false);
        buttonServer.SetActive(false);

                //data.SetActive(true);
                

        //if (isClient) {
            Debug.Log(" --- I AM CLIENT ---" + NetworkingManager.Singleton.LocalClientId);
            using (PooledBitStream stream = PooledBitStream.Get())
            {
                using (PooledBitWriter writer = PooledBitWriter.Get(stream))
                {
                    writer.WriteInt32Packed(Random.Range(-50, 50));

                    InvokeServerRpcPerformance(MyServerRPC, stream);
                }
            }
       // }

    }

	void ClientConnected( ulong clientId ) 
	{
			Debug.Log( $"I'm connected {clientId}" );
	}

    [ServerRPC]
    private void MyServerRPC(ulong clientId, Stream stream) //This signature is REQUIRED for the performance mode
    {
        using (PooledBitReader reader = PooledBitReader.Get(stream))
        {
            int number = reader.ReadInt32Packed();
            Debug.Log("The number received was: " + number);
            Debug.Log("This method ran on the server upon the request of a client");
            
        }
    }

}
