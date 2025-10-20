using UnityEngine;

public class FloatMover : MonoBehaviour
{
    [HideInInspector] public Vector3 startPoint;
    [HideInInspector] public Vector3 endPoint;
    [HideInInspector] public float speed = 3f;
    [HideInInspector] public float despawnBuffer = 6f;

    private Vector3 dir;
    private float pathLen;
    private float traveled;

    public Vector3 Velocity => dir * speed;

    void Start()
    {
        dir = (endPoint - startPoint).normalized;
        pathLen = Vector3.Distance(startPoint, endPoint);
        transform.position = startPoint;
    }

    void LateUpdate()
    {
        float step = speed * Time.deltaTime;
        transform.position += dir * step;
        traveled += step;

        if (traveled >= pathLen + despawnBuffer)
            Destroy(gameObject);
    }
}