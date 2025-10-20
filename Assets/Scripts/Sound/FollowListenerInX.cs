using UnityEngine;

public class FollowListenerInX : MonoBehaviour
{
    private Transform listenerTransform;

    void Start()
    {
        listenerTransform = Camera.main.transform;
    }

    void Update()
    {
        if (listenerTransform != null)
        {
            transform.position = new Vector3(
                listenerTransform.position.x,
                transform.position.y,
                transform.position.z
            );
        }
    }
}