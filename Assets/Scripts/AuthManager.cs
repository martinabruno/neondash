using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase;
using Firebase.Auth;

public class AuthManager : MonoBehaviour
{
    [Header("Input UI")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private GameObject resendVerificationButton;

    [Header("Pannelli UI")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject mainMenuPanel;

    private FirebaseAuth auth;
    private FirebaseUser user;

        private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        
        // Pulisce il testo di stato all'avvio
        if (statusText != null) 
            statusText.text = "";

        // Disattiva il pulsante di invio email all'avvio
        if (resendVerificationButton != null) 
            resendVerificationButton.SetActive(false);
    }


    private void Start()
    {
        // Controlla se Firebase ha già una sessione attiva salvata sul telefono
        if (auth != null && auth.CurrentUser != null)
        {
            user = auth.CurrentUser;
            
            // Verifica che l'utente abbia confermato l'email
            if (user.IsEmailVerified)
            {
                // Utente già loggato! Saltiamo il login e apriamo direttamente il gioco
                if (loginPanel != null) loginPanel.SetActive(false);
                
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GoToMainMenu();
                }

                // Fai partire la musica di sottofondo automaticamente
                AudioSource musicSource = GetComponent<AudioSource>();
                if (musicSource != null && !musicSource.isPlaying)
                {
                    musicSource.Play();
                }
                return;
            }
        }

    // Se non c'è nessun utente salvato, mostra normalmente il pannello di login
    if (loginPanel != null) loginPanel.SetActive(true);
}


    // REGISTRAZIONE + INVIO EMAIL DI CONFERMA
    public async void OnClick_Register()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("Compila tutti i campi!", Color.red);
            return;
        }

        SetStatus("Creazione account in corso...", Color.white);

        try
        {
            var authResult = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            user = authResult.User;

            if (user != null)
            {
                // Invia la mail di verifica ufficiale
                await user.SendEmailVerificationAsync();
                SetStatus("Registrazione riuscita! Controlla la tua email per verificare l'account.", Color.cyan);
                if (resendVerificationButton != null) 
                    resendVerificationButton.SetActive(true);
            }
        }
        catch (FirebaseException ex)
        {
            SetStatus($"Errore registrazione: {ex.Message}", Color.red);
        }
    }

    // LOGIN CON CONTROLLO STATO VERIFICA
    public async void OnClick_Login()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            SetStatus("Compila tutti i campi!", Color.red);
            return;
        }

        SetStatus("Accesso in corso...", Color.white);

        try
        {
            var authResult = await auth.SignInWithEmailAndPasswordAsync(email, password);
            user = authResult.User;

            // Aggiorna lo stato per verificare se l'utente ha cliccato il link nella mail
            await user.ReloadAsync();

            if (user.IsEmailVerified)
            {
                SetStatus("Accesso confermato!", Color.green);
                loginPanel.SetActive(false);
                
                // Invece di attivare solo il pannello, diciamo al GameManager di avviare la Home correttamente
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GoToMainMenu();
                }
                else
                {
                    mainMenuPanel.SetActive(true); // Fallimento di sicurezza nel caso il GameManager non risponda
                }
            }
            else
            {
                SetStatus("Email non verificata. Clicca sul link inviato alla tua casella di posta.", Color.yellow);
                if (resendVerificationButton != null) 
                    resendVerificationButton.SetActive(true);
            }
        }
        catch (FirebaseException ex)
        {
            SetStatus($"Credenziali errate: {ex.Message}", Color.red);
        }
    }

    // REINVIA EMAIL DI VERIFICA SE SMARRITA
    public async void OnClick_ResendVerification()
    {
        if (user != null)
        {
            try
            {
                await user.SendEmailVerificationAsync();
                SetStatus("Nuova email di verifica inviata!", Color.cyan);
            }
            catch (FirebaseException ex)
            {
                SetStatus($"Errore invio email: {ex.Message}", Color.red);
            }
        }
    }

    // RECUPERO PASSWORD (invia email per reimpostarla)
    public async void OnClick_ForgotPassword()
    {
        string email = emailInput.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            SetStatus("Inserisci la tua email prima di procedere.", Color.red);
            return;
        }

        SetStatus("Invio email di recupero in corso...", Color.white);

        try
        {
            await auth.SendPasswordResetEmailAsync(email);
            SetStatus("Controlla la tua email per reimpostare la password!", Color.cyan);
        }
        catch (FirebaseException ex)
        {
            SetStatus($"Errore invio email: {ex.Message}", Color.red);
        }
    }


    public void OnClick_Logout()
    {
        Debug.Log("[LOGOUT] Metodo OnClick_Logout avviato...");

        // Se auth è null, recupera direttamente l'istanza attiva
        if (auth == null)
        {
            auth = FirebaseAuth.DefaultInstance;
        }

        // 1. Disconnessione effettiva da Firebase
        if (auth != null)
        {
            auth.SignOut();
            user = null;
            Debug.Log("[LOGOUT] Sessione Firebase chiusa con successo.");
        }
        else
        {
            Debug.LogError("[LOGOUT] Impossibile raggiungere FirebaseAuth!");
        }

        // 2. Ferma la musica (opzionale)
        AudioSource musicSource = GetComponent<AudioSource>();
        if (musicSource != null)
        {
            musicSource.Stop();
        }

        // 3. Spegni il menu principale
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            Debug.Log("[LOGOUT] MainMenuPanel spento.");
        }

        // 4. Riaccendi il pannello di login
        if (loginPanel != null)
        {
            loginPanel.SetActive(true);
            Debug.Log("[LOGOUT] LoginPanel riattivato a schermo.");
        }
    }


    private void SetStatus(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }
}
