using Photon.Pun;
using Photon.Realtime;
using System.Collections;
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

    private void Start()
    {
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
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
        role = PhotonNetwork.CurrentRoom.PlayerCount;

        // Set the role in player's custom properties
        UpdatePlayerProperty("role", role);
    }

    public void Respawn()
    {
        if (localPlayerObject != null)
        {
            PhotonNetwork.Destroy(localPlayerObject);
        }

        localPlayerObject = PhotonNetwork.Instantiate(playerPrefab.name, spawnPointsTeamTwo[role - 1].position, Quaternion.identity);
        var playerSetup = localPlayerObject.GetComponent<PlayerSetup>();

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

        StartCoroutine(CheckWinner());
    }

    [PunRPC]
    void ReceiveChoice(int index)
    {
        Debug.Log(index);
        opponentChoice = index;
        StartCoroutine(CheckWinner());
    }

    IEnumerator CheckWinner()
    {
        if (playerChoice != -1 && opponentChoice != -1)
        {
            bool isGoalkeeper = role == PlayerRoles.RoleTwo;
            bool opponentIsGoalkeeper = !isGoalkeeper;

            if ((isGoalkeeper && playerChoice == opponentChoice) || (opponentIsGoalkeeper && playerChoice != opponentChoice))
            {
                Debug.Log("Goalkeeper wins!");
                UpdateScore(isGoalkeeper ? PlayerRoles.RoleTwo : PlayerRoles.RoleOne);
                localPlayerObject.GetComponent<PlayerSetup>().isWin();
            }
            else
            {
                Debug.Log("Goalkeeper loses!");
                UpdateScore(isGoalkeeper ? PlayerRoles.RoleOne : PlayerRoles.RoleTwo);
            }

            TriggerAnimations();
            yield return new WaitForSeconds(4f);

            playerChoice = -1;
            opponentChoice = -1;

            currentRound++;

            if (currentRound >= maxRounds)
            {
                Debug.Log("Round Over");
                SwapRoles();
                currentRound = 0;
            }
            Respawn();
        }
    }

    void TriggerAnimations()
    {
        if (localPlayerObject != null)
        {
            double animationStartTime = PhotonNetwork.Time + 0.1f;

            localPlayerObject.GetComponent<PlayerSetup>().GetComponent<PhotonView>().RPC("TriggerPenaltyKickAnimation", RpcTarget.All, playerChoice, animationStartTime);
        }
    }

    void UpdateScore(int scoringRole)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int playerRole = (int)player.CustomProperties["role"];
            if (playerRole == scoringRole)
            {
                int currentScore = player.CustomProperties.ContainsKey("score") ? (int)player.CustomProperties["score"] : 0;
                UpdatePlayerProperty(player, "score", currentScore + 1);
            }
        }
    }

    void SwapRoles()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int currentRole = (int)player.CustomProperties["role"];
            int newRole = currentRole == PlayerRoles.RoleOne ? PlayerRoles.RoleTwo : PlayerRoles.RoleOne;
            UpdatePlayerProperty(player, "role", newRole);
        }
    }

    void DisplayFinalScore()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int score = player.CustomProperties.ContainsKey("score") ? (int)player.CustomProperties["score"] : 0;
            Debug.Log($"{player.NickName} final score: {score}");
        }
    }

    void UpdatePlayerProperty(string key, object value)
    {
        Hashtable hash = new Hashtable();
        hash[key] = value;
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }

    void UpdatePlayerProperty(Player player, string key, object value)
    {
        Hashtable hash = new Hashtable();
        hash[key] = value;
        player.SetCustomProperties(hash);
    }
}


public static class PlayerRoles
{
    public const int RoleOne = 1; // Player
    public const int RoleTwo = 2; // Goalkeeper
}
