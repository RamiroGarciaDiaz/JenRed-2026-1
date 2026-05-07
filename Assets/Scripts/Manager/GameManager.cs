using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public int ChampionViewID { get; private set; }

    [Header("References")]
    [SerializeField] private Spawner spawner;
    [SerializeField] private DirectorManager directorManager;

    [Header("UI General (Feedback)")]
    [SerializeField] private TextMeshProUGUI directorAnnouncementText;
    [SerializeField] private TextMeshProUGUI feedbackEventText;
    
    [Header("Match Settings")]
    [SerializeField] private int minPlayersToStart = 3; 
    
    [Header("Scenes")]
    [SerializeField] private string menuLevel = "Starting Scene";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateDirectorAnnouncement();
        
        UpdateWaitingText();

        if (PhotonNetwork.IsMasterClient)
        {
            directorManager.Activate();
        }
        else
        {
            spawner.SpawnPlayer();
        }
    }
    
    private void UpdateWaitingText()
    {
        if (ChampionViewID != 0) return;

        if (feedbackEventText != null) 
        {
            int currentPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
            int currentTanks = currentPlayers - 1; 

            if (currentTanks < minPlayersToStart)
            {
                feedbackEventText.text = $"Waiting for Players... ({currentTanks} / {minPlayersToStart})";
            }
            else
            {
                feedbackEventText.text = "Players Ready! Director is choosing a Champion...";
            }
        }
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateWaitingText();
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
    
        if (ChampionViewID != 0 && (ChampionViewID / 1000) == otherPlayer.ActorNumber)
        {
            photonView.RPC(nameof(RPC_AnnounceEvent), RpcTarget.All, $"¡Champion {otherPlayer.NickName} fled! PEASANTS WIN.");
        }
        else
        {
            UpdateWaitingText();
            Invoke(nameof(CheckWinCondition), 0.5f);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        
        UpdateDirectorAnnouncement();
        
        photonView.RPC(nameof(RPC_AnnounceEvent), RpcTarget.All, $"¡{newMasterClient.NickName} assumed as the new Director!");

        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            TransformIntoDirector();
        }
    }

    private void TransformIntoDirector()
    {
        bool wasChampion = false;
        PhotonView[] views = FindObjectsOfType<PhotonView>();
        
        foreach (PhotonView view in views)
        {
            if (view.IsMine && view.TryGetComponent<TankModel>(out _))
            {
                if (view.ViewID == ChampionViewID) wasChampion = true;
                
                PhotonNetwork.Destroy(view.gameObject);
                break; 
            }
        }
        
        directorManager.Activate();
        
        if (wasChampion)
        {
            photonView.RPC(nameof(RPC_AnnounceEvent), RpcTarget.All, "¡Champion ascended to Director! Match Aborted (Draw).");
        }
        else
        {
            Invoke(nameof(CheckWinCondition), 0.5f);
        }
    }

    private void UpdateDirectorAnnouncement()
    {
        if (directorAnnouncementText != null && PhotonNetwork.MasterClient != null)
        {
            directorAnnouncementText.text = $"Director: {PhotonNetwork.MasterClient.NickName}";
        }
    }

    public void RequestSpawnItemAtPosition(Vector3 position, bool isPowerUp)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        spawner.SpawnDirectorItem(position, isPowerUp);
    }
    
    public void AssignChampion(int viewID)
    {
        ChampionViewID = viewID;
        photonView.RPC(nameof(RPC_SyncChampion), RpcTarget.All, viewID);
    }

    [PunRPC]
    private void RPC_SyncChampion(int viewID)
    {
        ChampionViewID = viewID;
    
        if (PhotonNetwork.IsMasterClient && directorManager != null) 
        {
            directorManager.OnChampionSet(viewID);
        }
    
        PhotonView targetView = PhotonNetwork.GetPhotonView(viewID);
        if (targetView != null && targetView.TryGetComponent(out TankModel tank))
        {
            tank.SetAsChampion();
            if (feedbackEventText != null)
            {
                feedbackEventText.text = $"¡{targetView.Owner.NickName} selected as Champion!";
                StartCoroutine(ClearFeedbackText(3f));
            }
        }
    }
    
    public void NotifyPlayerDied(string playerName, bool wasChampion)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (wasChampion)
        {
            photonView.RPC(nameof(RPC_AnnounceEvent), RpcTarget.All, $"¡Champion {playerName} has fallen!");
        }
        else
        {
            photonView.RPC(nameof(RPC_AnnounceEvent), RpcTarget.All, $"¡{playerName} destroyed!");
        }
        Invoke(nameof(CheckWinCondition), 0.5f); 
    }

    private void CheckWinCondition()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        TankModel[] remainingTanks = FindObjectsOfType<TankModel>();
        
        if (remainingTanks.Length == 0) return;

        if (remainingTanks.Length == 1)
        {
            TankModel lastTank = remainingTanks[0];

            if (lastTank.photonView.ViewID == ChampionViewID)
            {
                photonView.RPC(nameof(RPC_AnnounceEvent), RpcTarget.All, $"¡VICTORY! Champion {lastTank.photonView.Owner.NickName} and Director WON!");
            }
            else
            {
                photonView.RPC(nameof(RPC_AnnounceEvent), RpcTarget.All, $"¡PEASANTS WIN! {lastTank.photonView.Owner.NickName} Survived.");
            }
            StartCoroutine(CloseRoomAfterDelay(4f));
        }
    }
    
    private IEnumerator CloseRoomAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
        photonView.RPC(nameof(RPC_ForceDisconnect), RpcTarget.All);
    }
    
    [PunRPC]
    private void RPC_ForceDisconnect()
    {
        Log.Info("[GameManager] Match ended. Leaving room...");
        PhotonNetwork.LeaveRoom();
    }
    
    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(menuLevel);
    }
    
    [PunRPC]
    private void RPC_AnnounceEvent(string message)
    {
        if (feedbackEventText != null)
        {
            feedbackEventText.text = message;
            StopAllCoroutines();
            StartCoroutine(ClearFeedbackText(4f));
        }
    }

    private IEnumerator ClearFeedbackText(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (feedbackEventText != null) feedbackEventText.text = "";
    }
}