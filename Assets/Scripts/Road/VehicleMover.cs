using UnityEngine;

public class VehicleMover : MonoBehaviour
{
    [HideInInspector] public Vector3 startPoint;
    [HideInInspector] public Vector3 endPoint;
    [HideInInspector] public float speed = 5f;

    [Header("Despawn Settings")]
    [SerializeField] private float extraDespawnBuffer = 6f;

    private Vector3 direction;
    private float pathLength;
    private float traveledDistance;

    private void Start()
    {
        direction = (endPoint - startPoint).normalized;
        pathLength = Vector3.Distance(startPoint, endPoint);
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;

        transform.position += direction * step;
        traveledDistance += step;

        if (traveledDistance >= pathLength + extraDespawnBuffer)
            Destroy(gameObject);
    }
}
