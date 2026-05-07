using System;
using System.Collections;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class SpikeTrap : MonoBehaviour, ITrapping
{
    [Header("Trap Settings")]
    [SerializeField] private bool isActive;
    [SerializeField] private float distanceMultiplier;
    [SerializeField] private float speed;
    [SerializeField] private GameObject trap;
    
    public void OnActivateTrap()
    {
        isActive = true;
        StartCoroutine(StartTrap());
    }

    private IEnumerator StartTrap()
    {
        Vector3 startPos = trap.transform.position;
        Vector3 destination = startPos + (Vector3.up * distanceMultiplier);
        float elapsedTime = 0f;
        float duration = 1f / speed;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            trap.transform.position = Vector3.Lerp(startPos, destination, t);
            yield return null;
        }
        trap.transform.position = destination;
        
        elapsedTime = 0f;
        destination = startPos;
        startPos = trap.transform.position;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            trap.transform.position = Vector3.Lerp(startPos, destination, t);
            yield return null;
        }
    }
    
    public void OnDeactivateTrap()
    {
        
    }

    public void OnResetTrap()
    {
        
    }
}
