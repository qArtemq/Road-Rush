using UnityEngine;

public class TrainMover : MonoBehaviour
{
    [HideInInspector] public Vector3 startPoint;
    [HideInInspector] public Vector3 endPoint;
    [HideInInspector] public float speed = 20f;
    [HideInInspector] public float despawnBuffer = 10f;

    private Vector3 dir;
    private float pathLength;
    private float traveled;

    private void Start()
    {
        dir = (endPoint - startPoint).normalized;
        pathLength = Vector3.Distance(startPoint, endPoint);
        transform.position = startPoint;
    }

    private void Update()
    {
        float step = speed * Time.deltaTime;
        transform.position += dir * step;
        traveled += step;

        if (traveled >= pathLength + despawnBuffer)
            Destroy(gameObject);
    }
}
