using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Photon.Realtime;
using UnityEngine.Events;
using Photon.Pun;
// using Mirror;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using LibEngine.Leaderboard;
using Zenject;

public class MiniGame_Manager : MonoBehaviourPunCallbacks
{
    [Inject]
    private ILeaderboardController leaderboardController;

    [SerializeField]
    private MatchModes matchModes;

    [Header("Scene Refs:")]
    public ArcadeVehicleController vehicle_controller;
    public ArcadeVehicleNitro vehicle_nitro;
    public GameObject panel_game_started;
    public GameObject camera_spectator;
    public GameObject panel_score;
    public GameObject panel_pos;
    public GameObject panel_fishes;
    public GameObject panel_time;
    public GameObject panel_add_score;
    public GameObject joystick;
    public GameObject btn_boost;
    public GameObject btn_pause;

    [Space(10)]
    [Header("Texts Refs:")]
    public TMP_Text text_score;
    public TMP_Text text_time;
    public TMP_Text text_fishes;
    public TMP_Text text_position;
    public GameObject BonusPanel;
    public TMP_Text text_bonus;
    public GameObject TimerPanel;
    public TMP_Text text_Timer;

    [Header("Ending Screen")]
    public GameObject panel_end_screen;
    public Text Text_Username;
    public TMP_Text text_best_score;
    public TMP_Text text_ending_score;
    public TMP_Text text_time_played;
    public TMP_Text text_scoretime_played;
    public TMP_Text text_coins;
    public Text text_CurrentScore;
    [Header("GameEndPanel Enable/Disable")]
    public GameObject WinningPanel;
    public GameObject LosePanel;
    public GameObject DrawPanel;
    public Image ImgWinLose;
    public List<Sprite> Sprites;

    [Space(10)]
    [Header("Game Data:")]
    public int score;
    public int ScoreCoins;
    public int best_score=0;
    public int LowScore ;
    public float HighTimePlayed ;
    public int fishes;
    public int start_time = 90;
    public int time = 90;
    public bool start_instant = false;
    public bool unlimitedTime;
    public int maxFish = 20;
    public GameObject[] all_fishes;

    [Space(10)]
    [Header("Bonuses:")]
    public List<MiniGame_Bonus> all_bonuses;

    [Space(10)]
    [Header("Events:")]
    public UnityEvent on_catch_fish;
    public UnityEvent on_remove_fish;
    public UnityEvent onMaxFish;

    [Space(10)]
    [Header("Boat Details:")]
    public TMP_Text BoatVitesse;
    public TMP_Text BoatGear;
    public Slider Fuel;


    [Space]
    [SerializeField] float gameStartCameraWait = 3f;
    [SerializeField] float gameStartPanelWait = 2f;
    [SerializeField] float gameStartUIWait = .5f;
    [SerializeField] float gameStartControlsWait = .5f;

    private bool game_started = false;
    public bool gameLeaderBoard = false;
    private float timer = 0;
    private int scoretime_played = 0;
    private int fishes_score_combo = 0;
    private float gameStartTime;
    public float timePlayed;

    //Text Animations
    DOTweenAnimation text_score_anim;
    DOTweenAnimation text_fish_anim;
    DOTweenAnimation text_pos_anim;

    public int playerToStart = 2;

    private PhotonView photon_view;
    private float speedboat;
    private float _fuel;
    [SerializeField] RoomManager owner;
    private bool isLocalPlayerWinner = false;
    public Dictionary<int, int> playerScores = new Dictionary<int, int>(); // Maps player IDs to their scores

    private bool IsEndedGame = false;
    bool Engine_started = false;
    float delayTimer = 4f;

    private void Start()
    {
        photon_view = GetComponent<PhotonView>();

#if !UNITY_EDITOR
        unlimitedTime = false;
#endif
        //Init Components
        gameStartTime = Time.time;
        BonusPanel.SetActive(false);
        TimerPanel.SetActive(false);
        text_score_anim = text_score.GetComponent<DOTweenAnimation>();
        text_fish_anim = text_fishes.GetComponent<DOTweenAnimation>();
        if (text_position && text_position.TryGetComponent<DOTweenAnimation>(out DOTweenAnimation anm))
            text_pos_anim = anm;

        if (Fishverse_Core.instance)
        // GetComponent<MiniGameServer_API>().GetBestScore();

        //Set FPS
        Application.targetFrameRate = 60;
       // GetComponent<MiniGameServer_API>().SubmitScore(0);
        //Set Default Parameters
        fishes = 0;
        score = 0;
        ScoreCoins = 0;
        time = start_time;

        //Shuffle Bonuses

        all_bonuses = FindObjectsOfType<MiniGame_Bonus>().ToList();
        all_bonuses.Shuffle(60);

        foreach (var bonus in all_bonuses)
        {
            bonus.SetActive(false);
        }

        for (int i = 0; i < all_bonuses.Count; i++)
        {
            all_bonuses[i].SetActive(true);
        }

        RefreshTexts_UI();

        StartGame();
    }

