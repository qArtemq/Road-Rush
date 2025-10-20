using UnityEngine;
using TMPro;

public class DistanceCounter : MonoBehaviour
{
    [Header("Object Settings")]
    [SerializeField] private Transform player;

    [Header("UI Settings")]
    [SerializeField] private TMP_Text distanceText;

    [Header("Calculation Settings")]
    [SerializeField] private float unitsPerMeter = 1f;
    [SerializeField] private bool countOnlyForward = true;

    public int WalkStep => Mathf.Max(0, Mathf.FloorToInt(distance));

    private float startZ;
    public float distance;

    private void Start()
    {
        if (player == null)
            player = transform;

        startZ = player.position.z;
        distance = 0f;
        UpdateUI();
    }

    private void Update()
    {
        float delta = (startZ - player.position.z) / unitsPerMeter;

        if (countOnlyForward)
            distance = Mathf.Max(distance, delta);
        else
            distance = delta;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (distanceText != null)
            distanceText.text = $"{WalkStep} m";
    }

    public void ResetCounter()
    {
        startZ = player.position.z;
        distance = 0f;
        UpdateUI();
    }
}
