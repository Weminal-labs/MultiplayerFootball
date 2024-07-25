using UnityEngine;

public class BallSetUp : MonoBehaviour
{
    Animator animatior;

    private void Start()
    {
        animatior = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.CompareTag("BlueGoal"))
        {
            Debug.Log("Blue Team Scores!");

            PhotonNetwork.Destroy(gameObject);
        }
        if (other.CompareTag("RedGoal"))
        {

            Debug.Log("Red Team Scores!");

            PhotonNetwork.Destroy(gameObject);
        }*/
        Debug.Log("Blue Team Scores!");


    }



}