    public void StartGame()
    {
        gameLeaderBoard = true;
        StartCoroutine(StartGame(start_instant));
    }
    IEnumerator StartGame(bool instant)
    {
        if (instant)
        {
            yield return new WaitForSeconds(0.01f);
            camera_spectator.SetActive(false);
            panel_game_started.SetActive(true);
            panel_score.SetActive(true);
            panel_time.SetActive(true);
            panel_fishes.SetActive(true);

#if UNITY_STANDALONE_WIN
            joystick.SetActive(true);
            btn_boost.SetActive(true);
#endif

            btn_pause.SetActive(true);
            game_started = true;
        }
        else
        {
            yield return new WaitForSeconds(gameStartCameraWait);
            camera_spectator.SetActive(false);

            yield return new WaitForSeconds(gameStartPanelWait);
            panel_game_started.SetActive(true);

            yield return new WaitForSeconds(gameStartUIWait);
            panel_score.SetActive(true);
            panel_time.SetActive(true);
            panel_fishes.SetActive(true);
            if (panel_pos) panel_pos.SetActive(true);

            yield return new WaitForSeconds(gameStartControlsWait);
            if (owner.boatController.mobileInput)
            {
                joystick.SetActive(true);
            }
            btn_boost.SetActive(true);
            btn_pause.SetActive(true);

            game_started = true;
        }
    }

    private void Update()
    {
        if (!Engine_started && panel_game_started.activeSelf)
        {
            delayTimer -= Time.deltaTime;
            owner.boatController.enabled = false;
            if (delayTimer <= 0f)
            {
                Enginestarted();
            }
        }
        else if(!panel_game_started.activeSelf)
        {
            owner.boatController.enabled = false;
        }

        if (game_started)
        {
            /*foreach (var kvp in playerScores)
            {
                Debug.Log($"Player ID: {kvp.Key}, Score: {kvp.Value}");
            }*/
            speedboat = owner.boatController.carVelocity.z;
            _fuel = owner.boatController.fuel;

            timer += Time.deltaTime;
            if (timer > 1f)
            {
                timer = 0;
                time = Mathf.Clamp(time - 1, 0, 999);
                RefreshTexts_UI();

                if ((time == 0 && !unlimitedTime) || _fuel == 0)
                {
                    EndGame();
                }
            }
             timePlayed = Mathf.Clamp(Time.time - gameStartTime, 0, float.MaxValue);
            // Update the text_time_played UI element
            text_time_played.text = ((int)timePlayed).ToString();

            if (Mathf.RoundToInt(speedboat) > 0)
            {
                BoatVitesse.text = Mathf.RoundToInt(speedboat).ToString();
            }
            else
            {
                BoatVitesse.text = Mathf.RoundToInt(-speedboat).ToString();
            }
            //Debug.Log("HighTimePlayed"+HighTimePlayed);
            Fuel.value = _fuel / 100;
            text_CurrentScore.text = score.ToString();
          //  Debug.Log("avatar" + Fishverse_Core.instance.avatar);

        }
    }

    public void EndGame()
    {
        if (IsEndedGame)
            return;

        IsEndedGame = true;

        game_started = false;
        gameLeaderBoard = false;
        if (vehicle_controller)
            vehicle_controller.gameObject.SetActive(false);
        // End Game Game Panel Texts
        Text_Username.text = Fishverse_Core.instance.account_username.ToString();
        text_coins.text = ScoreCoins.ToString();
        text_scoretime_played.text = scoretime_played.ToString();
        text_ending_score.text = score.ToString();
        
        panel_end_screen.SetActive(true);
        if (score > best_score)
        {
           // best_score = score;
            GetComponent<MiniGameServer_API>().SubmitScore(best_score);
        }

        text_best_score.text = best_score.ToString();

        isLocalPlayerWinner = (score >= best_score && score > LowScore ) || PhotonNetwork.CurrentRoom.PlayerCount == 1 ;
        MatchResult resultMatch = default;

        if (isLocalPlayerWinner)
        {
            //WIN
            WinningPanel.SetActive(true);
            LosePanel.SetActive(false);
            DrawPanel.SetActive(false);
            ImgWinLose.sprite = Sprites[0];

            resultMatch = MatchResult.Win;
        }
        else if (best_score == LowScore && score == best_score && LowScore>=0 )
        {
            //DRAW
            WinningPanel.SetActive(false);
            LosePanel.SetActive(false);
            DrawPanel.SetActive(true);
            ImgWinLose.sprite = Sprites[0];

            resultMatch = MatchResult.Draw;
        }
        else
        {
            //LOSE
            WinningPanel.SetActive(false);
            LosePanel.SetActive(true);
            DrawPanel.SetActive(false);
            ImgWinLose.sprite = Sprites[1];

            resultMatch = MatchResult.Lose;
        }

        var result = new ResultsDataDTO(score, matchModes, resultMatch);
        leaderboardController.AddMatchResult(result);
    }

