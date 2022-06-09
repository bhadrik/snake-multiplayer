using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    
    public const byte GAME_START_EVENT = 53;
    public const byte GAME_OVER = 23;

    private Snake player;
    private GameObject food;

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject foodPrefab;


    [Header("UI")]
    [SerializeField] private GameObject gameOverPopup;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject menuCanvas;
    [SerializeField] private Transform[] startPositions;

    
    [Header("Multiplayer")]
    [SerializeField] private GameObject multiplayerGameOverPanel;
    [SerializeField] private GameObject multiplayerScoreGroup;
    [SerializeField] private GameObject singlePlayerScore;
    [SerializeField] private TextMeshProUGUI hostScoreText;
    [SerializeField] private TextMeshProUGUI otherScoreText;
    [SerializeField] private TextMeshProUGUI hostScoreText_end;
    [SerializeField] private TextMeshProUGUI otherScoreText_end;
    [SerializeField] private GameObject win;
    [SerializeField] private GameObject lost;
    
    private int myScore;
    private int otherScore;


    [Header("Food")]
    [Tooltip("x: min and y: max")]
    [SerializeField] Vector2 LevelRange_x = new Vector2(-8, 8);
    [SerializeField] Vector2 LevelRange_y = new Vector2(-4, 4);


    int highestScore;
    int score;

    public bool isMultiplayer = true;


    private void Awake() {

        Time.timeScale = 1;

        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(this.gameObject);
        }

        highestScore = PlayerPrefs.GetInt("snake_score");
        highScoreText.text = "High Score: " + highestScore.ToString();

        score = -1;
        ScoreUpdate();

        PhotonNetwork.NetworkingClient.EventReceived += OnNetworkStartEvent;
    }

    private void ScoreUpdate()
    {
        score++;
        scoreText.text = score.ToString();

        if(score > highestScore){
            highestScore = score;
            highScoreText.text = "High Score: " + highestScore.ToString();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over");
        gameOverPopup.SetActive(true);
        gameOverPopup.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = "Score: " + score.ToString();
        PlayerPrefs.SetInt("snake_score", highestScore);
        Time.timeScale = 0;
    }

    public void RestartGame(){
        if(isMultiplayer)
        NetworkManager.Instance.Disconnect();
        SceneManager.LoadSceneAsync(0);
    }


    // When host hit Start button
    public void StartGame(){
        Debug.Log("Click on start button");
        PhotonNetwork.RaiseEvent(GAME_START_EVENT, null, RaiseEventOptions.Default, SendOptions.SendUnreliable);
        OnStartGame();
    }

    private void OnStartGame()
    {
        Debug.Log("Start game now");
        menuCanvas.SetActive(false);

        singlePlayerScore.SetActive(false);
        multiplayerScoreGroup.SetActive(true);

        Transform t;
        if(NetworkManager.Instance.isHost)
        t = startPositions[0];
        else
        t = startPositions[1];
        
        player = PhotonNetwork.Instantiate(playerPrefab.name, t.position, t.rotation).GetComponent<Snake>();
        player.onHitDanger.AddListener(GameOver);
        player.onFoodEat.AddListener(ScoreUpdate);
        player.onFoodEat_m.AddListener(MultiplayerScoreUpdate);

        if(NetworkManager.Instance.isHost){
            food = PhotonNetwork.Instantiate(foodPrefab.name, Vector3.zero, Quaternion.identity);
            RandomizeFood();
        }
    }

    public void MultiplayerScoreUpdate(PhotonView view){
        if(view.IsMine){
            myScore++;
            hostScoreText.text = "My Score: " + myScore.ToString();

            if(myScore > highestScore){
                highestScore = myScore;
                highScoreText.text = "High Score: " + highestScore.ToString();
            }
        }
        else{
            otherScore++;
            otherScoreText.text = "Other Score" + otherScore.ToString();
        }
    }

    public void RandomizeFood() {
        if(!NetworkManager.Instance.isHost && isMultiplayer) return;

        food.transform.position = new Vector2(
            UnityEngine.Random.Range(LevelRange_x.x, LevelRange_x.y),
            UnityEngine.Random.Range(LevelRange_y.x, LevelRange_y.y));
    }

    private void OnNetworkStartEvent(EventData obj)
    {
        if (obj.Code == GAME_START_EVENT){
            OnStartGame();
        }
        else if (obj.Code == GAME_OVER){
            OnMultiplayerOver();
        }
    }

    public void OnMultiplayerOver()
    {
        multiplayerGameOverPanel.SetActive(true);

        hostScoreText_end.text = myScore.ToString();
        otherScoreText_end.text = otherScore.ToString();

        if(myScore > otherScore){
            //Win text
            win.SetActive(true);
        }
        else{
            //Loose text
            lost.SetActive(true);
        }

        PlayerPrefs.SetInt("snake_score", highestScore);
        Time.timeScale = 0;
    }

    public void StartSinglePlayer(){
        isMultiplayer = false;
        singlePlayerScore.SetActive(true);
        multiplayerScoreGroup.SetActive(false);
        player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity).GetComponent<Snake>();
        menuCanvas.SetActive(false);
        player.onHitDanger.AddListener(GameOver);
        player.onFoodEat.AddListener(ScoreUpdate);

        food = Instantiate(foodPrefab, Vector3.right * 8, Quaternion.identity);
        RandomizeFood();
    }
}
