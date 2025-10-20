using UnityEngine;

public class IslandData : MonoBehaviour
{
    [Header("Island Settings")]
    public IslandType type;
    public float overlapOffset = 0f;

    [HideInInspector] public float localMinZ;
    [HideInInspector] public float localMaxZ;
}

public enum IslandType
{
    None,
    Start,
    sideIsland,
    Grass,
    Road,
    Rail,
    River
}