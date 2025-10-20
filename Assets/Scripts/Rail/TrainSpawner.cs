using System.Collections;
using UnityEngine;

public class TrainSpawner : MonoBehaviour
{
    [Header("Train Parameters")]
    public GameObject trainPrefab;
    public Transform startPoint;
    public Transform endPoint;
    public float trainSpeed = 25f;
    public float despawnBuffer = 10f;

    [Header("Intervals")]
    public Vector2 intervalRange = new Vector2(10f, 18f);
    public float warningDelay = 2.5f;

    [Header("Signals")]
    public Renderer signalRenderer;
    public Color greenColor = Color.green;
    public Color redColor = Color.red;
    public Color offColor = Color.black;
    public float blinkInterval = 0.3f;

    private Coroutine routine;
    private Coroutine blinkRoutine;
    private bool isRunning = false;

    private void OnEnable()
    {
        StartTrainCycle();
        SetEmissionColor(greenColor);
    }

    private void OnDisable()
    {
        StopTrainCycle();
    }

    public void StartTrainCycle()
    {
        if (!isRunning)
        {
            routine = StartCoroutine(TrainCycle());
            isRunning = true;
        }
    }

    public void StopTrainCycle()
    {
        if (isRunning && routine != null)
        {
            StopCoroutine(routine);
            isRunning = false;
        }

        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }
    }

    private IEnumerator TrainCycle()
    {
        while (true)
        {
            float wait = Random.Range(intervalRange.x, intervalRange.y);
            yield return new WaitForSeconds(wait);

            TriggerWarning();
            yield return new WaitForSeconds(warningDelay);

            SpawnTrain();
        }
    }

    private void TriggerWarning()
    {

        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkLight());
        GameSoundManager.Instance?.PlayTrainWarning(transform.position);
    }

    private void StopWarning()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        SetEmissionColor(greenColor);
    }

    private IEnumerator BlinkLight()
    {
        bool state = false;
        while (true)
        {
            state = !state;
            SetEmissionColor(state ? redColor : offColor);
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    private void SpawnTrain()
    {
        if (trainPrefab == null || startPoint == null || endPoint == null)
            return;

        GameObject train = Instantiate(trainPrefab, startPoint.position, Quaternion.LookRotation((endPoint.position - startPoint.position).normalized));

        TrainMover mover = train.GetComponent<TrainMover>();
        if (mover == null) mover = train.AddComponent<TrainMover>();

        mover.startPoint = startPoint.position;
        mover.endPoint = endPoint.position;
        mover.speed = trainSpeed;
        mover.despawnBuffer = despawnBuffer;

        GameSoundManager.Instance?.PlayTrainPass(transform.position);

        StartCoroutine(WaitForTrainToEnd(mover));
    }

    private IEnumerator WaitForTrainToEnd(TrainMover mover)
    {
        while (mover != null)
            yield return null;

        StopWarning();
    }

    private void SetEmissionColor(Color color)
    {
        if (signalRenderer == null) return;

        Material[] mats = signalRenderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i].HasProperty("_EmissionColor") &&
                mats[i].IsKeywordEnabled("_EMISSION"))
            {
                mats[i].SetColor("_EmissionColor", color);
                signalRenderer.materials = mats;
                return;
            }
        }
    }
}
