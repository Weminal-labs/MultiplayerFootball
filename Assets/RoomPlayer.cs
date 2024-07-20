using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;


public class RoomPlayer : MonoBehaviourPunCallbacks
{


    public static RoomPlayer Instance;

    public GameObject connectingScreen;
    public GameObject roomListScreen;

    private void Awake()
    {
        Instance = this;
    }



    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);



        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected to master server.");

        roomListScreen.SetActive(true);
        connectingScreen.SetActive(false);

        PhotonNetwork.JoinLobby();
    }


    private void JoinOrCreateRoom(string json)
    {

        MyAPIData data = JsonConvert.DeserializeObject<MyAPIData>(json);
        // Use the data
        RoomManager.instance.ChangeNickname(data.userName);

        PhotonNetwork.JoinOrCreateRoom(data.roomId, new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
    }

}

[System.Serializable]
public class MyAPIData
{
    public string roomId;
    public string roomName;
    public string userId;
    public string userName;
}
