using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Chene : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI infoStats;
    [SerializeField] private TextMeshProUGUI walkStats;

    private void Awake()
    {
        int savedRecord = PlayerPrefs.GetInt("Record", 0);

        if (GameData.WalkStep > savedRecord)
        {
            savedRecord = GameData.WalkStep;
            PlayerPrefs.SetInt("Record", savedRecord);
            PlayerPrefs.Save();

            infoStats.text = "New Record";
        }
        else
        {
            infoStats.text = "Your Record";
        }

        GameData.WalkStep = savedRecord;
        walkStats.text = $"Distance: {GameData.WalkStep}m";
    }
    public void ButtomGame()
    {
        SceneManager.LoadScene("Game");
    }
    public void ButtomExit()
    {
        PlayerPrefs.Save();
        Application.Quit();
    }
    public void ButtonResetRecord()
    {
        PlayerPrefs.DeleteKey("Record");
        PlayerPrefs.Save();

        infoStats.text = "Record cleared!";
        walkStats.text = "Distance: 0m";
        GameData.WalkStep = 0;
    }
}
public static class GameData
{
    public static int WalkStep;
}
