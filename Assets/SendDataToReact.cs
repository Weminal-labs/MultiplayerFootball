using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class SendDataToReact : MonoBehaviour
{

    private Label winLabel;
    private Label player1Score;
    private Label player2Score;

    void OnEnable()
    {
        // Get the root of the UI
        var root = GetComponent<UIDocument>().rootVisualElement;

        // Find the buttons by their names
        var sendButton = root.Q<Button>("sendUserIdButton");
        var closeButton = root.Q<Button>("exitButton");

        winLabel = root.Q<Label>("winLabel");
        player1Score = root.Q<Label>("player1Score");
        player2Score = root.Q<Label>("player2Score");


        // Register button click events
        closeButton.clicked += CloseUnityApplication;
    }

    public void UpdateScore(Player playerWin, int[] result)
    {
        if (playerWin.IsLocal)
        {
            winLabel.text = "You win!";
        }
        player1Score.text = result[1].ToString();
        player2Score.text = result[2].ToString();

        /*        int[] result = new int[2];
                foreach (var player in PhotonNetwork.PlayerList)
                {
                    int[] turnScores = (int[])player.CustomProperties["turnScores"];
                    int playerGoals = turnScores.Count(score => score == 1);
                    result[player.ActorNumber - 1] = playerGoals;
                }
                player1Score.text = result[0].ToString();
                player2Score.text = result[1].ToString();*/
    }


    public void SendMessageToReact()
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
    }

    private void CloseUnityApplication()
    {
        // Close the Unity application
#if UNITY_WEBGL == true && UNITY_EDITOR == false
                ExitGame();
#endif        
        Application.Quit();
    }

    [DllImport("__Internal")]
    private static extern void FinishGame(string jsonString);
    [DllImport("__Internal")]
    private static extern void ExitGame();
}
