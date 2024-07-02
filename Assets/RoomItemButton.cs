using UnityEngine;

public class RoomItemButton : MonoBehaviour
{
    public string roomName;

    public void OnClick()
    {
        RoomList.Instance.JoinRoomByName(roomName);
    }
}
