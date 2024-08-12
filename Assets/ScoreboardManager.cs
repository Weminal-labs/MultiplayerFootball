using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UIElements;

public class ScoreboardManager : MonoBehaviourPunCallbacks
{
    private VisualElement player1TurnsContainer;
    private VisualElement player2TurnsContainer;
    private Label player1Name;
    private Label player2Name;
    private Label timer;

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        player1TurnsContainer = root.Q<VisualElement>("player1-turns");
        player2TurnsContainer = root.Q<VisualElement>("player2-turns");
        player1Name = root.Q<Label>("player1-name");
        player2Name = root.Q<Label>("player2-name");
        timer = root.Q<Label>("timer");

        if (PhotonNetwork.IsMasterClient)
        {
            UpdateScores();
        }

        player1Name.text = PhotonNetwork.LocalPlayer.NickName;
    }

    [PunRPC]
    public void UpdateScores()
    {
        // Get the current player's turn scores
        int[] player1TurnScores = (int[])PhotonNetwork.LocalPlayer.CustomProperties["turnScores"];
        int[] player2TurnScores = null;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.IsLocal)
            {
                player2TurnScores = (int[])player.CustomProperties["turnScores"];
                Debug.Log("Player 2 turn scores retrieved: " + string.Join(", ", player2TurnScores));
            }
        }

        if (player2TurnScores == null)
        {
            Debug.LogWarning("Player 2 turn scores not found.");
            return;
        }

        // Update the UI for player 1
        for (int i = 0; i < player1TurnScores.Length; i++)
        {
            UpdateTurnColor(player1TurnsContainer[i], player1TurnScores[i]);
        }

        // Update the UI for player 2
        for (int i = 0; i < player2TurnScores.Length; i++)
        {
            UpdateTurnColor(player2TurnsContainer[i], player2TurnScores[i]);
        }
    }

    public void CallUpdateScores()
    {
        photonView.RPC("UpdateScores", RpcTarget.All);
    }

    public void UpdateTimer(float time)
    {
        timer.text = time.ToString();
    }

    private void UpdateTurnColor(VisualElement turnElement, int score)
    {
        if (score > 0)
        {
            turnElement.style.backgroundColor = new StyleColor(Color.green);
        }
        else if (score == 0)
        {
            turnElement.style.backgroundColor = new StyleColor(Color.white);
        }
        else
        {
            turnElement.style.backgroundColor = new StyleColor(Color.red);
        }
    }

    public void setName(Player newPlayer)
    {
        player2Name.text = newPlayer.NickName;
    }
}
