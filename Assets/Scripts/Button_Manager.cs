using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Button_Manager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TextMeshProUGUI walkStats;

    [Header("Player & Controls")]
    [SerializeField] private Player player;
    [SerializeField] private InputActionReference menuAction;

    [Header("Gameplay")]
    [SerializeField] private DistanceCounter distanceCounter;

    private bool isOpen = false;

    private void Awake()
    {
        if (endPanel) endPanel.SetActive(false);
        if (menuPanel) menuPanel.SetActive(isOpen);

        if (player == null) player = FindAnyObjectByType<Player>();
        if (distanceCounter == null) distanceCounter = GetComponent<DistanceCounter>();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (player != null && player.isDead)
        {
            ShowGameOver();
        }
        if (menuAction != null && menuAction.action.WasPerformedThisFrame() && player != null && !player.isDead)
        {
            ToggleMenu();
        }
    }

    private void ToggleMenu()
    {
        isOpen = !isOpen;

        if (menuPanel != null) menuPanel.SetActive(isOpen);

        if (isOpen)
        {
            Time.timeScale = 0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            UpdateInformationBoard();
            PauseGameSounds();
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            ResumeGameSounds();
        }
    }

    private static void ResumeGameSounds()
    {
        if (GameSoundManager.Instance != null)
        {
            foreach (var src in GameSoundManager.Instance.activeLoops)
            {
                if (src != null && !src.isPlaying)
                    src.Play();
            }
        }
    }

    private static void PauseGameSounds()
    {
        if (GameSoundManager.Instance != null)
        {
            foreach (var src in GameSoundManager.Instance.activeLoops)
            {
                if (src != null && src.isPlaying)
                    src.Pause();
            }
        }
    }

    private void ShowGameOver()
    {
        if (endPanel != null) endPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    private void UpdateInformationBoard()
    {
        int record = PlayerPrefs.GetInt("Record", 0);
        if (walkStats != null)
            walkStats.text = $"Your last record: {record} ì";
    }

    public void RestartGameButton()
    {
        Time.timeScale = 1f;
        if (endPanel) endPanel.SetActive(false);

        player.isDead = false;
        isOpen = false;
        if (menuPanel) menuPanel.SetActive(isOpen);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void GoToLobbyButtom()
    {
        Time.timeScale = 1f;
        if (distanceCounter != null)
            GameData.WalkStep = distanceCounter.WalkStep;

        SceneManager.LoadScene("Menu");
    }
}
