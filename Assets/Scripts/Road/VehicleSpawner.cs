using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleSpawner : MonoBehaviour
{
    [Header("Path Settings")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Vehicle Prefabs")]
    public GameObject[] vehiclePrefabs;

    [Header("Traffic Parameters")]
    public Vector2 spawnIntervalRange = new Vector2(1.2f, 2.5f);
    public Vector2 speedRange = new Vector2(4f, 8f);

    [Header("Parent Folder")]
    public Transform parentFolder;

    [Header("Road Settings")]
    public float roadWidth = 4f;
    public int laneCount = 2;
    public float laneJitter = 0.3f;

    [Header("Fast Start Settings")]
    public bool useFastStart = true;
    public float startSpeedMultiplier = 2f;
    public float startSpawnMultiplier = 0.5f;
    public float fastStartDuration = 5f;

    [Header("Vehicle Spacing")]
    public float minDistanceBetween = 6f;

    [Header("Visual Settings")]
    public bool useJitter = true;

    [Header("Spawner Options")]
    public bool autoStart = true;

    private Coroutine spawnRoutine;
    private Vector3 forward;
    private Vector3 right;

    private Dictionary<int, List<GameObject>> laneVehicles = new Dictionary<int, List<GameObject>>();
    private Dictionary<int, float> laneSpeeds = new Dictionary<int, float>();


    private void Awake()
    {
        forward = (endPoint.position - startPoint.position).normalized;
        right = Vector3.Cross(Vector3.up, forward).normalized;

        for (int i = 0; i < laneCount; i++)
        {
            laneVehicles[i] = new List<GameObject>();
            laneSpeeds[i] = Random.Range(speedRange.x, speedRange.y);
        }

        if (parentFolder == null)
        {
            GameObject folder = new GameObject($"{name}_Vehicles");
            parentFolder = folder.transform;
            parentFolder.SetParent(transform);
        }
    }

    private void OnEnable()
    {
        if (autoStart)
            StartSpawning();
    }

    private void OnDisable()
    {
        StopSpawning();
    }


    public void StartSpawning()
    {
        if (spawnRoutine == null)
        {
            spawnRoutine = StartCoroutine(SpawnLoop());
            if (GameSoundManager.Instance != null)
            {
                GameSoundManager.Instance.AddLoopToObject(gameObject, GameSoundManager.Instance.carEngineLoop, true, 1f, true);
            }
        }
    }

    public void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.5f));

        float timer = 0f;

        while (true)
        {
            TrySpawnVehicle();

            GameSoundManager.Instance?.PlayCarHorn(transform.position);

            float speedMul = 1f;
            float spawnMul = 1f;

            if (useFastStart && timer < fastStartDuration)
            {
                float t = timer / fastStartDuration;
                speedMul = Mathf.Lerp(startSpeedMultiplier, 1f, t);
                spawnMul = Mathf.Lerp(startSpawnMultiplier, 1f, t);
            }

            float wait = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y) * spawnMul;
            yield return new WaitForSeconds(wait);

            timer += wait;
        }
    }


    private void TrySpawnVehicle()
    {
        if (vehiclePrefabs.Length == 0 || startPoint == null || endPoint == null)
            return;

        int laneIndex = Random.Range(0, laneCount);
        float laneOffset = -roadWidth / 2f + (roadWidth / Mathf.Max(1, laneCount - 1)) * laneIndex;
        float jitter = useJitter ? Random.Range(-laneJitter, laneJitter) : 0f;
        Vector3 spawnPos = startPoint.position + right * (laneOffset + jitter);

        if (laneVehicles[laneIndex].Count > 0)
        {
            GameObject lastCar = laneVehicles[laneIndex][laneVehicles[laneIndex].Count - 1];
            if (lastCar != null)
            {
                float dist = Vector3.Distance(lastCar.transform.position, spawnPos);
                if (dist < minDistanceBetween) return;
            }
        }

        GameObject prefab = vehiclePrefabs[Random.Range(0, vehiclePrefabs.Length)];
        GameObject go = Instantiate(prefab, spawnPos, Quaternion.LookRotation(forward, Vector3.up));

        if (parentFolder != null)
            go.transform.SetParent(parentFolder);

        float laneSpeed = laneSpeeds[laneIndex];
        VehicleMover mover = go.GetComponent<VehicleMover>();
        if (mover == null) mover = go.AddComponent<VehicleMover>();
        mover.startPoint = spawnPos;
        mover.endPoint = endPoint.position + right * (laneOffset + jitter);
        mover.speed = laneSpeed;

        laneVehicles[laneIndex].Add(go);

        CleanupList(laneIndex);
    }

    private void CleanupList(int laneIndex)
    {
        for (int i = laneVehicles[laneIndex].Count - 1; i >= 0; i--)
        {
            if (laneVehicles[laneIndex][i] == null)
                laneVehicles[laneIndex].RemoveAt(i);
        }

        if (laneVehicles[laneIndex].Count == 0)
        {
            laneSpeeds[laneIndex] = Random.Range(speedRange.x, speedRange.y);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (startPoint == null || endPoint == null) return;

        forward = (endPoint.position - startPoint.position).normalized;
        right = Vector3.Cross(Vector3.up, forward).normalized;

        Gizmos.color = Color.cyan;
        Vector3 leftEdgeStart = startPoint.position - right * (roadWidth / 2f);
        Vector3 rightEdgeStart = startPoint.position + right * (roadWidth / 2f);
        Vector3 leftEdgeEnd = endPoint.position - right * (roadWidth / 2f);
        Vector3 rightEdgeEnd = endPoint.position + right * (roadWidth / 2f);

        Gizmos.DrawLine(leftEdgeStart, leftEdgeEnd);
        Gizmos.DrawLine(rightEdgeStart, rightEdgeEnd);
        Gizmos.DrawLine(leftEdgeStart, rightEdgeStart);
        Gizmos.DrawLine(leftEdgeEnd, rightEdgeEnd);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < laneCount; i++)
        {
            float laneOffset = -roadWidth / 2f + (roadWidth / Mathf.Max(1, laneCount - 1)) * i;
            Vector3 laneStart = startPoint.position + right * laneOffset;
            Vector3 laneEnd = endPoint.position + right * laneOffset;
            Gizmos.DrawLine(laneStart, laneEnd);
        }
    }
#endif
}
