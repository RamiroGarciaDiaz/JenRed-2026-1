using Photon.Pun;
using UnityEngine;

public class CrateModel : MonoBehaviourPun, IDamage
{
    private float maxHealth = 60f;
    [SerializeField] float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
        GetComponentInChildren<Renderer>().material.color = Color.grey;
    }

    [PunRPC]
    public void OnDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            if (photonView.IsMine)
            {
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
