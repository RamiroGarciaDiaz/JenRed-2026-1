using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TankModel : MonoBehaviourPun, IDamage
{
    [Header("Life")]
    private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private Image hpUI;

    [Header("Shield")]
    private float maxShield = 100f;
    [SerializeField] private float currentShield = 0f;
    [SerializeField] private Image shieldUI;
    
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI nameUIText;
    
    public bool HasShield => currentShield > 0;

    private void Start()
    {
        currentHealth = maxHealth;
        currentShield = 0f; 
        SetNameUIText();
        
        if (shieldUI != null) 
        {
            shieldUI.fillAmount = 0f;
            shieldUI.gameObject.SetActive(false); 
        }

        GetComponentInChildren<Renderer>().material.color = photonView.IsMine ? Color.blue : Color.red;

        if (PhotonNetwork.IsMasterClient && DirectorManager.Instance != null)
        {
            DirectorManager.Instance.RefreshPlayerList();
        }
    }

    private void SetNameUIText()
    {
        nameUIText.text = photonView.Owner.NickName;
    }

    [PunRPC]
    public void OnDamage(float damage)
    {
        if (currentShield > 0)
        {
            if (damage <= currentShield)
            {
                currentShield -= damage;
                damage = 0; 
            }
            else
            {
                damage -= currentShield; 
                currentShield = 0;
            }
            
            if (shieldUI != null) 
            {
                shieldUI.fillAmount = currentShield / maxShield;
                if (currentShield <= 0)
                {
                    shieldUI.gameObject.SetActive(false);
                }
            }
        }
        
        if (damage > 0)
        {
            currentHealth -= damage;
            if (hpUI != null) hpUI.fillAmount = currentHealth / maxHealth;
        }

        if (currentHealth <= 0)
        {
            HandleDeath();
        }
    }

    [PunRPC]
    public void OnHeal(float healAmount)
    {
        currentHealth += healAmount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        if (hpUI != null) hpUI.fillAmount = currentHealth / maxHealth;
    }
    
    [PunRPC]
    public void OnShieldGained(float shieldAmount)
    {
        currentShield += shieldAmount;
        if (currentShield > maxShield) currentShield = maxShield; 
        
        if (shieldUI != null) 
        {
            shieldUI.gameObject.SetActive(true); 
            shieldUI.fillAmount = currentShield / maxShield;
        }
    }

    private void HandleDeath()
    {
        bool isChampion = GameManager.Instance != null && GameManager.Instance.ChampionViewID == photonView.ViewID;
        
        if (PhotonNetwork.IsMasterClient && currentHealth <= 0) 
        {
            GameManager.Instance.NotifyPlayerDied(photonView.Owner.NickName, isChampion);
        }

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        if (PhotonNetwork.IsMasterClient && DirectorManager.Instance != null)
        {
            DirectorManager.Instance.RefreshPlayerList();
        }
    }

    public void SetAsChampion()
    {
        transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            r.material.color = Color.yellow; 
        }
    }
}