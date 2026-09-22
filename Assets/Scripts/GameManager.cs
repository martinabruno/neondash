using UnityEngine;

public enum GamePhase { Ready, Playing, GameOver, Won }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    // Lasciata per evitare errori in Console nel caso in cui UIManager cerchi ancora di leggerla
    public static bool isFirstBoot = true; 

    [Header("Riferimenti Scena")]
    [SerializeField] private PlayerController player; 
    [SerializeField] private LevelGeneratorTilemap generator; 
    [SerializeField] private Camera mainCamera;

    [Header("Inquadratura Camera Mobile")]
    [SerializeField] private float cameraOffsetY = 0f;
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Selezione Livello (Editor)")]
    [SerializeField] private int startLevel = 1;

    [Header("Stato Partita")]
    [SerializeField] private int currentLevelId = 1;
    [SerializeField] private GamePhase phase = GamePhase.Ready;

    private int currentLives = 3;
    private int currentCoins = 0;
    private float levelTimer = 0f;
    private bool isTimerRunning = false;

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f; // SBLOCCO TEMPO: Previene qualsiasi freeze (non cliccabile) tra le schermate
    }

    private void Start()
    {
        // 1. All'avvio recupera dalla memoria l'ultimo livello giocato/selezionato
        currentLevelId = PlayerPrefs.GetInt("CurrentLevel", startLevel);
        
        // 2. Forza SEMPRE il ritorno all'Homepage (Risolve il problema del salto diretto nel livello)
        // GoToMainMenu();
    }

    // ==========================================
    // LA TUA OPZIONE B: Ritorno in base con aggiornamento del testo
    // ==========================================
    public void GoToMainMenu()
    {
        phase = GamePhase.Ready;
        Time.timeScale = 1f; // Sblocca i bottoni

        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Sovrascrive qualsiasi comando dell'UIManager per forzare la vista sull'Homepage
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideAllPopups();
            UIManager.Instance.ShowHUD(false);
        }

        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(true);
            UpdateMenuText();
        }
    }

    private void UpdateMenuText()
    {
        if (mainMenuPanel != null)
        {
            // Cerca il bottone con la scritta START e aggiunge il livello corrente dinamicamente
            TMPro.TextMeshProUGUI[] texts = mainMenuPanel.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                if (t != null && (t.text.ToUpper().Contains("START") || t.text.ToUpper().Contains("LIV.")))
                {
                    t.text = $"START LIV. {currentLevelId}";
                    break; // Modifica solo la scritta del bottone principale
                }
            }
        }
    }

    // Chiamato quando scegli un livello dal menu a griglia
    public void StartLevel(int levelId)
    {
        currentLevelId = levelId;
        PlayerPrefs.SetInt("CurrentLevel", currentLevelId);
        PlayerPrefs.Save();
        
        // Applica l'Opzione B: Ritorna alla Home aggiornando il testo del pulsante
        GoToMainMenu();
    }

    // ==========================================
    // AVVIO REALE DELLA PARTITA (Bottone START)
    // ==========================================
    public void StartGame()
    {
        phase = GamePhase.Playing;
        Time.timeScale = 1f;
        isTimerRunning = true;
        currentLives = 3;
        currentCoins = 0;
        levelTimer = 0f;

        // Si assicura di caricare il livello esatto scritto sul bottone
        currentLevelId = PlayerPrefs.GetInt("CurrentLevel", startLevel);
        TriggerLevelGeneration(currentLevelId);

        if (player != null && generator != null)
        {
            player.gameObject.SetActive(true);
            player.transform.position = new Vector3(generator.SpawnPosition.x + 0.5f, generator.SpawnPosition.y + 0.5f, 0f);
            
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
            }
        }

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowHUD(true);
            UIManager.Instance.UpdateLives(currentLives);
            
            int total = generator != null ? generator.TotalCoins : 0;
            UIManager.Instance.UpdateCoins(currentCoins, total);
            UIManager.Instance.UpdateTimer(0f);
        }
    }

    private void TriggerLevelGeneration(int levelId)
    {
        if (generator == null) return;

        LevelDef defToLoad = null;
        foreach (var lvl in LevelDatabase.Levels)
        {
            if (lvl.id == levelId)
            {
                defToLoad = lvl;
                break;
            }
        }

        if (defToLoad != null)
        {
            generator.BuildLevel(defToLoad);
        }
    }

    private void Update()
    {
        if (phase == GamePhase.Playing)
        {
            if (isTimerRunning)
            {
                levelTimer += Time.deltaTime;
                if (UIManager.Instance != null) UIManager.Instance.UpdateTimer(levelTimer);
            }

            if (player != null)
            {
                GameObject goal = GameObject.Find("Goal(Clone)"); 
                if (goal == null) goal = GameObject.FindGameObjectWithTag("Finish");
                if (goal == null) goal = GameObject.FindGameObjectWithTag("Goal");

                if (goal != null)
                {
                    if (Vector2.Distance(player.transform.position, goal.transform.position) < 0.8f)
                    {
                        CompleteLevel();
                    }
                }
            }
        }
    }

    private void LateUpdate()
    {
        if (mainCamera != null && player != null && phase == GamePhase.Playing)
        {
            Vector3 pos = mainCamera.transform.position;
            pos.x = player.transform.position.x;
            pos.y = player.transform.position.y + cameraOffsetY;
            pos.z = -10f; 
            mainCamera.transform.position = pos;
        }
    }

    // ==========================================
    // AZIONI DI GIOCO 
    // ==========================================
    public void OnPlayerDied()
    {
        if (phase != GamePhase.Playing) return;
        currentLives--;
        if (UIManager.Instance != null) UIManager.Instance.UpdateLives(currentLives);

        PlayAudio("PlayDamageSound");

        if (currentLives <= 0)
        {
            GameOver();
        }
        else
        {
            if (player != null && generator != null)
            {
                player.transform.position = new Vector3(generator.SpawnPosition.x + 0.5f, generator.SpawnPosition.y + 0.5f, 0f);
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = Vector2.zero;
            }
        }
    }

    public void OnCoinCollected() 
    {
        if (phase != GamePhase.Playing) return;
        currentCoins++;
        int total = generator != null ? generator.TotalCoins : 0;
        if (UIManager.Instance != null) UIManager.Instance.UpdateCoins(currentCoins, total);
        PlayAudio("PlayCoinSound");
    }

    public void OnCoinCollected(int amount) 
    {
        if (phase != GamePhase.Playing) return;
        currentCoins += amount;
        int total = generator != null ? generator.TotalCoins : 0;
        if (UIManager.Instance != null) UIManager.Instance.UpdateCoins(currentCoins, total);
        PlayAudio("PlayCoinSound");
    }

    public void OnFlagReached() => CompleteLevel();
    public void ReachFlag() => CompleteLevel();
    public void ShowLevelComplete() => CompleteLevel();

    public void CompleteLevel()
    {
        if (phase != GamePhase.Playing) return;
        phase = GamePhase.Won;
        isTimerRunning = false;

        PlayAudio("PlayWinSound");

        if (UIManager.Instance != null)
        {
            int total = generator != null ? generator.TotalCoins : 0;
            UIManager.Instance.ShowLevelComplete(currentLevelId, levelTimer, currentCoins, total, currentLives);
        }
    }

    public void GameOver()
    {
        if (phase == GamePhase.GameOver) return;
        phase = GamePhase.GameOver;
        isTimerRunning = false;

        PlayAudio("PlayGameOverSound");
        if (UIManager.Instance != null) UIManager.Instance.ShowGameOverScreen();
    }

    private void PlayAudio(string soundMethod)
    {
        // Controlla che l'istanza globale di RetroAudio esista
        if (RetroAudio.Instance != null)
        {
            // Chiama direttamente i metodi ufficiali definiti in RetroAudio.cs
            if (soundMethod.Contains("Coin"))
            {
                RetroAudio.Instance.PlayCoin();
            }
            else if (soundMethod.Contains("Damage") || soundMethod.Contains("Die") || soundMethod.Contains("GameOver"))
            {
                RetroAudio.Instance.PlayDie(); // Chiama il suono della morte/danno
            }
            else if (soundMethod.Contains("Win"))
            {
                RetroAudio.Instance.PlayWin(); // Chiama la sequenza sonora della vittoria
            }
        }
    }
}