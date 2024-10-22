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

    private string nickname = "";

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

    public string winnerAddress = "0000";
    private int maxRounds = 3;

    private GameObject localPlayerObject;


    public GameObject scoreboardManager;

    public GameObject endGameScreen;

    private float countdownTimer = 10f;
    private bool choiceMade = false;

    public GameObject[] Banner;

    void Awake()
    {
        instance = this;
#if !UNITY_EDITOR && UNITY_WEBGL
WebGLInput.captureAllKeyboardInput = false;

#endif

        PhotonNetwork.UseRpcMonoBehaviourCache = true;

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
        roomCam.SetActive(false);

        AssignRole();
        Respawn();
        scoreboardManager.SetActive(true);
        //scoreboardManager.GetComponent<ScoreboardManager>().UpdateName();
        scoreboardManager.GetComponent<ScoreboardManager>().CallUpdateScores();

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        scoreboardManager.GetComponent<ScoreboardManager>().setName(newPlayer);
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            StartCoroutine(ShowBanner());
            StartCoroutine(StartCountdown());
        }
    }

    public IEnumerator ShowBanner()
    {
        Banner[role - 1].SetActive(true);
        yield return new WaitForSeconds(2f);
        Banner[role - 1].SetActive(false);
    }


    void AssignRole()
    {
        // Determine role based on the number of players already in the room
        role = PhotonNetwork.CurrentRoom.PlayerCount;
        int[] turnScores = new int[maxRounds];
        string address = RoomPlayer.Instance.userAdress;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "userAddress", address } });
        string winnerAddress = (string)PhotonNetwork.LocalPlayer.CustomProperties["userAddress"];
        print(winnerAddress);
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
        playerSetup.GetComponent<PhotonView>().RPC("SetSkin", RpcTarget.AllBufferedViaServer, PhotonNetwork.LocalPlayer.ActorNumber);

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
        choiceMade = true;

        if (currentRound > maxRounds)
        {
            return;
        }

        playerChoice = index;
        buttonGoalkeeper.SetActive(false);
        photonView.RPC("ReceiveChoice", RpcTarget.Others, index);

        StartCoroutine(CheckWinner());
    }

    private IEnumerator StartCountdown()
    {


        countdownTimer = 10f;
        choiceMade = false;

        while (countdownTimer > 0)
        {
            if (choiceMade)
                yield break;

            // Update the timer display
            scoreboardManager.GetComponent<ScoreboardManager>().UpdateTimer(countdownTimer);

            // Wait for 1 second
            yield return new WaitForSeconds(1f);

            // Decrease the timer by 1 second
            countdownTimer -= 1f;
        }

        // Handle the case when time runs out and no choice has been made
        if (!choiceMade)
        {
            OnButtonClick(0);
        }
    }



    [PunRPC]
    void ReceiveChoice(int index)
    {
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
                // Goalkeeper blocks the goal
                UpdateScore(-1, currentRound);
            }

            TriggerAnimations();
            yield return new WaitForSeconds(3.5f);
            Banner[2].SetActive(true);

            yield return new WaitForSeconds(1.5f);
            Banner[2].SetActive(false);

            yield return new WaitForSeconds(1f);

            Banner[role - 1].SetActive(true);

            playerChoice = -1;
            opponentChoice = -1;

            currentRound++;
            allRound++;

            yield return new WaitForSeconds(2f);

            Banner[role - 1].SetActive(false);

            if (allRound >= 2 * maxRounds)
            {
                if (CheckIfTied())
                {
                    maxRounds++; // Increase the max rounds for an extra round
                    print("Tied max round" + maxRounds);


                    int[] turnScores = (int[])PhotonNetwork.LocalPlayer.CustomProperties["turnScores"];
                    int[] newTurnScores = new int[maxRounds];
                    turnScores.CopyTo(newTurnScores, 0); // Copy existing scores
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable { { "turnScores", newTurnScores } });

                    yield return new WaitForSeconds(0.2f);

                    int[] testturnScores = (int[])PhotonNetwork.LocalPlayer.CustomProperties["turnScores"];

                    print("Tied" + testturnScores.Length);
                    SwapRoles();

                }
                else
                {
                    if (localPlayerObject != null)
                    {
                        PhotonNetwork.Destroy(localPlayerObject);
                    }
                    DetermineWinnerAndSendAddress();
                    yield break;  // Exit the coroutine here to avoid running the following code

                }
            }

            if (currentRound > 3)
            {
                SwapRoles();
            }

            if (currentRound >= maxRounds && currentRound <= 3)
            {
                currentRound = 0;
                SwapRoles();
            }


            Respawn();

            buttonGoalkeeper.SetActive(true);
            StartCoroutine(StartCountdown());
        }
    }

    private bool CheckIfTied()
    {
        int totalGoalsPlayer1 = 0;
        int totalGoalsPlayer2 = 0;

        foreach (var player in PhotonNetwork.PlayerList)
        {
            int[] turnScores = (int[])player.CustomProperties["turnScores"];
            if (player.ActorNumber == 1)
            {
                totalGoalsPlayer1 = turnScores.Count(score => score == 1);
            }
            else if (player.ActorNumber == 2)
            {
                totalGoalsPlayer2 = turnScores.Count(score => score == 1);
            }
        }

        return totalGoalsPlayer1 == totalGoalsPlayer2;
    }
    /*    void OnApplicationQuit()
        {

            // Send a message to React when the application quits
            Application.ExternalCall("onUnityApplicationQuit");
        }*/
    void TriggerAnimations()
    {
        if (localPlayerObject != null)
        {
            double animationStartTime = PhotonNetwork.Time + 0.3f;

            localPlayerObject.GetComponent<PlayerSetup>().GetComponent<PhotonView>().RPC("TriggerPenaltyKickAnimation", RpcTarget.AllViaServer, playerChoice, opponentChoice, animationStartTime);
        }
    }

    void UpdateScore(int result, int round)
    {
        print(round);
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
        print("run oke");
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


    // Determine winner after all rounds are complete
    private void DetermineWinnerAndSendAddress()
    {
        winnerAddress = string.Empty;
        int maxGoals = int.MinValue;
        Player winner = null;
        int[] result = new int[2];
        foreach (var player in PhotonNetwork.PlayerList)
        {
            int[] turnScores = (int[])player.CustomProperties["turnScores"];
            int playerGoals = turnScores.Count(score => score == 1);
            print(turnScores);
            if (playerGoals > maxGoals)
            {
                maxGoals = playerGoals;
                winnerAddress = (string)player.CustomProperties["userAddress"];
                winner = player;
            }
            result[player.ActorNumber] = playerGoals;
        }


        endGameScreen.SetActive(true);
        endGameScreen.GetComponent<SendDataToReact>().UpdateScore(winner, result);
        endGameScreen.GetComponent<SendDataToReact>().SendMessageToReact();

        /*        foreach (var player in PhotonNetwork.PlayerList)
                {
                    if (player.IsMasterClient)
                    {
                        print("Master Client");
                        //StartCoroutine(CheckWallet(winnerAddress));
                        endGameScreen.SetActive(true);
                    }
                }*/
    }
}

public static class PlayerRoles
{
    public const int RoleOne = 1; // Player
    public const int RoleTwo = 2; // Goalkeeper
}
