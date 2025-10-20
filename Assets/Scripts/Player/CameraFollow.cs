using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Player player;

    [Header("Follow Settings")]
    [SerializeField] private float baseScrollSpeed = 1.2f;
    [SerializeField] private float catchUpSpeed = 5f;
    [SerializeField] private float followSpeedX = 3f;
    [SerializeField] private float loseDelay = 0.2f;

    private Camera cam;
    private bool isLosing = false;
    private Vector3 offset;

    private void Start()
    {
        cam = GetComponent<Camera>();

        if (target != null)
            offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        if (!target || isLosing) return;

        Vector3 camPos = transform.position;

        camPos += Vector3.back * baseScrollSpeed * Time.deltaTime;

        if (target.position.z + offset.z < camPos.z)
        {
            camPos.z = Mathf.Lerp(camPos.z, target.position.z + offset.z, catchUpSpeed * Time.deltaTime);
        }

        camPos.x = Mathf.Lerp(camPos.x, target.position.x + offset.x, followSpeedX * Time.deltaTime);

        transform.position = camPos;

        Vector3 viewportPos = cam.WorldToViewportPoint(target.position);
        bool isVisible = viewportPos.z > 0 && 
                         viewportPos.x > 0 && viewportPos.x < 1 && 
                         viewportPos.y > 0 && viewportPos.y < 1;

        if (!isVisible)
            StartCoroutine(LoseAfterDelay());
    }

    private IEnumerator LoseAfterDelay()
    {
        isLosing = true;
        yield return new WaitForSeconds(loseDelay);
        player.isDead = true;
    }

    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null) return;

        target = newTarget;
        offset = transform.position - target.position;
    }
}
