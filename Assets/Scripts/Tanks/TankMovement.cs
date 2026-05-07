using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Photon.Pun;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class TankMovement : MonoBehaviourPun
{
    [SerializeField] private float movSpeed = 0.25f;
    [SerializeField] private float rotSpeed = 0.25f;
    bool movementEnabled = false;

    void Update()
    {
        if (!photonView.IsMine)
            return;
        if (movementEnabled)
        {
            ProcessMovement();
        }
    }

    private void ProcessMovement()
    {
        transform.Translate(Vector3.forward * movSpeed * Time.deltaTime * Input.GetAxis("Vertical"), Space.Self);
        
        Vector3 Rotation = new Vector3(0, Input.GetAxis("Horizontal"), 0) * rotSpeed;
        Rotation *= Time.deltaTime;
        transform.Rotate(Rotation);
    }

    [PunRPC]
    public void EnableMovement()
    {
        if (photonView.IsMine)
        {
            movementEnabled = true;
        }
    }
}
