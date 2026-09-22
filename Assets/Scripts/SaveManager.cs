using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelProgressItem
{
    public int levelId;
    public int stars;
    public int bestTimeMs;
    public bool cleared;
}

[Serializable]
public class PlayerSaveData
{
    public List<LevelProgressItem> progress = new List<LevelProgressItem>();
}

public static class SaveManager
{
    private const string SAVE_KEY = "NEON_DASH_SAVE";

    public static PlayerSaveData Load()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json)) return new PlayerSaveData();
        return JsonUtility.FromJson<PlayerSaveData>(json);
    }

    public static void SaveProgress(int levelId, int stars, int timeMs)
    {
        PlayerSaveData data = Load();
        LevelProgressItem item = data.progress.Find(p => p.levelId == levelId);

        if (item == null)
        {
            item = new LevelProgressItem
            {
                levelId = levelId,
                stars = stars,
                bestTimeMs = timeMs,
                cleared = true
            };
            data.progress.Add(item);
        }
        else
        {
            item.stars = Mathf.Max(item.stars, stars);
            item.bestTimeMs = item.bestTimeMs > 0 ? Mathf.Min(item.bestTimeMs, timeMs) : timeMs;
            item.cleared = true;
        }

        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static bool IsLevelUnlocked(int levelId)
    {
        if (levelId == 1) return true;
        PlayerSaveData data = Load();
        LevelProgressItem prev = data.progress.Find(p => p.levelId == levelId - 1);
        return prev != null && prev.cleared;
    }
}