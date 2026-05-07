using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Spawner : MonoBehaviourPun
{
    [Header("Spawn Points")]
    [SerializeField] private GameObject spawnPrefab;
    [SerializeField] private GameObject cratePrefab;
    [SerializeField] private GameObject[] spawnPointsPrefabs;
    
    [Header("Director Items")]
    [SerializeField] private GameObject powerUpPrefab;
    [SerializeField] private GameObject powerDownPrefab;
    
    private int spawnIndex;

    public void SpawnDirectorItem(Vector3 position, bool isPowerUp)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        GameObject prefabToSpawn = isPowerUp ? powerUpPrefab : powerDownPrefab;
        if (prefabToSpawn == null) return;
        position.y = 1f; //Just in Case...
        PhotonNetwork.InstantiateRoomObject("Prefabs/" + prefabToSpawn.name, position, Quaternion.identity);
    }

    public void SpawnPlayer()
    {
        if (spawnPrefab == null) return;
        
        var player = PhotonNetwork.Instantiate("Prefabs/" + spawnPrefab.name, new Vector3(0, -5000, 0), Quaternion.identity);
        
        photonView.RPC(nameof(DefineSpawnPoint), RpcTarget.AllBuffered);
        StartCoroutine(AWaitBeforeSpawn(player));
    }

    private IEnumerator AWaitBeforeSpawn(GameObject player)
    {
        yield return new WaitForSeconds(1f);
        SetLocation(player);
        SetRotation(player);
    }
    
    [PunRPC]
    private void DefineSpawnPoint()
    {
        spawnIndex = (spawnIndex + 1) % spawnPointsPrefabs.Length;
    }
    
    private void SetLocation(GameObject player)
    {
        if (spawnPointsPrefabs.Length > 0)
            player.transform.position = spawnPointsPrefabs[spawnIndex].transform.position;
    }
    
    private void SetRotation(GameObject player)
    {
        player.transform.rotation = Quaternion.identity;
    }
    
    public void SpawnCrate()
    {
        if (!PhotonNetwork.IsMasterClient) return; 
        
        Vector3 randomPos = new Vector3(Random.Range(-5.5f, 6.5f), 1, Random.Range(-4.5f, 4.5f));
        PhotonNetwork.InstantiateRoomObject("Prefabs/" + cratePrefab.name, randomPos, Quaternion.identity);
    }
}