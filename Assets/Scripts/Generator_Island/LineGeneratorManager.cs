using UnityEngine;
using System.Collections.Generic;

public class LineGeneratorManager : MonoBehaviour
{
    [Header("Island Prefabs")]
    public GameObject[] grassPrefabs;
    public GameObject[] roadPrefabs;
    public GameObject firstIslandPrefab;
    public GameObject railIslandPrefab;
    public GameObject riverIslandPrefab;
    public GameObject sideIslandPrefab;

    [Header("Side Island Settings")]
    public float sideOffset = 71.5f;
    public int initialSideIslands = 10;
    public int sideIslandsBehind = 3;

    [Header("Generation Settings")]
    public Transform player;
    public int initialIslands = 10;
    public int islandsBehind = 3;
    public float generateDistance = 50f;
    public float destroyDistance = 50f;

    [Header("Island Type Weights")]
    public float grassWeight = 0.4f;
    public float roadWeight = 0.3f;
    public float railWeight = 0.15f;
    public float riverWeight = 0.15f;

    [Header("Island Group Sizes")]
    public int minGrassGroup = 1;
    public int maxGrassGroup = 2;
    public int minRoadGroup = 1;
    public int maxRoadGroup = 2;
    public int minRailGroup = 1;
    public int maxRailGroup = 3;
    public int minRiverGroup = 1;
    public int maxRiverGroup = 2;

    private List<GameObject> activeIslands = new List<GameObject>();

    private float nextAttachmentForward = 0f;
    private float nextAttachmentBackward = 0f;

    private float nextAttachmentLeftForward = 0f;
    private float nextAttachmentLeftBackward = 0f;

    private float nextAttachmentRightForward = 0f;
    private float nextAttachmentRightBackward = 0f;

    private IslandType lastType = IslandType.None;

    void Start()
    {
        if (riverIslandPrefab == null)
        {
            Debug.LogWarning("RiverIslandPrefab is not assigned! River islands will be skipped.");
            riverWeight = 0f;
        }

        SpawnFirstIsland();

        for (int i = 0; i < islandsBehind; i++)
        {
            GenerateNextIsland(true);
        }

        for (int i = 0; i < initialIslands; i++)
        {
            GenerateNextIsland(false);
        }

        for (int i = 0; i < sideIslandsBehind - islandsBehind; i++)
        {
            GenerateSideIsland(true, -sideOffset, ref nextAttachmentLeftForward, ref nextAttachmentLeftBackward);
            GenerateSideIsland(true, sideOffset, ref nextAttachmentRightForward, ref nextAttachmentRightBackward);
        }

        for (int i = 0; i < initialSideIslands - initialIslands; i++)
        {
            GenerateSideIsland(false, -sideOffset, ref nextAttachmentLeftForward, ref nextAttachmentLeftBackward);
            GenerateSideIsland(false, sideOffset, ref nextAttachmentRightForward, ref nextAttachmentRightBackward);
        }
    }

    void Update()
    {
        if (player == null) return;

        if (player.position.z - generateDistance < nextAttachmentForward)
        {
            GenerateNextIsland(false);
        }

        if (player.position.z - generateDistance < nextAttachmentLeftForward)
        {
            GenerateSideIsland(false, -sideOffset, ref nextAttachmentLeftForward, ref nextAttachmentLeftBackward);
        }

        if (player.position.z - generateDistance < nextAttachmentRightForward)
        {
            GenerateSideIsland(false, sideOffset, ref nextAttachmentRightForward, ref nextAttachmentRightBackward);
        }

        for (int i = activeIslands.Count - 1; i >= 0; i--)
        {
            GameObject island = activeIslands[i];
            IslandData data = island.GetComponent<IslandData>();
            float globalMinZ = island.transform.position.z + data.localMinZ;

            if (globalMinZ > player.position.z + destroyDistance)
            {
                activeIslands.RemoveAt(i);
                Destroy(island);
            }
        }
    }

    private void SpawnFirstIsland()
    {
        GameObject island = Instantiate(firstIslandPrefab);
        island.SetActive(true);

        Bounds bounds = CalculateBounds(island);
        float localMinZ = bounds.min.z;
        float localMaxZ = bounds.max.z;

        IslandData data = island.GetComponent<IslandData>();
        data.localMinZ = localMinZ;
        data.localMaxZ = localMaxZ;

        float positionZ = 0f - localMinZ;
        island.transform.position = new Vector3(0, 0, positionZ);

        float globalMinZ = positionZ + localMinZ;
        float globalMaxZ = positionZ + localMaxZ;

        nextAttachmentForward = globalMinZ + data.overlapOffset;
        nextAttachmentBackward = globalMaxZ + data.overlapOffset;

        activeIslands.Add(island);

        SpawnFirstSideIsland(-sideOffset, globalMinZ, globalMaxZ, ref nextAttachmentLeftForward, ref nextAttachmentLeftBackward);
        SpawnFirstSideIsland(sideOffset, globalMinZ, globalMaxZ, ref nextAttachmentRightForward, ref nextAttachmentRightBackward);

        lastType = data.type;
    }

    private void SpawnFirstSideIsland(float xOffset, float alignMinZ, float alignMaxZ, ref float nextForward, ref float nextBackward)
    {
        GameObject side = Instantiate(sideIslandPrefab);
        side.SetActive(true);

        Bounds bounds = CalculateBounds(side);
        float localMinZ = bounds.min.z;
        float localMaxZ = bounds.max.z;

        IslandData data = side.GetComponent<IslandData>();
        data.localMinZ = localMinZ;
        data.localMaxZ = localMaxZ;

        float positionZ = alignMinZ - localMinZ;
        side.transform.position = new Vector3(xOffset, 0, positionZ);

        float globalMinZ = positionZ + localMinZ;
        float globalMaxZ = positionZ + localMaxZ;

        nextForward = globalMinZ + data.overlapOffset;
        nextBackward = globalMaxZ + data.overlapOffset;

        activeIslands.Add(side);
    }

