using UnityEngine;

public class DiagonalRotation : MonoBehaviour
{
    public float rotationSpeed = 50f;

    void Update()
    {
        Vector3 diagonalAxis = new Vector3(1, 1, 1).normalized;
        transform.Rotate(diagonalAxis, rotationSpeed * Time.deltaTime, Space.World);
    }
}