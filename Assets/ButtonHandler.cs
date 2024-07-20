// Attach this script to your button UI elements
using UnityEngine;

public class ButtonHandler : MonoBehaviour
{
    public int buttonIndex; // Set this to the appropriate button index in the Inspector

    public void OnButtonClick()
    {
        RoomManager.instance.OnButtonClick(buttonIndex);
    }
}
