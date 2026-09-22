using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGeneratorTilemap : MonoBehaviour
{
    [Header("Tilemaps di Scena")]
    [SerializeField] private Tilemap tilemapSolid;
    [SerializeField] private Tilemap tilemapOneWay;

    [Header("Tile Assets (.asset)")]
    [SerializeField] private TileBase solidTile;
    [SerializeField] private TileBase oneWayTile;

    [Header("Prefabs Oggetti Interattivi")]
    [SerializeField] private GameObject spikePrefab;
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private GameObject goalPrefab;

    private readonly List<GameObject> activeSpawns = new List<GameObject>();
    private readonly List<GameObject> activeCoins = new List<GameObject>();

    public Vector2 SpawnPosition { get; private set; }
    public int TotalCoins { get; private set; }
    public int CollectedCoins { get; private set; }
    public int Cols { get; private set; }
    public int Rows { get; private set; }

    public void BuildLevel(LevelDef def)
    {
        ClearLevel();

        Rows = def.map.Length;
        Cols = 0;
        foreach (var row in def.map)
        {
            if (row.Length > Cols) Cols = row.Length;
        }

        for (int y = 0; y < Rows; y++)
        {
            string rowStr = def.map[y];
            // Inversione asse Y per le coordinate di Unity (l'origine è in basso a sinistra)
            int unityY = Rows - 1 - y;

            for (int x = 0; x < Cols; x++)
            {
                char ch = x < rowStr.Length ? rowStr[x] : '.';
                Vector3Int tilePos = new Vector3Int(x, unityY, 0);
                Vector3 worldPos = new Vector3(x + 0.5f, unityY + 0.5f, 0);

                switch (ch)
                {
                    case '#':
                        tilemapSolid.SetTile(tilePos, solidTile);
                        break;

                    case '=':
                        tilemapOneWay.SetTile(tilePos, oneWayTile);
                        break;

                    case '^':
                        GameObject spike = Instantiate(spikePrefab, worldPos, Quaternion.identity, transform);
                        activeSpawns.Add(spike);
                        break;

                    case 'c':
                        GameObject coin = Instantiate(coinPrefab, worldPos, Quaternion.identity, transform);
                        activeCoins.Add(coin);
                        TotalCoins++;
                        break;

                    case 'P':
                        SpawnPosition = new Vector2(x, unityY);
                        break;

                    case 'G':
                        GameObject goal = Instantiate(goalPrefab, worldPos, Quaternion.identity, transform);
                        activeSpawns.Add(goal);
                        break;
                }
            }
        }
    }

    public void CheckCoinOverlap(Vector3 playerPos)
    {
        for (int i = activeCoins.Count - 1; i >= 0; i--)
        {
            GameObject coin = activeCoins[i];
            if (coin != null && Vector2.Distance(coin.transform.position, playerPos) < 0.65f)
            {
                activeCoins.RemoveAt(i);
                Destroy(coin);
                CollectedCoins++;
                RetroAudio.Instance.PlayCoin();
                UIManager.Instance.UpdateCoins(CollectedCoins, TotalCoins);
            }
        }
    }

    public void ClearLevel()
    {
        if (tilemapSolid != null) tilemapSolid.ClearAllTiles();
        if (tilemapOneWay != null) tilemapOneWay.ClearAllTiles();

        foreach (var obj in activeSpawns)
        {
            if (obj != null) Destroy(obj);
        }
        activeSpawns.Clear();

        foreach (var coin in activeCoins)
        {
            if (coin != null) Destroy(coin);
        }
        activeCoins.Clear();

        TotalCoins = 0;
        CollectedCoins = 0;
    }

        public void CollectCoin()
    {
        CollectedCoins++;
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCoins(CollectedCoins, TotalCoins);
        }
    }
}