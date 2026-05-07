using Photon.Pun;
using UnityEngine;

public class BulletProyectile : MonoBehaviourPun
{
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifeTime = 20f; 
    [SerializeField] private float bulletDamage = 30f;
    [SerializeField] private int maxBounces = 2; 
    
    private int currentBounces = 0;
    private int ownerID;

    private void Start()
    {
        GetComponentInChildren<Renderer>().material.color = Color.black;
    }

    void Update()
    {
        float step = bulletSpeed * Time.deltaTime;
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, step, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            bool shouldBounce = false;
            
            if (!hit.collider.isTrigger && !hit.collider.TryGetComponent(out IDamage _))
            {
                shouldBounce = true;
            }
            else if (hit.collider.TryGetComponent(out TankModel tank) && hit.collider.TryGetComponent(out PhotonView view))
            {
                if (view.ViewID != ownerID && tank.HasShield)
                {
                    shouldBounce = true;
                    view.RPC("OnDamage", RpcTarget.AllBuffered, bulletDamage);
                }
            }
            if (shouldBounce)
            {
                if (currentBounces < maxBounces)
                {
                    transform.forward = Vector3.Reflect(transform.forward, hit.normal);
                    transform.position = hit.point; 
                    currentBounces++;
                }
                else
                {
                    DestroyBullet();
                }
                return; 
            }
        }
        transform.position += transform.forward * step;
        
        bulletLifeTime -= Time.deltaTime;
        if (bulletLifeTime <= 0)
        {
            DestroyBullet();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IDamage damageInterface) && other.gameObject.TryGetComponent(out PhotonView view))
        {
            if (view.ViewID != ownerID && ownerID != 0)
            {
                if (other.gameObject.TryGetComponent(out TankModel tank) && tank.HasShield)
                {
                    return; 
                }
                view.RPC("OnDamage", RpcTarget.AllBuffered, bulletDamage);
                DestroyBullet();
            }
        }
    }

    public void SetOwner(int id) { ownerID = id; }

    private void DestroyBullet()
    {
        if (photonView.IsMine) PhotonNetwork.Destroy(gameObject);
    }
}