using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;


public class RoomPlayer : MonoBehaviourPunCallbacks
{


    public static RoomPlayer Instance;

    public GameObject connectingScreen;
    public GameObject camm;
    public GameObject roomManagerGameObject;

    public int roomId = 0000;
    public string userAdress;

    public AudioSource audioSource;

    private void Awake()
    {
        Instance = this;

#if !UNITY_EDITOR && UNITY_WEBGL
WebGLInput.captureAllKeyboardInput = false;
#endif
        PhotonNetwork.PhotonServerSettings.AppSettings.NetworkLogging = ExitGames.Client.Photon.DebugLevel.ALL;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 12000000; // 12000 seconds
        PhotonNetwork.KeepAliveInBackground = 12000000;

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


        //roomListScreen.SetActive(true);
        connectingScreen.SetActive(false);
        camm.SetActive(false);
        PhotonNetwork.JoinLobby();
    }


    public IEnumerator JoinOrCreateRoom(string json)
    {
        yield return new WaitUntil(() => PhotonNetwork.InLobby);
        MyAPIData data = JsonConvert.DeserializeObject<MyAPIData>(json);
        // Use the data
        RoomManager.instance.ChangeNickname(data.userName);
        roomId = int.Parse(data.roomId);
        userAdress = data.userId;
        print("from Room PLayer" + roomId);
        print("from Room PLayer" + userAdress);
        PhotonNetwork.JoinOrCreateRoom(data.roomId, new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);
        roomManagerGameObject.SetActive(true);
    }

    public void SoundControl(int volumn)
    {
        audioSource.volume = volumn;
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"Disconnected due to: {cause}");
        switch (cause)
        {
            case DisconnectCause.ExceptionOnConnect:
            case DisconnectCause.Exception:
            case DisconnectCause.ServerTimeout:
            case DisconnectCause.ClientTimeout:
                // Thử kết nối lại sau một khoảng thời gian
                Invoke("ConnectToServer", 5f);
                break;
            default:
                // Xử lý các nguyên nhân khác hoặc thông báo cho người dùng
                break;
        }
    }
    public void ConnectToServer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
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
