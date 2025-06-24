using UnityEngine;

public class PlayerManualControl : MonoBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal"); 
        float moveZ = Input.GetAxisRaw("Vertical");   

        Vector3 move = new Vector3(moveX, 0, moveZ).normalized;

        transform.position += move * moveSpeed * Time.deltaTime;
    }
}
