using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyGameManager : MonoBehaviourPunCallbacks
{
    public GameObject[] playerButtons;
    public GameObject[] goalkeeperButtons;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI playerScoreText;
    public TextMeshProUGUI goalkeeperScoreText;

    //public GameObject waitingForPlayersPanel;

    private int playerChoice = -1;
    private int goalkeeperChoice = -1;
    private int playerScore = 0;
    private int goalkeeperScore = 0;
    private int round = 0;

    private const int maxRounds = 5;

    private const string PlayerRole = "PlayerRole";
    private const string GoalkeeperRole = "GoalkeeperRole";

    void Start()
    {
        /*foreach (Button button in playerButtons)
        {
            Button tempButton = button;
            tempButton.onClick.AddListener(() => OnPlayerButtonClick(tempButton));
        }

        foreach (Button button in goalkeeperButtons)
        {
            Button tempButton = button;
            tempButton.onClick.AddListener(() => OnGoalkeeperButtonClick(tempButton));
        }*/

        UpdateButtonInteractivity();

        //waitingForPlayersPanel.SetActive(true);
        SetGameElementsInteractable(false);
    }

    public override void OnJoinedRoom()
    {
        CheckPlayerCount();
        AssignRoles();
        print("Player entered room");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CheckPlayerCount();
        AssignRoles();
    }

    void CheckPlayerCount()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            print(PhotonNetwork.CurrentRoom.PlayerCount);
            //waitingForPlayersPanel.SetActive(false);
            SetGameElementsInteractable(true);
        }
        else
        {
            //waitingForPlayersPanel.SetActive(true);
            SetGameElementsInteractable(false);
        }
    }

    void SetGameElementsInteractable(bool interactable)
    {
        print(interactable);
        foreach (GameObject button in playerButtons)
        {
            button.SetActive(interactable && IsPlayerRole());
        }

        foreach (GameObject button in goalkeeperButtons)
        {
            button.SetActive(interactable && IsGoalkeeperRole());
        }
    }

    public void OnPlayerButtonClick(Button button)
    {
        print("Player clicked");
        if (IsPlayerRole())
        {
            playerChoice = System.Array.IndexOf(playerButtons, button);
            photonView.RPC("PlayerChose", RpcTarget.Others, playerChoice);
            CheckChoices();
        }
    }

    public void OnGoalkeeperButtonClick(Button button)
    {
        print("Player clicked");

        if (IsGoalkeeperRole())
        {
            goalkeeperChoice = System.Array.IndexOf(goalkeeperButtons, button);
            photonView.RPC("GoalkeeperChose", RpcTarget.Others, goalkeeperChoice);
            CheckChoices();
        }
    }

    [PunRPC]
    void PlayerChose(int choice)
    {
        playerChoice = choice;
        CheckChoices();
    }

    [PunRPC]
    void GoalkeeperChose(int choice)
    {
        goalkeeperChoice = choice;
        CheckChoices();
    }

    void CheckChoices()
    {
        if (playerChoice != -1 && goalkeeperChoice != -1)
        {
            round++;
            if (playerChoice == goalkeeperChoice)
            {
                resultText.text = "Goalkeeper Wins!";
                goalkeeperScore++;
            }
            else
            {
                resultText.text = "Player Wins!";
                playerScore++;
            }

            playerScoreText.text = "Player Score: " + playerScore;
            goalkeeperScoreText.text = "Goalkeeper Score: " + goalkeeperScore;

            if (round >= maxRounds)
            {
                if (playerScore > goalkeeperScore)
                {
                    resultText.text = "Player Wins the Game!";
                }
                else
                {
                    resultText.text = "Goalkeeper Wins the Game!";
                }
            }

            // Swap roles
            SwapRoles();
            UpdateButtonInteractivity();

            playerChoice = -1;
            goalkeeperChoice = -1;
        }
    }

    void SwapRoles()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player.CustomProperties.ContainsKey(PlayerRole))
            {
                player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { PlayerRole, false } });
                player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { GoalkeeperRole, true } });
            }
            else if (player.CustomProperties.ContainsKey(GoalkeeperRole))
            {
                player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { GoalkeeperRole, false } });
                player.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { PlayerRole, true } });
            }
        }
    }

    bool IsPlayerRole()
    {
        bool isPlayerRole = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(PlayerRole) && (bool)PhotonNetwork.LocalPlayer.CustomProperties[PlayerRole];
        Debug.Log("IsPlayerRole: " + isPlayerRole);
        return isPlayerRole;
    }


    bool IsGoalkeeperRole()
    {
        bool isPlayerRole = PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey(GoalkeeperRole) && (bool)PhotonNetwork.LocalPlayer.CustomProperties[GoalkeeperRole];
        Debug.Log("IsPlayerRole: " + isPlayerRole);

        return isPlayerRole;
    }

    void AssignRoles()
    {
        print("Assigning roles");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            print("Player count: " + PhotonNetwork.CurrentRoom.PlayerCount);
            Player[] players = PhotonNetwork.PlayerList;

            // Assign roles
            players[0].SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { PlayerRole, true } });
            players[0].SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { GoalkeeperRole, false } });
            players[1].SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { PlayerRole, false } });
            players[1].SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { GoalkeeperRole, true } });

            // Print assigned roles
            Debug.Log("Player 1 (ActorNumber: " + players[0].ActorNumber + ") - PlayerRole: " + players[0].CustomProperties[PlayerRole] + ", GoalkeeperRole: " + players[0].CustomProperties[GoalkeeperRole]);
            Debug.Log("Player 2 (ActorNumber: " + players[1].ActorNumber + ") - PlayerRole: " + players[1].CustomProperties[PlayerRole] + ", GoalkeeperRole: " + players[1].CustomProperties[GoalkeeperRole]);
        }
    }


    void UpdateButtonInteractivity()
    {

        foreach (GameObject button in playerButtons)
        {
            button.SetActive(IsPlayerRole());
        }

        foreach (GameObject button in goalkeeperButtons)
        {
            button.SetActive(IsGoalkeeperRole());
        }
    }
}
