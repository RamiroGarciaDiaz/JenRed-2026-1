using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DirectorItem : MonoBehaviourPun
{
    public enum ItemEffect { Heal, Damage, Shield } 

    [Header("Item Design")]
    [SerializeField] private ItemEffect effectType;
    [SerializeField] private float effectValue = 30f;

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (other.TryGetComponent(out PhotonView targetView) && other.TryGetComponent(out TankModel tank))
        {
            ApplyEffect(targetView);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void ApplyEffect(PhotonView targetView)
    {
        switch (effectType)
        {
            case ItemEffect.Heal:
                targetView.RPC("OnHeal", RpcTarget.AllBuffered, effectValue);
                break;
            case ItemEffect.Damage:
                targetView.RPC("OnDamage", RpcTarget.AllBuffered, effectValue);
                break;
            case ItemEffect.Shield: 
                targetView.RPC("OnShieldGained", RpcTarget.AllBuffered, effectValue);
                break;
        }
    }
}