using UnityEngine;

public class BouncingWall : MonoBehaviour
{

    public float bounceForce = 10f;
    Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            Vector3 direction = collision.contacts[0].point - transform.position;
            direction = direction.normalized;
            rb.AddForce(direction * bounceForce, ForceMode.Impulse);
            anim.Play("Bounce");
        }
    }

    public void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            anim.Play("Idle");
        }
    }

}
