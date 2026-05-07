using TMPro;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class RematchUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Botones")]
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button declineButton;

    [Header("Textos")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Estado de votos (4 labels, uno por player)")]
    [SerializeField] private TextMeshProUGUI[] playerVoteLabels = new TextMeshProUGUI[4];

    private float timeRemaining;
    private bool counting;
    private bool voted;

    private void OnEnable()
    {
        RematchManager.OnVoteStarted  += HandleVoteStarted;
        RematchManager.OnPlayerVoted  += HandlePlayerVoted;
        RematchManager.OnVoteEnded    += HandleVoteEnded;
    }

    private void OnDisable()
    {
        RematchManager.OnVoteStarted  -= HandleVoteStarted;
        RematchManager.OnPlayerVoted  -= HandlePlayerVoted;
        RematchManager.OnVoteEnded    -= HandleVoteEnded;
    }

    private void Start()
    {
        panel.SetActive(false);
        acceptButton.onClick.AddListener(() => Vote(true));
        declineButton.onClick.AddListener(() => Vote(false));
    }

    private void Update()
    {
        if (!counting) return;
        timeRemaining -= Time.deltaTime;
        timeRemaining = Mathf.Max(0f, timeRemaining);
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
    }


    private void HandleVoteStarted(float timeout)
    {
        timeRemaining = timeout;
        counting      = true;
        voted         = false;

        panel.SetActive(true);
        SetButtonsInteractable(true);

        if (statusText != null) statusText.text = "Rematch?";

        var players = PhotonNetwork.PlayerList;
        for (int i = 0; i < playerVoteLabels.Length; i++)
        {
            if (playerVoteLabels[i] == null) continue;
            if (i < players.Length)
            {
                playerVoteLabels[i].gameObject.SetActive(true);
                playerVoteLabels[i].text  = players[i].NickName + ": ?";
                playerVoteLabels[i].color = Color.white;
            }
            else
            {
                playerVoteLabels[i].gameObject.SetActive(false);
            }
        }
    }

    private void HandlePlayerVoted(int actorNr, bool accepted)
    {
        var players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length && i < playerVoteLabels.Length; i++)
        {
            if (players[i].ActorNumber != actorNr) continue;
            if (playerVoteLabels[i] == null) continue;

            playerVoteLabels[i].text  = players[i].NickName + (accepted ? ": ✓" : ": ✗");
            playerVoteLabels[i].color = accepted ? Color.green : Color.red;
            break;
        }

        if (!accepted && statusText != null)
            statusText.text = "A player declined. Returning to menu...";
    }

    private void HandleVoteEnded(bool rematch)
    {
        counting = false;
        SetButtonsInteractable(false);

        if (statusText != null)
            statusText.text = rematch ? "Rematch! Loading..." : "No rematch. See you!";
    }


    private void Vote(bool accept)
    {
        if (voted) return;
        voted = true;
        SetButtonsInteractable(false);

        if (statusText != null)
            statusText.text = accept ? "Voted: Rematch! Waiting..." : "Voted: No rematch.";

        RematchManager.Instance.SubmitVote(accept);
    }

    private void SetButtonsInteractable(bool state)
    {
        if (acceptButton  != null) acceptButton.interactable  = state;
        if (declineButton != null) declineButton.interactable = state;
    }
}
