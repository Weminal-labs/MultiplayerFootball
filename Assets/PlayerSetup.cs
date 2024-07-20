using Photon.Pun;
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

    void Start()
    {
        animator = GetComponent<Animator>();
        photonAnimatorView = GetComponent<PhotonAnimatorView>();

        if (photonView.IsMine)
        {
            photonAnimatorView.enabled = true;
        }
        else
        {
            photonAnimatorView.enabled = false;
        }
    }

    public void isLocalPlayer()
    {
        playerTag.SetActive(false);
        print(GetRole());
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
        Debug.Log("Role set to: " + role); // Debug log to check the role
        // Play the appropriate animation based on the role
        PlayIdleAnimation();
    }

    private void PlayIdleAnimation()
    {
        if (animator != null)
        {
            if (role == PlayerRoles.RoleOne)
            {
                animator.SetBool("isGoal", false); // Assuming PlayerIdle is default
            }
            else if (role == PlayerRoles.RoleTwo)
            {
                animator.SetBool("isGoal", true);
            }
            else
            {
                Debug.LogError("Invalid role: " + role); // Debug log for invalid role
            }
        }
        else
        {
            Debug.LogError("Animator not found!"); // Debug log for missing animator
        }
    }

    public int GetRole()
    {
        return role;
    }
}
