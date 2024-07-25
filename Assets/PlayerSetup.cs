using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerSetup : MonoBehaviourPun
{
    public GameObject playerTag;
    public string nickname;
    public TextMeshPro nicknameText;
    private int role;
    public Animator animator;
    private PhotonAnimatorView photonAnimatorView;
    public GameObject ball;

    void Start()
    {
        animator = GetComponent<Animator>();
        photonAnimatorView = GetComponent<PhotonAnimatorView>();

        photonAnimatorView.enabled = photonView.IsMine;
        ball.SetActive(false);
    }

    public void isLocalPlayer()
    {
        playerTag.SetActive(false);
        Debug.Log(GetRole());
    }

    public void isWin()
    {
        Debug.Log("Ball catch");
        ball.SetActive(true);
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
        Debug.Log("Role set to: " + role);
        PlayIdleAnimation();
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
    public void TriggerPenaltyKickAnimation(int chooseNumber, double startTime)
    {
        StartCoroutine(SyncAnimation(chooseNumber, startTime));
    }

    private IEnumerator SyncAnimation(int chooseNumber, double startTime)
    {
        double timeToWait = startTime - PhotonNetwork.Time;
        if (timeToWait > 0)
        {
            yield return new WaitForSeconds((float)timeToWait);
        }

        Debug.Log("animation event" + chooseNumber);
        if (animator != null)
        {
            animator.SetTrigger("PenaltyKick");
            animator.SetInteger("chooseNumber", chooseNumber);
        }
    }
}
