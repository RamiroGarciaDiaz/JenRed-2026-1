using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Button selectButton;
    [SerializeField] private Image highlightBorder;
    [SerializeField] private GameObject championBadge;
 
    public int ViewID { get; private set; }
 
    private Action<int> _onSelected;
 
    public void Setup(string playerName, int viewID, Action<int> onSelected) //Ante la duda, deja el Setup maxi, por si explota algo en photon. Quiza nos facilita cosas mas tarde.
    {
        ViewID = viewID;
        nameLabel.text = playerName;
        _onSelected = onSelected;
 
        highlightBorder.enabled = false;
        championBadge.SetActive(false);
 
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => _onSelected?.Invoke(ViewID));
    }
 
    public void SetHighlight(bool active)
    {
        highlightBorder.enabled = active;
    }
 
    public void SetChampionBadge(bool active)
    {
        championBadge.SetActive(active);
        selectButton.interactable = !active;
    }
}