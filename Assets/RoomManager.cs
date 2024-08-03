using Aptos.Accounts;
using Aptos.BCS;
using Aptos.Unity.Rest;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Linq;
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
    private int allRound = 0;

    private const int maxRounds = 5;

    private GameObject localPlayerObject;


    public GameObject scoreboardManager;

    void Awake()
    {
        instance = this;
#if !UNITY_EDITOR && UNITY_WEBGL
WebGLInput.captureAllKeyboardInput = false;

#endif

        PhotonNetwork.UseRpcMonoBehaviourCache = true;

        PhotonNetwork.PhotonServerSettings.AppSettings.NetworkLogging = ExitGames.Client.Photon.DebugLevel.ALL;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 12000000; // 12000 seconds
        PhotonNetwork.KeepAliveInBackground = 12000000;
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
        scoreboardManager.SetActive(true);
        //scoreboardManager.GetComponent<ScoreboardManager>().UpdateName();
        scoreboardManager.GetComponent<ScoreboardManager>().CallUpdateScores();

    }

    void AssignRole()
    {
        // Determine role based on the number of players already in the room
        role = PhotonNetwork.CurrentRoom.PlayerCount;
        int[] turnScores = new int[maxRounds];

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "userAdress", RoomPlayer.Instance.userAdress } });

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "turnScores", turnScores } });
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
        playerSetup.GetComponent<PhotonView>().RPC("SetRole", RpcTarget.AllBufferedViaServer, role);

        playerSetup.isLocalPlayer();

        // Rotate the goalkeeper by 180 degrees on the y-axis
        if (role == PlayerRoles.RoleTwo)
        {
            localPlayerObject.transform.Rotate(0, 180, 0);
            playerSetup.ui.transform.Rotate(0, 0, 180);
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

            if (!isGoalkeeper && playerChoice != opponentChoice)
            {
                // Player scores a goal
                UpdateScore(1, currentRound);
            }
            if (!isGoalkeeper && playerChoice == opponentChoice)
            {
                // Player scores a goal
                UpdateScore(-1, currentRound);
            }



            TriggerAnimations();
            yield return new WaitForSeconds(3.5f);

            playerChoice = -1;
            opponentChoice = -1;

            currentRound++;
            allRound++;

            if (allRound >= 2 * maxRounds)
            {
                DetermineWinnerAndSendAddress();
                Application.Quit();
            }

            if (currentRound >= maxRounds)
            {
                Debug.Log("Round Over");
                currentRound = 0;
                DisplayFinalScore();
                SwapRoles();
            }

            Respawn();


        }
    }

    void OnApplicationQuit()
    {

        // Send a message to React when the application quits
        Application.ExternalCall("onUnityApplicationQuit");
    }
    void TriggerAnimations()
    {
        if (localPlayerObject != null)
        {
            double animationStartTime = PhotonNetwork.Time + 0.2f;

            localPlayerObject.GetComponent<PlayerSetup>().GetComponent<PhotonView>().RPC("TriggerPenaltyKickAnimation", RpcTarget.AllBufferedViaServer, playerChoice, opponentChoice, animationStartTime);
        }
    }

    void UpdateScore(int result, int round)
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            var playerSetup = player.TagObject as PlayerSetup;
            if (playerSetup != null && playerSetup.GetRole() == PlayerRoles.RoleOne)
            {
                int[] turnScores = (int[])player.CustomProperties["turnScores"];
                turnScores[round] = result;
                player.SetCustomProperties(new Hashtable { { "turnScores", turnScores } });
            }
        }
        scoreboardManager.GetComponent<ScoreboardManager>().CallUpdateScores();
    }

    void SwapRoles()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            var playerSetup = player.TagObject as PlayerSetup;
            if (playerSetup != null)
            {
                int currentRole = playerSetup.GetRole();
                int newRole = currentRole == PlayerRoles.RoleOne ? PlayerRoles.RoleTwo : PlayerRoles.RoleOne;
                //playerSetup.photonView.RPC("SetRole", RpcTarget.AllBufferedViaServer, newRole);
                role = newRole;
            }
        }
    }



    void DisplayFinalScore()
    {
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int[] turnScores = (int[])player.CustomProperties["turnScores"];

            var playerSetup = player.TagObject as PlayerSetup;
            if (playerSetup != null)
            {
                Debug.Log($"{player.NickName} turn scores: {string.Join(", ", turnScores)}");
            }
        }
    }

    IEnumerator SetNet()
    {
        RestClient restClient = RestClient.Instance.SetEndPoint(Constants.TESTNET_BASE_URL);
        Coroutine restClientSetupCor = StartCoroutine(RestClient.Instance.SetUp());
        yield return restClientSetupCor;

        AptosTokenClient tokenClient = AptosTokenClient.Instance.SetUp(restClient);
    }

    public void CheckWallet(string winnerAddress)
    {
        StartCoroutine(SetNet());

        // Initialize Account Using Hexadecimal Private Key.
        /*        const string PrivateKeyHex = "0xee87f9f47a79a0ccc9ab0f31a18a60e96e1de7ee4ed6f6a54081930a54916a45";
                Account bob = Account.LoadKey(PrivateKeyHex);

                print("BOB Account Address: " + bob.AccountAddress);*/

        ISerializable[] transactionArguments ={
        new U64((ulong)RoomPlayer.Instance.roomId),
        new BString(winnerAddress),
    };

        // Initialize the Payload.
        EntryFunction payload = EntryFunction.Natural(
            new ModuleId(AccountAddress.FromHex("4dc362f62787da9c0655223fe2819fbac878345cbc5115674e89326e20a42ed7"), "gamev3"), // Package ID and Module Name.
            "pick_winner_and_transfer_bet", // Function Name.
            new TagSequence(new ISerializableTag[0]), // Type Arguments.
            new Sequence(transactionArguments) // Arguments.
        );
    }


    // Determine winner after all rounds are complete
    private void DetermineWinnerAndSendAddress()
    {
        string winnerAddress = string.Empty;
        int maxGoals = int.MinValue;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            int[] turnScores = (int[])player.CustomProperties["turnScores"];
            int playerGoals = turnScores.Count(score => score == 1);

            if (playerGoals > maxGoals)
            {
                maxGoals = playerGoals;
                winnerAddress = (string)player.CustomProperties["userAddress"];
            }
        }
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (player.IsMasterClient)
            {
                CheckWallet(winnerAddress);
            }
        }
    }
}

public static class PlayerRoles
{
    public const int RoleOne = 1; // Player
    public const int RoleTwo = 2; // Goalkeeper
}
