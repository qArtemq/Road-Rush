using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] InputActionReference move;
    [SerializeField] InputActionReference jump;
    [SerializeField] public Transform visual;

    [Header("Hop Settings")]
    [SerializeField] public float hopDistance = 1.5f;
    [SerializeField] public float hopHeight = 0.7f;
    [SerializeField] public float hopDuration = 0.2f;
    [SerializeField] public float gridSize = 1.5f;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private float maxHopDistance = 3f;
    [SerializeField] private float minHopDistance = 1f;
    [SerializeField] public float hopCooldown = 0.1f;

    [Header("Audio")]
    [SerializeField] private AudioClip jumpSound;

    [Header("Skins")]
    [SerializeField] private GameObject[] skins;

    [Header("Stretch Settings")]
    [SerializeField, Range(0f, 2f)] float stretchAmount = 0.4f;
    [SerializeField, Range(0f, 1f)] float squashAmount = 0.4f;
    [SerializeField, Range(0.05f, 0.5f)] float stretchSpeed = 0.15f;

    private AudioSource audioSource;
    private Rigidbody rb;

    private bool isJumping = false;
    private bool hopQueued = false;
    private Vector3 queuedDir = Vector3.zero;
    private Vector3 prevInputDir = Vector3.zero;
    private Vector3 facingDir = Vector3.back;

    private GameObject currentLog;
    private int currentSkinIndex = -1;
    private float lastHopTime = 0f;

    public bool isDead = false;

    private void Awake()
    {
        InitializeSkins();

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void OnEnable()
    {
        move.action.Enable();
        jump.action.Enable();
    }

    private void OnDisable()
    {
        move.action.Disable();
        jump.action.Disable();
    }

    private void Update()
    {
        if (!isDead)
        {
            HandleMovement();
            HandleSkinChange();
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Log"))
        {
            currentLog = collision.gameObject;
            transform.SetParent(currentLog.transform, true);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Log"))
        {
            transform.SetParent(null, true);
            currentLog = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Transform root = other.transform.root;
        if (root == null) return;

        if (other.CompareTag("Car"))
        {
            GameSoundManager.Instance?.PlayCarHit(transform.position);
            isDead = true;
            return;
        }

        if (root.CompareTag("Train"))
        {
            GameSoundManager.Instance?.PlayTrainCrush(transform.position);
            isDead = true;
            return;
        }

        if (root.CompareTag("River"))
        {
            GameSoundManager.Instance?.PlaySplash(transform.position);
            isDead = true;
            return;
        }
    }


    private void InitializeSkins()
    {
        if (skins == null || skins.Length == 0) return;

        foreach (var s in skins)
            s.SetActive(false);

        if (skins.Length > 0)
        {
            skins[0].SetActive(true);
            currentSkinIndex = 0;
            visual = skins[0].transform;

            CameraFollow camFollow = FindFirstObjectByType<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.SetTarget(visual);
            }
        }
    }

    private void HandleSkinChange()
    {
        for (int i = 0; i < 8; i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                ChangeSkin(i);
            }
        }
    }
    private void ChangeSkin(int index)
    {
        if (index == currentSkinIndex || index < 0 || index >= skins.Length) return;

        Quaternion oldRotation = visual.rotation;
        Vector3 oldPosition = visual.position;

        if (currentSkinIndex >= 0)
            skins[currentSkinIndex].SetActive(false);

        skins[index].SetActive(true);
        currentSkinIndex = index;
        visual = skins[index].transform;
        visual.position = oldPosition;
        visual.rotation = oldRotation;

        RotateVisual(facingDir);

        CameraFollow camFollow = FindFirstObjectByType<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.SetTarget(visual);
        }
    }
    private void HandleMovement()
    {
        Vector2 moveInput = move.action.ReadValue<Vector2>();
        Vector3 dir = Vector3.zero;

        if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            dir = moveInput.x > 0 ? Vector3.left : Vector3.right;
        else if (Mathf.Abs(moveInput.y) > 0)
            dir = moveInput.y > 0 ? Vector3.back : Vector3.forward;

        if (dir.sqrMagnitude > 0.01f)
        {
            facingDir = dir.normalized;
            RotateVisual(facingDir);
        }

        if (isJumping)
        {
            if (dir != Vector3.zero && dir != prevInputDir)
            {
                hopQueued = true;
                queuedDir = dir;
            }
        }
        else
        {
            if (hopQueued)
            {
                hopQueued = false;
                StartCoroutine(Hop(queuedDir));
            }
            else
            {
                if (dir.sqrMagnitude > 0.01f && Time.time >= lastHopTime + hopDuration + hopCooldown)
                {
                    StartCoroutine(Hop(facingDir));
                    lastHopTime = Time.time;
                }
                else if (jump.action.triggered && Time.time >= lastHopTime + hopDuration + hopCooldown)
                {
                    facingDir = Vector3.back;
                    RotateVisual(facingDir);
                    StartCoroutine(Hop(facingDir));
                    lastHopTime = Time.time;
                }
            }
        }

        prevInputDir = dir;
    }


    private void RotateVisual(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
        visual.rotation = Quaternion.Euler(-90f, lookRot.eulerAngles.y, 0f);
    }

    private IEnumerator Hop(Vector3 dir)
    {
        if (isJumping) yield break;
        isJumping = true;

        audioSource.PlayOneShot(jumpSound);

        float adaptiveHopDistance = hopDistance;

        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, dir.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, maxHopDistance, platformLayer))
        {
            adaptiveHopDistance = hit.distance;
            adaptiveHopDistance = Mathf.Clamp(adaptiveHopDistance, minHopDistance, maxHopDistance);
        }

        Vector3 platformVelocity = Vector3.zero;
        if (currentLog != null)
        {
            FloatMover mover = currentLog.GetComponent<FloatMover>();
            if (mover != null)
            {
                platformVelocity = mover.Velocity;
            }

            transform.SetParent(null, true);
            currentLog = null;
        }

        Vector3 start = transform.position;
        Vector3 targetDir = dir.normalized * adaptiveHopDistance;
        Vector3 unsnappedEnd = start + targetDir;
        float snappedX = Mathf.Round(unsnappedEnd.x / gridSize) * gridSize;
        float snappedZ = Mathf.Round(unsnappedEnd.z / gridSize) * gridSize;
        Vector3 end = new Vector3(snappedX, unsnappedEnd.y, snappedZ);

        float t = 0f;

        Vector3 originalScale = visual.localScale;

        yield return StartCoroutine(StretchVisual(
            visual,
            originalScale,
            new Vector3(
                originalScale.x * (1 + squashAmount),
                originalScale.y * (1 - squashAmount),
                originalScale.z * (1 + squashAmount)
            ),
            stretchSpeed * 0.5f
        ));

        Vector3 previousPosition = start;
        while (t < 1f)
        {
            t += Time.deltaTime / hopDuration;
            float yOffset = 4 * hopHeight * t * (1 - t);
            Vector3 lerpPos = Vector3.Lerp(start, end, t) + Vector3.up * yOffset;

            float stretchY = 1 + Mathf.Sin(t * Mathf.PI) * stretchAmount;
            visual.localScale = new Vector3(
                originalScale.x / (1 + stretchAmount * 0.5f),
                originalScale.y * stretchY,
                originalScale.z / (1 + stretchAmount * 0.5f)
            );

            Vector3 platformDelta = platformVelocity * Time.deltaTime;
            transform.position = lerpPos + platformDelta;

            previousPosition = transform.position;
            yield return null;
        }


        transform.position = end + platformVelocity * hopDuration;

        yield return StartCoroutine(StretchVisual(
            visual,
            visual.localScale,
            new Vector3(
                originalScale.x * (1 + squashAmount),
                originalScale.y * (1 - squashAmount),
                originalScale.z * (1 + squashAmount)
            ),
            stretchSpeed * 0.5f
        ));

        yield return StartCoroutine(StretchVisual(visual, visual.localScale, originalScale, stretchSpeed));
        visual.localScale = originalScale;

        isJumping = false;
        CheckIfOnLog();
    }

    private void CheckIfOnLog()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.5f))
        {
            if (hit.collider.CompareTag("Log") && currentLog != hit.collider.gameObject)
            {
                currentLog = hit.collider.gameObject;
                transform.SetParent(currentLog.transform, true);
            }
        }
        else if (currentLog != null)
        {
            transform.SetParent(null, true);
            currentLog = null;
        }
    }

    private IEnumerator StretchVisual(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            target.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
    }
}