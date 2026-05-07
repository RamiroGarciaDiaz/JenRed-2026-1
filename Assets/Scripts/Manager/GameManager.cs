using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Collections;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Spawner spawner;

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
        
    }
    
    private void UpdateWaitingText()
    {
        
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateWaitingText();
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient) return;
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
    
    public void NotifyPlayerDied(string playerName, bool wasChampion)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        Invoke(nameof(CheckWinCondition), 0.5f); 
    }

    private void CheckWinCondition()
    {
        
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