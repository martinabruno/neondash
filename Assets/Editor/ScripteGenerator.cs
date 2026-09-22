using System.IO;
using UnityEditor;
using UnityEngine;

public class SpriteGenerator : EditorWindow
{
    private const int SIZE = 24;

    // Palette originale estratta da engine.ts
    private static readonly Color C_TRANSPARENT = new Color(0, 0, 0, 0);
    private static readonly Color C_PLAYER      = HexColor("#A3E635");
    private static readonly Color C_EYES        = HexColor("#0A0F1E");
    private static readonly Color C_BLOCK       = HexColor("#1E2F5E");
    private static readonly Color C_BLOCK_TOP   = HexColor("#33518F");
    private static readonly Color C_PLATFORM    = HexColor("#3B5AA0");
    private static readonly Color C_SPIKE       = HexColor("#F472B6");
    private static readonly Color C_COIN        = HexColor("#FBBF24");
    private static readonly Color C_FLAG        = HexColor("#22D3EE");

    [MenuItem("Tools/Neon Dash/Genera Sprite PNG")]
    public static void GenerateAllSprites()
    {
        string dir = "Assets/Sprites";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        SaveAndConfigureSprite(CreatePlayerTexture(), $"{dir}/player.png");
        SaveAndConfigureSprite(CreateBlockTexture(), $"{dir}/block_solid.png");
        SaveAndConfigureSprite(CreatePlatformTexture(), $"{dir}/platform_oneway.png");
        SaveAndConfigureSprite(CreateSpikeTexture(), $"{dir}/spike.png");
        SaveAndConfigureSprite(CreateCoinTexture(), $"{dir}/coin.png");
        SaveAndConfigureSprite(CreateFlagTexture(), $"{dir}/flag.png");

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Neon Dash", "Tutti i 6 file .png sono stati generati e configurati in Assets/Sprites!", "OK");
    }

    private static Texture2D CreatePlayerTexture()
    {
        Texture2D tex = GetEmptyTexture();
        // Rettangolo giocatore: 15x19 centrato su tela 24x24
        int startX = 4, endX = 19;
        int startY = 2, endY = 21;

        for (int y = startY; y < endY; y++)
            for (int x = startX; x < endX; x++)
                tex.SetPixel(x, y, C_PLAYER);

        // Occhi (orientati a destra)
        for (int y = 12; y <= 15; y++)
        {
            tex.SetPixel(15, y, C_EYES);
            tex.SetPixel(16, y, C_EYES);
            tex.SetPixel(10, y, C_EYES);
            tex.SetPixel(11, y, C_EYES);
        }

        tex.Apply();
        return tex;
    }

    private static Texture2D CreateBlockTexture()
    {
        Texture2D tex = GetEmptyTexture();
        // Blocco pieno 24x24 con bordo superiore evidenziato di 3px
        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                if (y >= SIZE - 3)
                    tex.SetPixel(x, y, C_BLOCK_TOP);
                else
                    tex.SetPixel(x, y, C_BLOCK);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D CreatePlatformTexture()
    {
        Texture2D tex = GetEmptyTexture();
        // Piattaforma sottile (5px di spessore in alto)
        for (int y = SIZE - 5; y < SIZE; y++)
            for (int x = 0; x < SIZE; x++)
                tex.SetPixel(x, y, C_PLATFORM);

        tex.Apply();
        return tex;
    }

    private static Texture2D CreateSpikeTexture()
    {
        Texture2D tex = GetEmptyTexture();
        // Triangolo per la punta (base 24px, vertice in alto al centro)
        int apexY = SIZE - 4;
        for (int y = 0; y <= apexY; y++)
        {
            float progress = (float)y / apexY;
            int halfWidth = Mathf.RoundToInt((1f - progress) * (SIZE / 2f - 2));
            int mid = SIZE / 2;

            for (int x = mid - halfWidth; x <= mid + halfWidth; x++)
            {
                if (x >= 0 && x < SIZE)
                    tex.SetPixel(x, y, C_SPIKE);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateCoinTexture()
    {
        Texture2D tex = GetEmptyTexture();
        Vector2 center = new Vector2(SIZE / 2f, SIZE / 2f);
        float radius = 6.5f;

        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                if (dist <= radius)
                {
                    // Dettaglio pixel centrale più scuro
                    if (x >= 11 && x <= 12 && y >= 9 && y <= 14)
                        tex.SetPixel(x, y, C_BLOCK);
                    else
                        tex.SetPixel(x, y, C_COIN);
                }
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D CreateFlagTexture()
    {
        Texture2D tex = GetEmptyTexture();
        // Asta (x = 5 e 6)
        for (int y = 0; y < SIZE; y++)
        {
            tex.SetPixel(5, y, C_FLAG);
            tex.SetPixel(6, y, C_FLAG);
        }

        // Drappo bandiera triangolare verso destra
        for (int y = 11; y < 23; y++)
        {
            int distFromCenter = Mathf.Abs(y - 17);
            int width = 14 - distFromCenter * 2;
            for (int x = 7; x < 7 + width; x++)
            {
                if (x < SIZE)
                    tex.SetPixel(x, y, C_FLAG);
            }
        }
        tex.Apply();
        return tex;
    }

    private static Texture2D GetEmptyTexture()
    {
        Texture2D tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);
        Color[] fill = new Color[SIZE * SIZE];
        for (int i = 0; i < fill.Length; i++) fill[i] = C_TRANSPARENT;
        tex.SetPixels(fill);
        return tex;
    }

    private static void SaveAndConfigureSprite(Texture2D tex, string path)
    {
        byte[] bytes = tex.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = SIZE; // 24 PPU
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }
    }

    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}