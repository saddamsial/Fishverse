using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using LibEngine.Leaderboard;
using Zenject;

public class OpenWorld_Manager : MonoBehaviour
{
    [Inject]
    private ILeaderboardController leaderboardController;

    [SerializeField]
    private MatchModes matchMode;

    public GameObject GameEndPanel;
    [Header("GameEndPanel Text")]
    [SerializeField] TMP_Text ScoreText;
    [SerializeField] TMP_Text TotalFishCaughtText;
    [SerializeField] TMP_Text HighScoreText;
    [SerializeField] Text Username;
    [Header("GameEndPanel Enable/Disable")]
    public GameObject WinningPanel;
    public GameObject LosePanel;
    public GameObject DrawPanel;
    public Image ImgWinLose;
    public List<Sprite> Sprites;
    private float startTime;
    public string timer;
    public int Score = 0;
    public int TotalFishCaught = 0;
    //public int Bonus = 10;
    [SerializeField] float HighestScore;
    private bool game_started = false;
    private bool isLocalPlayerWinner = false;
    // Start is called before the first frame update
    public int highScore = 0;
    public int LowScore = 0;
    private bool GameEnded=false;
    private bool solomode=false;
    public ScoreManager ScoreManager;
    void Start()
    {
        startTime = Time.time;
        GameEndPanel.SetActive(false);
        WinningPanel.SetActive(false);
        LosePanel.SetActive(false);

        // Only the master client starts the game
        StartGame();
        if(PhotonNetwork.CurrentRoom.CustomProperties["GameMap"].ToString() == "Open_World_Solo")
        {
            solomode = true;
        }
    }

    private void StartGame()
    {
        StartCoroutine(SynchronizeStartTime());
    }

    // Update is called once per frame
    void Update()
    {
        if (game_started)
        {
            if(LowScore==0 && Score < highScore)
            {
                LowScore = Score;
            }
            UpdateTimerText();
            if (timer == "00:00" && !GameEnded && !solomode)
            {
                EndGame();
            }

        }
    }
    private void UpdateTimerText()
    {
        if (!solomode)
        {
            float elapsedTime = Time.time - startTime;
            float countdownDuration = 240f; // 4 minutes in seconds

            float remainingTime = Mathf.Max(countdownDuration - elapsedTime, 0f);

            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);

            // Format the timer string with leading zeros
            string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
            timer = timerString;
        }
        else
        {
            // This code will count from 0:00 to infinity
            float elapsedTime = Time.time - startTime;

            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            // Format the timer string with leading zeros
            string timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
            timer = timerString;
        }
    }
    public void Addscore(int coin)
    {
        Score = coin;
    }
    public void AddFishCaught(int FishCount)
    {
        TotalFishCaught = FishCount;
    }

    public void EndGame()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
#endif
        if (ScoreManager != null)
        {
            ScoreManager.EndGame();
        }
        ScoreText.text = Score.ToString();
        TotalFishCaughtText.text = TotalFishCaught.ToString();
        GameEndPanel.SetActive(true);
        Username.text = Fishverse_Core.instance.account_username;
        HighScoreText.text = highScore.ToString();

        MatchResult resultMatch = default;
        if (ScoreManager != null)
        {
            // Determine if the local player has won or lost
            if (ScoreManager.wins == 1)
            {
                WinningPanel.SetActive(true);
                LosePanel.SetActive(false);
                DrawPanel.SetActive(false);
                ImgWinLose.sprite = Sprites[0];

                resultMatch = MatchResult.Win;
            }
            else if (ScoreManager.draws == 1)
            {
                WinningPanel.SetActive(false);
                LosePanel.SetActive(false);
                DrawPanel.SetActive(true);
                ImgWinLose.sprite = Sprites[0];

                resultMatch = MatchResult.Draw;
            }
            else
            {
                WinningPanel.SetActive(false);
                LosePanel.SetActive(true);
                DrawPanel.SetActive(false);
                ImgWinLose.sprite = Sprites[1];

                resultMatch = MatchResult.Lose;
            }
        }
        GameEnded = true;

        var result = new ResultsDataDTO(Score, matchMode, resultMatch);
        leaderboardController.AddMatchResult(result);
    }

    IEnumerator SynchronizeStartTime()
    {
        yield return new WaitForSeconds(1.5f);
        // Notify other players that the game has started
        GameStarted();
    }
    private void GameStarted()
    {
        game_started = true; // Start the game for all players
    }



    public void UpdateHighScore(int newScore)
    {
        if (newScore > highScore)
        {
            highScore = newScore;
           // PlayerPrefs.SetInt("HighScore", highScore);
            SyncHighScore(highScore);
        }
        if (newScore < highScore)
        {
            LowScore = newScore;
        }
    }


    private void SyncHighScore(int newHighScore)
    {
        Debug.Log( "High Score: " + newHighScore) ;
    }

}
