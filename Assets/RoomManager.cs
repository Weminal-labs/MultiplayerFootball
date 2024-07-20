using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;

    public GameObject playerPrefab;
    public GameObject ballPrefab;

    [Space]
    public GameObject roomCam;

    [Space]
    public GameObject nameUI;
    public GameObject connectingUI;

    private string nickname = "unname";

    [HideInInspector]
    public int role = 0;

    public string roomNameToJoin = "test";

    [Space]
    public Transform[] spawnPointsTeamTwo;

    public GameObject buttonGoalkeeper;

    private int playerChoice = -1;
    private int opponentChoice = -1;

    private int currentRound = 0;
    private const int maxRounds = 5;

    private GameObject localPlayerObject;

    void Awake()
    {
        instance = this;
    }

    public void ChangeNickname(string _name)
    {
        nickname = _name;
    }

    public void JoinRoomButtonPress()
    {
        PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, new RoomOptions { MaxPlayers = 2 }, TypedLobby.Default);

        nameUI.SetActive(false);
        connectingUI.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined room.");
        roomCam.SetActive(false);

        AssignRole();
        Respawn();
    }

    void AssignRole()
    {
        // Determine role based on the number of players already in the room
        int currentPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        if (currentPlayerCount == 1)
        {
            role = PlayerRoles.RoleOne; // Player
        }
        else if (currentPlayerCount == 2)
        {
            role = PlayerRoles.RoleTwo; // Goalkeeper
        }

        // Set the role in player's custom properties
        Hashtable hash = new Hashtable();
        hash["role"] = role;

        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    public void Respawn()
    {
        if (localPlayerObject != null)
        {
            PhotonNetwork.Destroy(localPlayerObject);
        }

        localPlayerObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPointsTeamTwo[role - 1].position, Quaternion.identity);
        PlayerSetup playerSetup = localPlayerObject.GetComponent<PlayerSetup>();

        playerSetup.GetComponent<PhotonView>().RPC("SetName", RpcTarget.AllBuffered, nickname);
        playerSetup.GetComponent<PhotonView>().RPC("SetRole", RpcTarget.AllBuffered, role);
        playerSetup.isLocalPlayer();

        // Rotate the goalkeeper by 180 degrees on the y-axis
        if (role == PlayerRoles.RoleTwo)
        {
            localPlayerObject.transform.Rotate(0, 180, 0);
        }

        PhotonNetwork.LocalPlayer.NickName = nickname;
    }

    public void OnButtonClick(int index)
    {
        if (currentRound >= maxRounds)
        {
            Debug.Log("Game Over");
            return;
        }

        Debug.Log(index);
        playerChoice = index;
        photonView.RPC("ReceiveChoice", RpcTarget.Others, index);
        CheckWinner();
    }

    [PunRPC]
    void ReceiveChoice(int index)
    {
        Debug.Log(index);
        opponentChoice = index;
        CheckWinner();
    }

    void CheckWinner()
    {
        if (playerChoice != -1 && opponentChoice != -1)
        {
            // Determine roles
            bool isGoalkeeper = role == PlayerRoles.RoleTwo;
            bool opponentIsGoalkeeper = !isGoalkeeper;

            if ((isGoalkeeper && playerChoice == opponentChoice) || (opponentIsGoalkeeper && playerChoice != opponentChoice))
            {
                Debug.Log("Goalkeeper wins!");
                UpdateScore(isGoalkeeper ? PlayerRoles.RoleTwo : PlayerRoles.RoleOne);
            }
            else
            {
                Debug.Log("Goalkeeper loses!");
                UpdateScore(isGoalkeeper ? PlayerRoles.RoleOne : PlayerRoles.RoleTwo);
            }

            // Reset choices for next round
            playerChoice = -1;
            opponentChoice = -1;

            // Increment the round
            currentRound++;
            if (currentRound >= maxRounds)
            {
                Debug.Log("Round Over");
                SwapRoles();
                Respawn();
                currentRound = 0; // Reset round count for the next set
            }
        }
    }

    void UpdateScore(int scoringRole)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int playerRole = (int)player.CustomProperties["role"];
            if (playerRole == scoringRole)
            {
                int currentScore = player.CustomProperties.ContainsKey("score") ? (int)player.CustomProperties["score"] : 0;
                Hashtable hash = new Hashtable();
                hash["score"] = currentScore + 1;
                player.SetCustomProperties(hash);
                Debug.Log($"{player.NickName} score: {hash["score"]}");
            }
        }
    }

    void SwapRoles()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int currentRole = (int)player.CustomProperties["role"];
            int newRole = currentRole == PlayerRoles.RoleOne ? PlayerRoles.RoleTwo : PlayerRoles.RoleOne;

            Hashtable hash = new Hashtable();
            hash["role"] = newRole;
            player.SetCustomProperties(hash);
        }
    }

    void DisplayFinalScore()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            int score = player.CustomProperties.ContainsKey("score") ? (int)player.CustomProperties["score"] : 0;
            Debug.Log($"{player.NickName} final score: {score}");
        }
    }
}

public static class PlayerRoles
{
    public const int RoleOne = 1; // Player
    public const int RoleTwo = 2; // Goalkeeper
}
