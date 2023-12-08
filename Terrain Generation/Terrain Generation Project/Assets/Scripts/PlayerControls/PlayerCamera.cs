using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;
    public float rotationSpeed;

    Vector2 lookRotation;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouseInput = new(-Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"));
        lookRotation += rotationSpeed * Time.unscaledDeltaTime * mouseInput;
        Quaternion look = Quaternion.Euler(lookRotation);

        transform.SetPositionAndRotation(player.position + offset, look);
        player.forward = transform.forward;
    }
}
