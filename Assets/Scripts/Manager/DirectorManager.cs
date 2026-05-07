using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems; 

public class DirectorManager : MonoBehaviour
{
    public static DirectorManager Instance { get; private set; }

    [Header("Cámara")]
    [SerializeField] private Camera directorCamera;
    
    [Header("UI Director")]
    [SerializeField] private GameObject directorUIRoot;
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private TextMeshProUGUI dropsCountText;
    
    [Header("Director Tools")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int maxDrops = 5;
    [SerializeField] private float spawnCooldown = 2.5f;

    private float _nextSpawnTime = 0f;
    private int _currentDrops;
    private int _selectedChampionViewID = -1;
    private readonly List<PlayerEntry> _entries = new();
    private int minPlayers = 3;
    private bool readyToStart = false;

    public bool IsMatchReady => readyToStart;

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient || !directorUIRoot.activeSelf) return;
        
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; 
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            TrySpawnItemAtMouse(true);
        }
        
        if (Input.GetMouseButtonDown(1))
        {
            TrySpawnItemAtMouse(false);
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        
        directorUIRoot.SetActive(false);
    }

    public void Activate()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        directorUIRoot.SetActive(true);
        
        if (GameManager.Instance != null && GameManager.Instance.ChampionViewID != 0)
        {
            if (selectionPanel != null) selectionPanel.SetActive(false);
        }
        else
        {
            if (selectionPanel != null) selectionPanel.SetActive(true);
        }

        _currentDrops = maxDrops;
        _nextSpawnTime = 0f;
        UpdateDropsUI();

        Log.Info("Director Active");
        RefreshPlayerList();
    }
    
    private void UpdateDropsUI()
    {
        if (dropsCountText != null)
        {   
            dropsCountText.text = $"Remaining Drops: <color=yellow>{_currentDrops}</color> / {maxDrops}";
        }
    }
    
    public void RefreshPlayerList()
    {
        foreach (var e in _entries) Destroy(e.gameObject);
        _entries.Clear();

        readyToStart = false;

        foreach (var pv in FindObjectsOfType<PhotonView>())
        {
            if (!pv.gameObject.TryGetComponent<TankModel>(out _)) continue;

            var go = Instantiate(playerEntryPrefab, playerListContainer);
            var entry = go.GetComponent<PlayerEntry>();
            entry.Setup(pv.Owner?.NickName ?? "Player", pv.ViewID, OnPlayerSelected);
            _entries.Add(entry);
        }

        if (_entries.Count >= minPlayers)
        {
            readyToStart = true;
        }
    }
    
    private void OnPlayerSelected(int viewID)
    {
        _selectedChampionViewID = viewID;
        Log.Info("Director has decided Champion:"+ viewID + "is new Champion");
        foreach (var e in _entries)
            e.SetHighlight(e.ViewID == viewID);
    }

    public void ConfirmChampion()
    {
        if (_selectedChampionViewID < 0)
        {
            Debug.LogWarning("Champion Not Selected");
            return;
        }
        
        GameManager.Instance.AssignChampion(_selectedChampionViewID);
        
        if (selectionPanel != null) selectionPanel.SetActive(false);
        
        foreach (var pv in FindObjectsOfType<PhotonView>())
        {
            if (!pv.gameObject.TryGetComponent<TankModel>(out _)) continue;

            pv.RPC("EnableMovement", RpcTarget.All);
        }
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }
    
    public void OnChampionSet(int viewID)
    {
        foreach (var e in _entries)
            e.SetChampionBadge(e.ViewID == viewID);
    }

    public void OnChampionDisconnected()
    {
        Log.Warning("Champion Disconnected");
        RefreshPlayerList();
    }

    private void TrySpawnItemAtMouse(bool isPowerUp)
    {
        if (_currentDrops <= 0)
        {
            return; 
        }
        
        if (Time.time < _nextSpawnTime)
        {
            return;
        }

        Ray ray = directorCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundLayer))
        {
            GameManager.Instance.RequestSpawnItemAtPosition(hit.point, isPowerUp);
            
            _currentDrops--;
            _nextSpawnTime = Time.time + spawnCooldown; 
            
            UpdateDropsUI();
        }
    }
}