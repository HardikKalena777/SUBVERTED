using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public float moveSpeed = 10f; // Speed of camera movement
    public float rotationSpeed = 100f; // Speed of camera rotation

    void Update()
    {
        // Handle movement
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        float vertical = Input.GetAxis("Vertical"); // W/S or Up/Down Arrow
        Vector3 movement = new Vector3(horizontal, 0, vertical);
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.Self);

        // Handle rotation
        if (Input.GetMouseButton(1)) // Right mouse button held
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            // Rotate the camera based on mouse movement
            transform.Rotate(Vector3.up, mouseX * rotationSpeed * Time.deltaTime, Space.World);
            transform.Rotate(Vector3.right, -mouseY * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
