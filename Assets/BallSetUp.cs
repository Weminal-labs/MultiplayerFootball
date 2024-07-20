using Photon.Pun;
using UnityEngine;

public class BallSetUp : MonoBehaviour
{


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BlueGoal"))
        {
            Debug.Log("Blue Team Scores!");

            PhotonNetwork.Destroy(gameObject);
        }
        if (other.CompareTag("RedGoal"))
        {

            Debug.Log("Red Team Scores!");

            PhotonNetwork.Destroy(gameObject);
        }

    }



}
