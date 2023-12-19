using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    CharacterController controller;

    Vector3 moveDir;
    public float moveSpeed;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector2 moveDirInput = new(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        moveDir = (transform.forward * moveDirInput.y + transform.right * moveDirInput.x) * moveSpeed;
        //ApplyGravity();
        
    }

    void ApplyGravity()
    {
        moveDir.y += Physics.gravity.y * Time.deltaTime;
        if (controller.isGrounded && moveDir.y < 0)
            moveDir.y = 0;

    }

    private void FixedUpdate()
    {
        controller.Move(moveDir);
    }
}