    private void GenerateNextIsland(bool isBackward)
    {
        IslandType type = GetNextIslandType();

        int minGroup = GetMinGroup(type);
        int maxGroup = GetMaxGroup(type);
        int groupSize = Random.Range(minGroup, maxGroup + 1);

        for (int i = 0; i < groupSize; i++)
        {
            GameObject prefab = GetPrefabByType(type);
            if (prefab == null)
            {
                continue;
            }
            GameObject island = Instantiate(prefab);
            island.SetActive(true);

            Bounds bounds = CalculateBounds(island);
            float localMinZ = bounds.min.z;
            float localMaxZ = bounds.max.z;

            IslandData data = island.GetComponent<IslandData>();
            data.localMinZ = localMinZ;
            data.localMaxZ = localMaxZ;

            float positionZ;
            if (isBackward)
            {
                positionZ = nextAttachmentBackward - localMinZ;
                nextAttachmentBackward = positionZ + localMaxZ + data.overlapOffset;
            }
            else
            {
                positionZ = nextAttachmentForward - localMaxZ;
                nextAttachmentForward = positionZ + localMinZ + data.overlapOffset;
            }

            island.transform.position = new Vector3(0, 0, positionZ);

            activeIslands.Add(island);

            GenerateSideIsland(isBackward, -sideOffset, ref nextAttachmentLeftForward, ref nextAttachmentLeftBackward);
            GenerateSideIsland(isBackward, sideOffset, ref nextAttachmentRightForward, ref nextAttachmentRightBackward);
        }

        lastType = type;
    }

    private void GenerateSideIsland(bool isBackward, float xOffset, ref float nextForward, ref float nextBackward)
    {
        GameObject side = Instantiate(sideIslandPrefab);
        side.SetActive(true);

        Bounds bounds = CalculateBounds(side);
        float localMinZ = bounds.min.z;
        float localMaxZ = bounds.max.z;

        IslandData data = side.GetComponent<IslandData>();
        data.localMinZ = localMinZ;
        data.localMaxZ = localMaxZ;

        float positionZ;
        if (isBackward)
        {
            positionZ = nextBackward - localMinZ;
            nextBackward = positionZ + localMaxZ + data.overlapOffset;
        }
        else
        {
            positionZ = nextForward - localMaxZ;
            nextForward = positionZ + localMinZ + data.overlapOffset;
        }

        side.transform.position = new Vector3(xOffset, 0, positionZ);

        activeIslands.Add(side);
    }

    private Bounds CalculateBounds(GameObject obj)
    {
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        bool hasBounds = false;

        foreach (Collider c in obj.GetComponentsInChildren<Collider>())
        {
            bounds.Encapsulate(c.bounds);
            hasBounds = true;
        }

        if (!hasBounds)
        {
            foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
            {
                bounds.Encapsulate(r.bounds);
                hasBounds = true;
            }
        }

        if (!hasBounds)
        {
            bounds = new Bounds(Vector3.zero, new Vector3(1f, 1f, 10f));
        }

        return bounds;
    }

    private IslandType GetNextIslandType()
    {
        int maxAttempts = 10;
        IslandType type;
        float effectiveRiverWeight = (riverIslandPrefab == null) ? 0f : riverWeight;

        do
        {
            float totalWeight = grassWeight + roadWeight + railWeight + riverWeight;
            float rand = Random.value * totalWeight;

            if (rand < grassWeight) type = IslandType.Grass;
            else if (rand < grassWeight + roadWeight) type = IslandType.Road;
            else if (rand < grassWeight + roadWeight + railWeight) type = IslandType.Rail;
            else type = IslandType.River;

            maxAttempts--;
            if (maxAttempts <= 0)
            {
                type = IslandType.Grass;
                break;
            }

        } while (type == lastType);

        return type;
    }

    private int GetMinGroup(IslandType type)
    {
        switch (type)
        {
            case IslandType.Grass: return minGrassGroup;
            case IslandType.Road: return minRoadGroup;
            case IslandType.Rail: return minRailGroup;
            case IslandType.River: return minRiverGroup;
            default: return 1;
        }
    }

    private int GetMaxGroup(IslandType type)
    {
        switch (type)
        {
            case IslandType.Grass: return maxGrassGroup;
            case IslandType.Road: return maxRoadGroup;
            case IslandType.Rail: return maxRailGroup;
            case IslandType.River: return maxRiverGroup;
            default: return 1;
        }
    }

    private GameObject GetPrefabByType(IslandType type)
    {
        switch (type)
        {
            case IslandType.Grass:
                if (grassPrefabs.Length > 0)
                    return grassPrefabs[Random.Range(0, grassPrefabs.Length)];
                else
                {
                    return null;
                }
            case IslandType.Road:
                if (roadPrefabs.Length > 0)
                    return roadPrefabs[Random.Range(0, roadPrefabs.Length)];
                else
                {
                    return null;
                }
            case IslandType.Rail:
                if (railIslandPrefab != null)
                    return railIslandPrefab;
                else
                {
                    return null;
                }
            case IslandType.River:
                if (riverIslandPrefab != null)
                    return riverIslandPrefab;
                else
                {
                    return null;
                }
            default:
                return grassPrefabs.Length > 0 ? grassPrefabs[0] : null;
        }
    }
}
