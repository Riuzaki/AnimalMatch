using UnityEngine;
using UnityEngine.UI;

public class GuiController : MonoBehaviour
{

    public Image pauseWindow;
    public Image volumeImg;
    public Image blurImg;
    public Image gameOverWindow;
    public Image levelCompleteWindow;

    public Text pointText;
    public Text timerText;

    public Slider pointsBar;

    public Sprite volumeOff;
    public Sprite volumeOn;

    private bool activeMode;

    private int pointsPlayer;
    public int timeForLevel;

    private float levelTimer;

    public delegate void ChangeActiveGame(bool isActive);
    public static event ChangeActiveGame OnActiveGameHandler;

    public delegate void RestartGame();
    public static event RestartGame RestartGameHandler;

    private void OnEnable()
    {
        BoardController.ChangePointsHandler += ChangePoints;
        AudioController.MuteChangeHandler += MuteChange;
    }

    private void OnDisable()
    {
        BoardController.ChangePointsHandler -= ChangePoints;
        AudioController.MuteChangeHandler -= MuteChange;
    }

    void Start()
    {
        levelTimer = timeForLevel;
        pointsPlayer = 0;
        activeMode = true;

    }

    void Update()
    {
        if (activeMode)
        {
            UpdateTimer();
        }

    }

    public void ChangePauseMode()
    {
        if (activeMode)
        {
            pauseWindow.gameObject.SetActive(true);
            Time.timeScale = 0;
            blurImg.gameObject.SetActive(true);
            activeMode = false;
        }
        else
        {
            blurImg.gameObject.SetActive(false);
            pauseWindow.gameObject.SetActive(false);
            activeMode = true;
            Time.timeScale = 1;
        }
        OnActiveGameHandler(activeMode);
    }

    public void MuteChange(bool mute)
    {
        if (mute)
            volumeImg.sprite = volumeOff;
        else
            volumeImg.sprite = volumeOn;
    }

    private void ChangePoints(int point)
    {
        pointsPlayer = point;
        pointText.text = "Points: " + pointsPlayer.ToString();
        pointsBar.value = pointsPlayer;

        if (pointsBar.maxValue <= pointsPlayer)
            LevelCompleted();
    }

    private void UpdateTimer()
    {
        timerText.text = ((int)levelTimer).ToString();

        if (levelTimer <= 0)
            GameOver();

        levelTimer -= Time.deltaTime;
    }

    public void Restart()
    {
        pointText.text = "Points: 0";
        levelTimer = timeForLevel;
        blurImg.gameObject.SetActive(false);
        gameOverWindow.gameObject.SetActive(false);
        levelCompleteWindow.gameObject.SetActive(false);
        pointsBar.value = 0;

        if (!activeMode)
            ChangePauseMode();

        RestartGameHandler();
    }

    private void GameOver()
    {
        blurImg.gameObject.SetActive(true);
        gameOverWindow.gameObject.SetActive(true);
        gameOverWindow.GetComponentInChildren<Text>().text = pointText.text;
        activeMode = false;
        OnActiveGameHandler(false);
    }

    private void LevelCompleted()
    {
        blurImg.gameObject.SetActive(true);
        levelCompleteWindow.gameObject.SetActive(true);
        levelCompleteWindow.GetComponentInChildren<Text>().text = "Time left: " + ((int)levelTimer).ToString();
        activeMode = false;
        OnActiveGameHandler(false);
    }
}
