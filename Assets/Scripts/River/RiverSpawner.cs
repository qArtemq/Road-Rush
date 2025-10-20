using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverSpawner : MonoBehaviour
{
    [Header("River Boundaries")]
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Float Prefabs")]
    public GameObject[] floatPrefabs;

    [Header("Movement Parameters")]
    public Vector2 speedRange = new Vector2(2f, 4f);
    public Vector2 spawnIntervalRange = new Vector2(1f, 2.5f);
    public float riverWidth = 6f;
    public int laneCount = 3;
    public float minDistanceBetween = 4f;
    public float verticalOffset = 0f;
    public float despawnBuffer = 6f;

    [Header("Fast Start Settings")]
    public bool useFastStart = true;
    public float startSpeedMultiplier = 2f;
    public float startSpawnMultiplier = 0.5f;
    public float fastStartDuration = 5f;

    [Header("Parent Folder")]
    public Transform parentFolder;

    [Header("Spawner Options")]
    public bool autoStart = true;

    private Coroutine loop;
    private Vector3 baseDir, baseRight;
    private AudioSource riverLoopSource;

    private class LaneState
    {
        public List<GameObject> active = new List<GameObject>();
        public bool? fromLeft = null;
        public float laneOffset = 0f;
        public float speed = 0f;
    }

    private List<LaneState> lanes = new List<LaneState>();

    private void Awake()
    {
        if (leftPoint != null && rightPoint != null)
        {
            baseDir = (rightPoint.position - leftPoint.position).normalized;
            baseRight = Vector3.Cross(Vector3.up, baseDir).normalized;
        }
        if (parentFolder == null)
        {
            GameObject folder = new GameObject($"{name}_Objects");
            parentFolder = folder.transform;
            parentFolder.SetParent(transform);
        }
        InitializeLanes();
    }

    private void OnEnable()
    {
        if (autoStart) StartSpawning();
    }

    private void OnDisable()
    {
        StopSpawning();
    }

    private void InitializeLanes()
    {
        lanes.Clear();

        for (int i = 0; i < laneCount; i++)
        {
            var lane = new LaneState();

            lane.laneOffset = (laneCount == 1)
                ? 0f
                : -riverWidth / 2f + (riverWidth / (laneCount - 1)) * i;

            lane.speed = Random.Range(speedRange.x, speedRange.y);

            lanes.Add(lane);
        }
    }

    public void StartSpawning()
    {
        if (loop != null) return;

        loop = StartCoroutine(SpawnLoop());

        if (GameSoundManager.Instance != null)
        {
            riverLoopSource = GameSoundManager.Instance.AddLoopToObject(gameObject, GameSoundManager.Instance.riverLoop, true, 0.5f, true);
        }
    }

    public void StopSpawning()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }

        if (riverLoopSource != null)
            GameSoundManager.Instance?.RemoveLoop(riverLoopSource);
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(Random.Range(0f, 0.5f));
        float timer = 0f;

        while (true)
        {
            int laneIndex = Random.Range(0, lanes.Count);
            TrySpawnInLane(laneIndex);

            float spawnMul = 1f;
            if (useFastStart && timer < fastStartDuration)
            {
                float t = timer / fastStartDuration;
                spawnMul = Mathf.Lerp(startSpawnMultiplier, 1f, t);
            }

            float wait = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y) * spawnMul;
            yield return new WaitForSeconds(wait);

            timer += wait;
        }
    }



    private void TrySpawnInLane(int laneIndex)
    {
        if (floatPrefabs.Length == 0 || leftPoint == null || rightPoint == null) 
            return;

        var lane = lanes[laneIndex];

        if (lane.active.Count == 0)
        {
            lane.fromLeft = (Random.value > 0.5f);
            lane.speed = Random.Range(speedRange.x, speedRange.y);
        }

        bool fromLeft = lane.fromLeft ?? true;

        Vector3 start = fromLeft ? leftPoint.position : rightPoint.position;
        Vector3 end = fromLeft ? rightPoint.position : leftPoint.position;

        Vector3 spawnPos = start + baseRight * lane.laneOffset + Vector3.up * verticalOffset;

        if (lane.active.Count > 0)
        {
            GameObject last = lane.active[lane.active.Count - 1];
            if (last != null && Vector3.Distance(last.transform.position, spawnPos) < minDistanceBetween)
                return;
        }

        GameObject prefab = floatPrefabs[Random.Range(0, floatPrefabs.Length)];
        GameObject go = Instantiate(prefab, spawnPos, Quaternion.LookRotation((end - start).normalized, Vector3.up));

        if (parentFolder != null)
            go.transform.SetParent(parentFolder);

        var mover = go.GetComponent<FloatMover>();
        if (mover == null) mover = go.AddComponent<FloatMover>();
        mover.startPoint = start + baseRight * lane.laneOffset;
        mover.endPoint = end + baseRight * lane.laneOffset;

        float speedMul = 1f;
        if (useFastStart)
        {
            float t = Mathf.Clamp01(Time.timeSinceLevelLoad / fastStartDuration);
            speedMul = Mathf.Lerp(startSpeedMultiplier, 1f, t);
        }

        mover.speed = lane.speed * speedMul;
        mover.despawnBuffer = despawnBuffer;

        lane.active.Add(go);
        CleanupLane(lane);

        lanes[laneIndex] = lane;
    }

    private void CleanupLane(LaneState lane)
    {
        for (int i = lane.active.Count - 1; i >= 0; i--)
        {
            if (lane.active[i] == null) lane.active.RemoveAt(i);
        }

        if (lane.active.Count == 0)
        {
            lane.fromLeft = null;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (leftPoint == null || rightPoint == null) return;

        Vector3 dir = (rightPoint.position - leftPoint.position).normalized;
        Vector3 cross = Vector3.Cross(Vector3.up, dir).normalized;

        Gizmos.color = Color.cyan;
        Vector3 p1 = leftPoint.position + cross * (riverWidth / 2f);
        Vector3 p2 = leftPoint.position - cross * (riverWidth / 2f);
        Vector3 p3 = rightPoint.position + cross * (riverWidth / 2f);
        Vector3 p4 = rightPoint.position - cross * (riverWidth / 2f);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p1, p3);
        Gizmos.DrawLine(p2, p4);

        Gizmos.color = Color.yellow;
        for (int i = 0; i < laneCount; i++)
        {
            float laneOffset = (laneCount == 1)
                ? 0f
                : -riverWidth / 2f + (riverWidth / (laneCount - 1)) * i;

            Vector3 ls = leftPoint.position + cross * laneOffset;
            Vector3 rs = rightPoint.position + cross * laneOffset;
            Gizmos.DrawLine(ls, rs);
        }
    }
#endif
}
