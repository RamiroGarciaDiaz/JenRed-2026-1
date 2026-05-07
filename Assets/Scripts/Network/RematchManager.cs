using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class RematchManager : MonoBehaviourPunCallbacks
{
    public static RematchManager Instance { get; private set; }

    public static event System.Action OnRematchAccepted;
    public static event System.Action<float> OnVoteStarted;
    public static event System.Action<int, bool> OnPlayerVoted;
    public static event System.Action<bool> OnVoteEnded;

    [SerializeField] private float timeoutSeconds = 30f;
    [SerializeField] private string gameSceneName  = "GameScene";
    [SerializeField] private string mainMenuSceneName = "StartingScene";

    private PhotonView pv;
    private Dictionary<int, bool> votes = new Dictionary<int, bool>();
    private bool voteActive;
    private float timer;

    public float TimeoutSeconds => timeoutSeconds;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        pv = GetComponent<PhotonView>();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if (!PhotonNetwork.InRoom) return;
        if (PhotonNetwork.IsMasterClient)
            pv.RPC(nameof(RPC_StartVote), RpcTarget.All, timeoutSeconds);
    }

    private void Update()
    {
        if (!voteActive || !PhotonNetwork.IsMasterClient) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            voteActive = false;
            pv.RPC(nameof(RPC_EndVote), RpcTarget.All, false);
        }
    }


    [PunRPC]
    private void RPC_StartVote(float timeout)
    {
        votes.Clear();
        voteActive = true;
        timer = timeout;
        OnVoteStarted?.Invoke(timeout);
    }

    public void SubmitVote(bool accept)
    {
        if (!voteActive) return;
        pv.RPC(nameof(RPC_ReceiveVote), RpcTarget.MasterClient,
               PhotonNetwork.LocalPlayer.ActorNumber, accept);
    }

    [PunRPC]
    private void RPC_ReceiveVote(int actorNr, bool accepted)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (!voteActive) return;

        votes[actorNr] = accepted;

        pv.RPC(nameof(RPC_BroadcastVote), RpcTarget.All, actorNr, accepted);

        if (!accepted)
        {
            voteActive = false;
            pv.RPC(nameof(RPC_EndVote), RpcTarget.All, false);
            return;
        }

        int totalPlayers = PhotonNetwork.CurrentRoom.PlayerCount;
        if (votes.Count >= totalPlayers)
        {
            bool allAccepted = true;
            foreach (bool v in votes.Values)
                if (!v) { allAccepted = false; break; }

            if (allAccepted)
            {
                voteActive = false;
                pv.RPC(nameof(RPC_EndVote), RpcTarget.All, true);
            }
        }
    }

    [PunRPC]
    private void RPC_BroadcastVote(int actorNr, bool accepted)
    {
        OnPlayerVoted?.Invoke(actorNr, accepted);
    }

    [PunRPC]
    private void RPC_EndVote(bool rematch)
    {
        voteActive = false;
        OnVoteEnded?.Invoke(rematch);
        StartCoroutine(HandleResult(rematch));
    }

    private IEnumerator HandleResult(bool rematch)
    {
        yield return new WaitForSeconds(1.5f);

        if (rematch)
        {
            OnRematchAccepted?.Invoke();
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(gameSceneName);
        }
        else
        {
            PhotonNetwork.LeaveRoom();
        }
    }


    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (voteActive && PhotonNetwork.IsMasterClient)
        {
            voteActive = false;
            pv.RPC(nameof(RPC_EndVote), RpcTarget.All, false);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (voteActive && PhotonNetwork.IsMasterClient)
        {
            voteActive = false;
            pv.RPC(nameof(RPC_EndVote), RpcTarget.All, false);
        }
    }
}
