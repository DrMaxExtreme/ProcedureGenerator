using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Transform cameraTransform;
    public float smoothRotationSpeed = 5f;
    public float cameraFollowSpeed = 5f;

    private Vector3 cameraOffset;

    private void Start()
    {
        if (cameraTransform != null)
        {
            // Calculate the initial camera offset from the player
            cameraOffset = cameraTransform.position - transform.position;
        }
    }

    private void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0f, moveVertical);

        movement.Normalize();

        transform.position += movement * moveSpeed * Time.deltaTime;

        SmoothFollowPlayer();
    }

    private void SmoothFollowPlayer()
    {
        if (cameraTransform != null)
        {
            // Calculate the target position for the camera based on the player's position and offset
            Vector3 targetCameraPosition = transform.position + cameraOffset;

            // Calculate the direction from the camera position to the player position
            Vector3 cameraToPlayerDirection = transform.position - cameraTransform.position;

            // Calculate the target rotation for the camera to look directly at the player
            Quaternion targetCameraRotation = Quaternion.LookRotation(cameraToPlayerDirection, Vector3.up);

            // Move the camera towards the target position smoothly with the specified follow speed
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetCameraPosition, cameraFollowSpeed * Time.deltaTime);

            // Rotate the camera towards the target rotation smoothly
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetCameraRotation, smoothRotationSpeed * Time.deltaTime);
        }
    }
}
