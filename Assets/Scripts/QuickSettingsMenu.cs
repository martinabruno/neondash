using UnityEngine;

public class QuickSettingsMenu : MonoBehaviour
{
    [Header("Contenitore a comparsa")]
    [SerializeField] private GameObject subButtonsContainer;

    [Header("Riferimenti Funzionali")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AuthManager authManager;

    private void Start()
    {
        // All'avvio nasconde sempre i due pulsanti
        if (subButtonsContainer != null)
        {
            subButtonsContainer.SetActive(false);
        }
    }

    // Apre o chiude i sottomenu ogni volta che si preme l'ingranaggio
    public void ToggleMenu()
    {
        if (subButtonsContainer != null)
        {
            bool statoAttuale = subButtonsContainer.activeSelf;
            subButtonsContainer.SetActive(!statoAttuale);
        }
    }

    // Attiva / Disattiva l'audio
    public void ToggleMusic()
    {
        if (musicSource != null)
        {
            musicSource.mute = !musicSource.mute;
        }
    }

    // Esegue il logout
    public void TriggerLogout()
    {
        if (subButtonsContainer != null)
        {
            subButtonsContainer.SetActive(false);
        }

        if (authManager != null)
        {
            authManager.OnClick_Logout();
        }
    }
}