using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Boot,
    MainMenu,
    Loading,
    GameplayReady,
    BossCinematic,
    ActiveCombat,
    VictoryRound,
    DefeatRound,
    Paused
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState CurrentState { get; private set; }

    // Events ให้ class อื่น subscribe ได้ ไม่ต้อง reference GameManager ตรงๆ
    public static event System.Action<GameState> OnGameStateChanged;

    void Awake()
    {
        // Singleton pattern — ถ้ามีอยู่แล้วให้ทำลายตัวใหม่ทิ้ง
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // คงอยู่ข้ามฉาก
    }

    void Start()
    {
        ChangeState(GameState.Boot);
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        HandleStateLogic(newState);
    }

    void HandleStateLogic(GameState state)
    {
        switch (state)
        {
            case GameState.Boot:
                // TODO: โหลด config, database ก่อนเข้าเมนู
                ChangeState(GameState.MainMenu);
                break;

            case GameState.MainMenu:
                // TODO: โหลดฉาก MainMenu
                // SceneManager.LoadScene("MainMenu");
                break;

            case GameState.Loading:
                // TODO: แสดง loading screen
                break;

            case GameState.ActiveCombat:
                Time.timeScale = 1f;
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                break;

            case GameState.VictoryRound:
            case GameState.DefeatRound:
                Time.timeScale = 1f;
                // TODO: แสดง RewardScreenUI หรือ DefeatUI
                break;
        }
    }

    // ให้ UI หรือ class อื่นเรียกใช้ได้ง่าย
    public void GoToMainMenu() => ChangeState(GameState.MainMenu);
    public void StartCombat()  => ChangeState(GameState.ActiveCombat);
    public void PauseGame()    => ChangeState(GameState.Paused);
    public void ResumeGame()   => ChangeState(GameState.ActiveCombat);
    public void PlayerWon()    => ChangeState(GameState.VictoryRound);
    public void PlayerDied()   => ChangeState(GameState.DefeatRound);
}