using UnityEngine;

public class AircraftController : MonoBehaviour
{
    [SerializeField] private GameObject AircraftPrefab;
    [SerializeField] private float spawnAreaSize;
    [SerializeField] private float groundY;

    void Start()
    {
        SpawnDrone();
    }

    public void SpawnDrone()
    {
        Vector3 randomSpawnPos = new Vector3(
            Random.Range(-spawnAreaSize, spawnAreaSize),
            groundY,
            Random.Range(-spawnAreaSize, spawnAreaSize)
        );

        Instantiate(AircraftPrefab, randomSpawnPos, Quaternion.identity);
    }
}
