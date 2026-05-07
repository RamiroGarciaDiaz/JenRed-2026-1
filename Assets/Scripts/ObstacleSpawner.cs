using Photon.Pun;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] spawnPrefabs;
    void Start()
    {
        PhotonManager.Instance.OnRoom += SpawnPrefab;
    }

    void SpawnPrefab()
    {
        foreach (GameObject prefab in spawnPrefabs)
        {
            PhotonNetwork.Instantiate(prefab.name, transform.position, transform.rotation); 
        }
    }
}
