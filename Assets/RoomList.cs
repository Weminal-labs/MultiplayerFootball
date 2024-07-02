using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomList : MonoBehaviourPunCallbacks
{

    public static RoomList Instance;

    public GameObject roomManagerGameObject;

    public RoomManager roomManager;

    [Header("UI References")]
    public Transform roomListParent;
    public GameObject roomListingPrefab;

    private List<RoomInfo> cacheRoomList = new List<RoomInfo>();


    public void ChangeRoomToCreateName(string _name)
    {
        roomManager.roomNameToJoin = _name;
    }


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

        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (cacheRoomList.Count <= 0)
        {
            cacheRoomList = roomList;
        }
        else
        {
            foreach (var room in roomList)
            {
                for (int i = 0; i < cacheRoomList.Count; i++)
                {
                    if (room.Name == cacheRoomList[i].Name)
                    {
                        List<RoomInfo> newList = cacheRoomList;

                        if (room.RemovedFromList)
                        {
                            newList.RemoveAt(i);
                        }
                        else
                        {
                            newList[i] = room;
                        }

                        cacheRoomList = newList;
                    }
                }
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        foreach (Transform transform in roomListParent)
        {
            Destroy(transform.gameObject);
        }

        foreach (var room in cacheRoomList)
        {
            GameObject roomItem = Instantiate(roomListingPrefab, roomListParent);

            roomItem.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = room.Name;
            roomItem.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = room.PlayerCount + " /8";

            roomItem.GetComponent<RoomItemButton>().roomName = room.Name;
        }
    }

    public void JoinRoomByName(string _name)
    {
        roomManager.roomNameToJoin = _name;
        roomManagerGameObject.SetActive(true);
        gameObject.SetActive(false);
    }

}
