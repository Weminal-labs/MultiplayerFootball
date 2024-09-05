using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPun
{
    public GameObject playerTag;
    public string nickname;
    public TextMeshPro nicknameText;
    public int role;
    public Animator animator;
    private PhotonAnimatorView photonAnimatorView;
    public GameObject ball;

    public GameObject[] skin;

    public GameObject camera;

    public GameObject ui;



    void Start()
    {
        animator = GetComponent<Animator>();
        photonAnimatorView = GetComponent<PhotonAnimatorView>();

        photonAnimatorView.enabled = photonView.IsMine;
        ball.SetActive(false);

        if (photonView.IsMine)
        {
            PhotonNetwork.LocalPlayer.TagObject = this;
        }

    }


    public void isLocalPlayer()
    {
        playerTag.SetActive(false);
        //camera.SetActive(true);
        //ui.SetActive(true);
    }

    [PunRPC]
    public void SetName(string _name)
    {
        nickname = _name;
        nicknameText.text = nickname;
    }



    [PunRPC]
    public void SetRole(int _role)
    {
        role = _role;
        PlayIdleAnimation();
    }

    [PunRPC]
    public void SetSkin(int _skin)
    {
        skin[_skin].SetActive(true);
    }

    private void PlayIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isGoal", role == PlayerRoles.RoleTwo);
        }
        else
        {
            Debug.LogError("Animator not found!");
        }
    }

    public int GetRole()
    {
        return role;
    }


    [PunRPC]
    public void TriggerPenaltyKickAnimation(int chooseNumber, int opponentChoice, double startTime)
    {
        StartCoroutine(SyncAnimation(chooseNumber, opponentChoice, startTime));
    }

    private IEnumerator SyncAnimation(int chooseNumber, int opponentChoice, double startTime)
    {
        double timeToWait = startTime - PhotonNetwork.Time;

        // Add a small buffer to account for network delays
        timeToWait = Mathf.Max(0, (float)timeToWait);

        // Wait for the calculated time before starting the animation
        yield return new WaitForSeconds((float)timeToWait);

        if (animator != null)
        {
            animator.SetTrigger("PenaltyKick");
            animator.SetInteger("chooseNumber", chooseNumber);
            animator.SetInteger("enemyNumber", opponentChoice);
        }
    }
}
