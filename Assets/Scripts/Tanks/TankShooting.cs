using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TankShooting : MonoBehaviourPun
{
    [SerializeField] private GameObject ProyectilePrefab;
    [SerializeField] private GameObject outPoint;
    private int viewID;
    private float shootCooldown = 2f;
    private float shootTimer = 0;

    private void Start()
    {
        viewID = gameObject.GetComponent<PhotonView>().ViewID;
    }
    void Update()
    {
        if (!photonView.IsMine)
            return;
        shootTimer += Time.deltaTime;
        ProcessShooting();
    }

    private void ProcessShooting()
    {
        if (shootTimer > shootCooldown)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                shootTimer = 0;
                var bullet = PhotonNetwork.Instantiate("Prefabs/" + ProyectilePrefab.name, outPoint.transform.position, transform.rotation);
                bullet.GetComponent<BulletProyectile>().SetOwner(viewID);
            }
        }
    }

    public void ShootCooldownChange(float cooldown)
    {
        shootCooldown = cooldown;
    }
}
