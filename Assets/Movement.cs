using UnityEngine;

public class Movement : MonoBehaviour
{
    public GameObject capsule; // Assign your capsule prefab in the inspector
    public float shootPower = 0.001f; // Adjust the shooting power as needed

    private Vector3 dragStartPos;

    void Update()
    {
        // Detect when the left mouse button is pressed
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = Input.mousePosition;
            print(dragStartPos);
        }

        // Detect when the left mouse button is released
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 dragEndPos = Input.mousePosition; // Record end position
            print(dragEndPos);
            ShootCapsule(dragStartPos, dragEndPos);
        }
    }

    void ShootCapsule(Vector3 start, Vector3 end)
    {
        Vector3 dragVector = start - end; // Calculate drag vector (reversed)

        print(dragVector);

        // Convert dragVector to a 3D vector considering only horizontal (x) and forward (z) directions
        Vector3 shootDirection = new Vector3(-dragVector.y, 0, dragVector.x).normalized;

        // Apply force to the capsule in the direction of the drag
        capsule.GetComponent<Rigidbody>().AddForce(shootDirection * shootPower * dragVector.magnitude, ForceMode.Acceleration);
    }
}