    public void UpdateHighScore(int newScore , float time)
    {
       
        if (newScore > best_score)
        {
            best_score = newScore;
        }
        if(time>= timePlayed)
        {
            HighTimePlayed = time;
        }
        LowScore = playerScores.Count > 0 ? playerScores.Values.Min() : 0;

    }
    // Add a method to add or update a player's score in the dictionary
    public void UpdatePlayerScore(int playerId, int playerScore)
    {
        if (playerScores.ContainsKey(playerId))
        {
            playerScores[playerId] = playerScore; // Update the existing score
        }
        else
        {
            playerScores.Add(playerId, playerScore); // Add a new player and score
        }
    }
  
    public void AddFish()
    {
        if (fishes < maxFish)
        {
            fishes += 1;

            if (all_fishes[fishes - 1])
            {
                all_fishes[fishes - 1].SetActive(true);
            }

            on_catch_fish.Invoke();

            if (fishes >= maxFish)
                onMaxFish.Invoke();

            RefreshTexts_UI();

            //Play texts animations

            if (text_fish_anim)
            {
                text_fish_anim.DORestart();
            }
        }
    }

    public void AddScore(bool remove_fish = false)
    {
        //Convert fishes to score

        if (remove_fish)
        {
            if (all_fishes[Mathf.Max(fishes - 1, 0)])
            {
                all_fishes[Mathf.Max(fishes - 1, 0)].SetActive(false);
            }

            fishes--;
            fishes = Mathf.Clamp(fishes, 0, 9999);

            on_remove_fish.Invoke();
        }

        score += (10 + fishes_score_combo);

        if (fishes > 0)
        {
            fishes_score_combo++;
        }
        else
        {
            fishes_score_combo = 0;
        }

        RefreshTexts_UI();


        //Play texts animations

        if (text_score_anim)
        {
            text_score_anim.DORestart();
        }

        if (text_fish_anim && remove_fish)
        {
            text_fish_anim.DORestart();
        }
    }

    public void AddBonus(MiniGame_Bonus.BonusType bonus_type, GameObject target)
    {


        if (bonus_type == MiniGame_Bonus.BonusType.Time)
        {
            scoretime_played += 20;
            time += 20;
            score += 20 ;
            BonusPanel.SetActive(false);
            TimerPanel.SetActive(true);
            text_Timer.text = "+20 Seconds";
        }

        else if (bonus_type == MiniGame_Bonus.BonusType.Coins)
        {
            score += 15;
            ScoreCoins += 15;
            BonusPanel.SetActive(true);
            TimerPanel.SetActive(false);
            text_bonus.text = "+15 Points";
        }

        else if (bonus_type == MiniGame_Bonus.BonusType.CoinsBig)
        {
            score += 35;
            ScoreCoins += 35;
            BonusPanel.SetActive(true);
            TimerPanel.SetActive(false);
            text_bonus.text = "+35 Points";
        }

        else if (bonus_type == MiniGame_Bonus.BonusType.Nitro)
        {
            vehicle_nitro = target.GetComponent<ArcadeVehicleNitro>();
            vehicle_nitro.nitro = 1;
            vehicle_nitro.RefreshNitroUI();

            text_bonus.text = "Nitro Refilled";
        }
        else if (bonus_type == MiniGame_Bonus.BonusType.Fuel)
        {
            vehicle_controller = target.GetComponent<ArcadeVehicleController>();
            vehicle_controller.Refuel(20);
            text_bonus.text = "+20 Fuel";
        }

        panel_add_score.SetActive(false);
        panel_add_score.SetActive(true);

        RefreshTexts_UI();
    }

    public void RefreshTexts_UI()
    {
        text_score.text = score.ToString();
        text_time.text = time.ToString();

        if (fishes < maxFish)
        {
            text_fishes.color = Color.white;
            text_fishes.text = fishes.ToString() + $"/{maxFish}";
        }
        else
        {
            text_fishes.color = Color.white;
            text_fishes.text = fishes.ToString() + $"/{maxFish}";
        }
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("Player {0} left room", otherPlayer.NickName);

        // Remove the player's score from the dictionary
        if (playerScores.ContainsKey(otherPlayer.ActorNumber))
        {
            playerScores.Remove(otherPlayer.ActorNumber);
        }
    }
    void Enginestarted()
    {
        Engine_started = true;

        // Enable the boat controller
        owner.boatController.enabled = true;
    }
}
