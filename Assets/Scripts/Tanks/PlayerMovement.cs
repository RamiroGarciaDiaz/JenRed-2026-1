using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerMovement : MonoBehaviourPun
{
    [SerializeField] private float movSpeed = 20f;
    [SerializeField] private float rotSpeed = 20f;
    
    void Update()
    {
        if (!photonView.IsMine)
            return;
        ProcessMovement();
    }

    private void ProcessMovement()
    {
        Vector3 Movement =  new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Movement = Movement.normalized;
        Movement *= movSpeed;
        transform.Translate(Movement);
        
        Vector3 Rotation = new Vector3(Input.GetAxis("Mouse X"), 0, Input.GetAxis("Mouse Y"));
        Rotation *= rotSpeed;
        Rotation.x += Input.GetAxis("Mouse Y") * rotSpeed;
        Rotation.y += Input.GetAxis("Mouse X") * rotSpeed;
    }
}
