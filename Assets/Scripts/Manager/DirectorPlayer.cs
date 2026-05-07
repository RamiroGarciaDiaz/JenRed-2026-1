using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems; 

public class DirectorPlayer : MonoBehaviourPun
{
    [Header("Cámara")]
    [SerializeField] private Camera directorCamera;

    [Header("Movement")]
    [SerializeField] private float movSpeed = 20f;
    
    [Header("Team Property")]
    [SerializeField] private TeamID teamID;
    
    [Header("Director Tools")]
    [SerializeField] private LayerMask interactionLayer;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryActivateTrap();
        }
        ProcessMovement();
    }

    private void ProcessMovement()
    {
        Vector3 Movement =  new Vector3(Input.GetAxis("Horizontal"),0,Input.GetAxis("Vertical"));
        Movement = Movement.normalized;
        Movement *= movSpeed * Time.deltaTime;
        transform.Translate(Movement);
    }
    
    private void TryActivateTrap()
    {
        Ray ray = directorCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100000f, interactionLayer))
        {
            if (hit.collider.TryGetComponent(out ITrapping trappable))
            {
                trappable.OnActivateTrap();
            }
        }
    }
}