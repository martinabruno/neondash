using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class SkinData
{
    public string skinName;
    public Sprite previewSprite;                      // Immagine/posa da mostrare nel menu
    public RuntimeAnimatorController animatorController; // L'Override Controller con le animazioni
}

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    [Header("Pannelli UI")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject skinSelectPanel;

    [Header("Elementi Grafici Menu")]
    [SerializeField] private Image largePreviewImage;
    [SerializeField] private TextMeshProUGUI skinNameText;

    [Header("Riferimento Personaggio")]
    [SerializeField] private Animator playerAnimator;

    [Header("Catalogo Skin")]
    [SerializeField] private List<SkinData> availableSkins = new List<SkinData>();

    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // Applica la prima skin (o l'ultima salvata)
        currentIndex = PlayerPrefs.GetInt("SelectedSkin", 0);
        if (currentIndex >= availableSkins.Count) currentIndex = 0;
        
        ApplySkin(currentIndex);
    }

    // --- METODI PER I PULSANTI ---

    public void OpenSkinSelect()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (skinSelectPanel != null) skinSelectPanel.SetActive(true);
        UpdateUI();
    }

    public void CloseSkinSelect()
    {
        if (skinSelectPanel != null) skinSelectPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void NextSkin()
    {
        if (availableSkins.Count == 0) return;
        currentIndex = (currentIndex + 1) % availableSkins.Count;
        ApplySkin(currentIndex);
        UpdateUI();
    }

    public void PreviousSkin()
    {
        if (availableSkins.Count == 0) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = availableSkins.Count - 1;
        ApplySkin(currentIndex);
        UpdateUI();
    }

    private void ApplySkin(int index)
    {
        if (availableSkins.Count == 0 || index < 0 || index >= availableSkins.Count) return;

        // Salva la scelta in memoria
        PlayerPrefs.SetInt("SelectedSkin", index);
        PlayerPrefs.Save();

        // Assegna il controller delle animazioni al Player
        if (playerAnimator != null && availableSkins[index].animatorController != null)
        {
            playerAnimator.runtimeAnimatorController = availableSkins[index].animatorController;
        }
    }

    private void UpdateUI()
    {
        if (availableSkins.Count == 0) return;

        if (skinNameText != null)
            skinNameText.text = availableSkins[currentIndex].skinName;

        if (largePreviewImage != null && availableSkins[currentIndex].previewSprite != null)
            largePreviewImage.sprite = availableSkins[currentIndex].previewSprite;
    }
}