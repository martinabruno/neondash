using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Pannelli UI")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject skinSelectPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject levelSelectPanel;

    [Header("Elementi HUD Originali")]
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Elementi Win Panel")]
    [SerializeField] private TextMeshProUGUI winStatsText;
    [SerializeField] private Image star1;
    [SerializeField] private Image star2;
    [SerializeField] private Image star3;
    [SerializeField] private Color starActiveColor = new Color(1f, 0.85f, 0f, 1f);
    [SerializeField] private Color starDisabledColor = new Color(0.2f, 0.2f, 0.2f, 0.4f);

    [Header("Soglie Stelle in Secondi")]
    [SerializeField] private float timeFor3Stars = 25f;
    [SerializeField] private float timeFor2Stars = 45f;

    [Header("Bottoni Selezione Livelli")]
    [SerializeField] private Button[] levelButtons;

    public float MoveInput { get; set; } = 0f;
    public bool JumpRequested { get; set; } = false;

    private bool isTouchMoving = false;
    private bool isTouchJumping = false;
    private float currentTimerValue = 0f;
    private int currentCoinsCount = 0;
    private int totalCoinsCount = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        UpdateLevelSelectButtons();
    }

    private void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        if (Mathf.Abs(h) > 0.01f) MoveInput = h;
        else if (!isTouchMoving) MoveInput = 0f;

        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            JumpRequested = true;
        else if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
            if (!isTouchJumping) JumpRequested = false;
    }

    // ==========================================
    // GRAFICA HUD RIPRISTINATA (Cuori e Testo compatto)
    // ==========================================

    public void UpdateLives(int lives)
    {
        if (livesText == null) return;
        livesText.text = "";
        for (int i = 0; i < lives; i++) livesText.text += "♥ ";
    }

    public void UpdateCoins(int coins)
    {
        currentCoinsCount = coins;
        if (coinsText != null) coinsText.text = totalCoinsCount > 0 ? $"COIN: {coins}/{totalCoinsCount}" : $"COIN: {coins}/";
    }

    public void UpdateCoins(int coins, int totalCoins)
    {
        currentCoinsCount = coins;
        totalCoinsCount = totalCoins;
        if (coinsText != null) coinsText.text = totalCoins > 0 ? $"COIN: {coins}/{totalCoins}" : $"COIN: {coins}/";
    }

    public void UpdateCoins(object arg1, object arg2)
    {
        if (int.TryParse(arg1?.ToString(), out int c)) currentCoinsCount = c;
        if (int.TryParse(arg2?.ToString(), out int t)) totalCoinsCount = t;
        if (coinsText != null) coinsText.text = totalCoinsCount > 0 ? $"COIN: {currentCoinsCount}/{totalCoinsCount}" : $"COIN: {arg1}/{arg2}";
    }

    public void UpdateTimer(float timeInSeconds)
    {
        currentTimerValue = timeInSeconds;
        if (timerText == null) return;
        int min = Mathf.FloorToInt(timeInSeconds / 60f);
        int sec = Mathf.FloorToInt(timeInSeconds % 60f);
        int cents = Mathf.FloorToInt((timeInSeconds * 100f) % 100f);
        timerText.text = $"{min:00}:{sec:00}.{cents:00}";
    }

    public void UpdateTimer(string formattedTime)
    {
        if (timerText != null) timerText.text = formattedTime;
    }

    // ==========================================
    // GESTIONE PANNELLI E VITTORIA
    // ==========================================

    public void ShowHUD() => ShowHUD(true);
    public void ShowHUD(object parameter) => ShowHUD(true);

    public void ShowHUD(bool show)
    {
        HideAllPopups();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(!show);
        if (hudPanel != null) hudPanel.SetActive(show);
    }

    public void HideAllPopups()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (skinSelectPanel != null) skinSelectPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    }

    public void ShowLevelComplete()
    {
        int currentLvl = PlayerPrefs.GetInt("CurrentLevel", 1);
        ShowWin(currentLvl, currentTimerValue, currentCoinsCount);
    }

    public void ShowLevelComplete(object p1, object p2, object p3, object p4, object p5)
    {
        int currentLvl = PlayerPrefs.GetInt("CurrentLevel", 1);
        ShowWin(currentLvl, currentTimerValue, currentCoinsCount);
    }

    public void ShowWin(int currentLevel, float finalTime, int coinsCollected)
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(true);
        Time.timeScale = 0f;

        int stars = 1;
        if (finalTime > 0f)
        {
            if (finalTime <= timeFor3Stars) stars = 3;
            else if (finalTime <= timeFor2Stars) stars = 2;
        }

        if (star1 != null) star1.color = (stars >= 1) ? starActiveColor : starDisabledColor;
        if (star2 != null) star2.color = (stars >= 2) ? starActiveColor : starDisabledColor;
        if (star3 != null) star3.color = (stars >= 3) ? starActiveColor : starDisabledColor;

        int min = Mathf.FloorToInt(finalTime / 60f);
        int sec = Mathf.FloorToInt(finalTime % 60f);
        int cents = Mathf.FloorToInt((finalTime * 100f) % 100f);
        if (winStatsText != null) winStatsText.text = $"COIN: {coinsCollected} | TIME: {min:00}:{sec:00}.{cents:00}";

        string starKey = $"Level_{currentLevel}_Stars";
        if (stars > PlayerPrefs.GetInt(starKey, 0)) PlayerPrefs.SetInt(starKey, stars);

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (currentLevel >= unlockedLevel && unlockedLevel < 12)
        {
            PlayerPrefs.SetInt("UnlockedLevel", currentLevel + 1);
        }
        PlayerPrefs.Save();
    }

    public void ShowGameOver()
    {
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ShowGameOverScreen(params object[] args) => ShowGameOver();

    // ==========================================
    // PROGRESSIONE & SELEZIONE LIVELLI
    // ==========================================

    public void UpdateLevelSelectButtons()
    {
        if (levelButtons == null || levelButtons.Length == 0) return;

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        
        // Colori: Azzurro per i livelli sbloccati, Grigio scuro per quelli bloccati
        Color activeCyan = new Color(0f, 0.898f, 1f, 1f);           // #00E5FF
        Color lockedDark = new Color(0.035f, 0.051f, 0.086f, 1f);   // #090D16

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNum = i + 1;
            bool isUnlocked = (levelNum <= unlockedLevel);

            levelButtons[i].interactable = isUnlocked;

            // Impedisce al bottone di sbiadire automaticamente quando è disabilitato
            ColorBlock cb = levelButtons[i].colors;
            cb.disabledColor = lockedDark;
            levelButtons[i].colors = cb;

            Image btnImage = levelButtons[i].GetComponent<Image>();
            TextMeshProUGUI btnText = levelButtons[i].GetComponentInChildren<TextMeshProUGUI>();

            if (isUnlocked)
            {
                if (btnImage != null) btnImage.color = activeCyan; // Sfondo azzurro
                if (btnText != null)
                {
                    btnText.color = Color.black; // Testo numero nero
                    btnText.text = levelNum.ToString();
                }
            }
            else
            {
                if (btnImage != null) btnImage.color = lockedDark; // Sfondo grigio
                if (btnText != null)
                {
                    btnText.color = new Color(0f, 0.898f, 1f, 1f); // Testo chiaro
                    btnText.text = "X"; // X sicura e perfettamente compatibile con qualsiasi font
                }
            }
        }
    }

    public void OnClick_SelectSpecificLevel(int levelNumber)
    {
        int unlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (levelNumber > unlocked) return;

        HideAllPopups();
        
        GameManager gm = GameManager.Instance != null ? GameManager.Instance : Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.StartLevel(levelNumber);
        }
    }

    public void OnClick_OpenLevelSelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
        UpdateLevelSelectButtons();
    }

    public void OnClick_CloseLevelSelect()
    {
        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OnClick_Retry()
    {
        Time.timeScale = 1f;
        HideAllPopups();
        GameManager gm = GameManager.Instance != null ? GameManager.Instance : Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.StartGame(); // Riavvia istantaneamente senza ricaricare la scena
        }
    }

    public void OnClick_HomeFromGameOver()
    {
        Time.timeScale = 1f;
        HideAllPopups();
        GameManager gm = GameManager.Instance != null ? GameManager.Instance : Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.GoToMainMenu();
        }
    }

    public void OnClick_NextLevel()
    {
        Time.timeScale = 1f;
        HideAllPopups(); // Fondamentale per sbloccare i click!
        
        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        int nextLevel = Mathf.Min(currentLevel + 1, 12);
        
        GameManager gm = GameManager.Instance != null ? GameManager.Instance : Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.StartLevel(nextLevel);
        }
    }

    public void OnClick_HomeFromWin()
    {
        Time.timeScale = 1f;
        HideAllPopups();
        GameManager gm = GameManager.Instance != null ? GameManager.Instance : Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.GoToMainMenu();
        }
    }

    // Supporto Touch HUD
    public void PointerDown_MoveLeft() { MoveInput = -1f; isTouchMoving = true; }
    public void PointerDown_MoveRight() { MoveInput = 1f; isTouchMoving = true; }
    public void PointerUp_Move() { MoveInput = 0f; isTouchMoving = false; }
    public void PointerDown_Jump() { JumpRequested = true; isTouchJumping = true; }
    public void PointerUp_Jump() { JumpRequested = false; isTouchJumping = false; }

    [ContextMenu("Reset Tutti i Progressi")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("UnlockedLevel");
        for (int i = 1; i <= 12; i++) PlayerPrefs.DeleteKey($"Level_{i}_Stars");
        PlayerPrefs.SetInt("UnlockedLevel", 1);
        PlayerPrefs.Save();
        UpdateLevelSelectButtons();
    }
}