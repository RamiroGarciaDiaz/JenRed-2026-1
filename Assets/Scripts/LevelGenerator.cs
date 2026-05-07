using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LevelGenerator : MonoBehaviourPunCallbacks
{
    [Header("Colección de Mapas")]
    public Texture2D[] mapPool; // Arrastra aquí todas tus texturas de niveles
    private Texture2D selectedMap;

    [Header("Ajustes de Generación")]
    public float pixelOffset = 1f;
    private const string MAP_INDEX_PROP = "MapIndex";

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject trapPrefab;
    public GameObject spawnPrefab;
    public GameObject goalPrefab;

    private bool levelGenerated = false;

    void Start()
    {
        // 1. Solo el Master Client decide qué mapa toca
        if (PhotonNetwork.IsMasterClient)
        {
            int randomIndex = Random.Range(0, mapPool.Length);
            Hashtable props = new Hashtable { { MAP_INDEX_PROP, randomIndex } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        }
        else
        {
            // 2. Si no soy Master, chequeo si la propiedad ya existe (por si entré tarde)
            CheckForMapProperty();
        }
    }

    // Se llama automáticamente cuando cambian las propiedades de la sala
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (!levelGenerated && propertiesThatChanged.ContainsKey(MAP_INDEX_PROP))
        {
            int index = (int)propertiesThatChanged[MAP_INDEX_PROP];
            StartGeneration(index);
        }
    }

    private void CheckForMapProperty()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(MAP_INDEX_PROP))
        {
            int index = (int)PhotonNetwork.CurrentRoom.CustomProperties[MAP_INDEX_PROP];
            StartGeneration(index);
        }
    }

    private void StartGeneration(int index)
    {
        if (index < 0 || index >= mapPool.Length) return;
        selectedMap = mapPool[index];
        GenerateLevel();
        levelGenerated = true;
    }

    public void GenerateLevel()
    {
        // ... (La lógica de lectura de píxeles se mantiene igual)
        for (int x = 0; x < selectedMap.width; x++)
        {
            for (int y = 0; y < selectedMap.height; y++)
            {
                Color pixelColor = selectedMap.GetPixel(x, y);
                Vector3 position = new Vector3(x * pixelOffset, 0, y * pixelOffset);
                if (pixelColor.a < 0.1f) continue;
                SpawnAndScale(pixelColor, position);
            }
        }
    }

    void SpawnAndScale(Color color, Vector3 pos)
    {
        GameObject objToSpawn = null;
        bool isFloor = false;

        if (ColorMatch(color, Color.white)) objToSpawn = wallPrefab;
        else if (ColorMatch(color, Color.black)) { objToSpawn = floorPrefab; isFloor = true; }
        else if (ColorMatch(color, Color.red)) objToSpawn = trapPrefab;
        else if (ColorMatch(color, Color.green)) objToSpawn = spawnPrefab;
        else if (ColorMatch(color, Color.blue)) objToSpawn = goalPrefab;

        if (objToSpawn != null)
        {
            Vector3 finalPos = isFloor ? new Vector3(pos.x, -0.01f, pos.z) : pos;
            GameObject instance = Instantiate(objToSpawn, finalPos, Quaternion.identity, transform);
            AdjustScale(instance, pixelOffset);
        }
    }

    void AdjustScale(GameObject obj, float targetSize)
    {
        Renderer renderer = obj.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Vector3 currentSize = renderer.bounds.size;
            float scaleX = (targetSize / currentSize.x) * obj.transform.localScale.x;
            float scaleZ = (targetSize / currentSize.z) * obj.transform.localScale.z;
            obj.transform.localScale = new Vector3(scaleX, obj.transform.localScale.y, scaleZ);
        }
    }

    bool ColorMatch(Color a, Color b) => Vector4.Distance(a, b) < 0.1f;
}