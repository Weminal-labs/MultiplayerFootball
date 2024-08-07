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
        sendButton.clicked += SendMessageToReact;
        closeButton.clicked += CloseUnityApplication;
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
        string jsonString = JsonUtility.ToJson(jsonData);

        // Call the JavaScript function to send the message to React
        Application.ExternalCall("FinishGame", jsonString);

        // Optionally, you can close the application after sending the message
        Application.Quit();
    }

    private void CloseUnityApplication()
    {
        // Close the Unity application
        Application.Quit();
    }
}
