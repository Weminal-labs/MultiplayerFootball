using Newtonsoft.Json;
using Photon.Pun;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class SendDataToReact : MonoBehaviour
{
    void OnEnable()
    {
        // Get the root of the UI
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Find the buttons by their names
        var sendButton = root.Q<Button>("sendUserIdButton");
        var closeButton = root.Q<Button>("exitButton");

        // Register button click events
        closeButton.clicked += SendMessageToReact;
        sendButton.clicked += CloseUnityApplication;
    }

    private void SendMessageToReact()
    {
        // Make sure that RoomPlayer.Instance and RoomManager.instance are properly initialized
        if (RoomPlayer.Instance == null || RoomManager.instance == null)
        {
            Debug.LogError("RoomPlayer.Instance or RoomManager.instance is not initialized.");
            return;
        }

        // Create a JSON object
        var jsonData = new
        {
            roomId = RoomPlayer.Instance.roomId,
            userId = RoomManager.instance.winnerAddress,
        };

        // Convert the JSON object to a string
        string jsonString = JsonConvert.SerializeObject(jsonData);

        print(jsonString);
        print(jsonString.GetType());

        // Call the JavaScript function to send the message to React

        // Optionally, you can close the application after sending the message

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
            {
#if UNITY_WEBGL == true && UNITY_EDITOR == false
                FinishGame(jsonString);
#endif
            }
        }
        Application.Quit();
    }

    private void CloseUnityApplication()
    {
        // Close the Unity application
        Application.Quit();
    }

    [DllImport("__Internal")]
    private static extern void FinishGame(string jsonString);

}