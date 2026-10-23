using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class ArenaBounds : MonoBehaviour
{
    public const float Width = 40f;
    public const float Height = 22.5f;
    public const float WallThickness = 0.8f;
    public const int BorderCells = 2;
    public const float VisibleBorder = 1.6f;
    public const float DecorScale = 0.85f;
    public const float SafeRadius = 4.5f;

    public static ArenaBounds Instance { get; private set; }

    public Rect Area { get; private set; }

    public Rect ViewArea => new Rect(
        Area.xMin - VisibleBorder, Area.yMin - VisibleBorder,
        Area.width + VisibleBorder * 2f, Area.height + VisibleBorder * 2f);

    private static Sprite fallbackGroundSprite;

    public static ArenaBounds Create(Vector2 center, MapSO map, int seed)
    {
        if (Instance != null) Destroy(Instance.gameObject);

        GameObject root = new GameObject("ArenaBounds");
        root.transform.position = new Vector3(center.x, center.y, 0f);
        ArenaBounds arena = root.AddComponent<ArenaBounds>();
        arena.Build(center, map, seed);
        return arena;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static Vector2 Clamp(Vector2 point, float margin)
    {
        if (Instance == null) return point;

        Rect area = Instance.Area;
        float x = Mathf.Clamp(point.x, area.xMin + margin, area.xMax - margin);
        float y = Mathf.Clamp(point.y, area.yMin + margin, area.yMax - margin);
        return new Vector2(x, y);
    }

    private void Build(Vector2 center, MapSO map, int seed)
    {
        Area = new Rect(center.x - Width * 0.5f, center.y - Height * 0.5f, Width, Height);
        Material spriteMaterial = FindSpriteMaterial();
        System.Random rng = new System.Random(1000 + seed);

        bool hasTiles = map != null && map.groundTiles != null && map.groundTiles.Exists(t => t != null);
        if (hasTiles)
        {
            BuildTileGround(map, spriteMaterial, rng);
        }
        else
        {
            BuildFallbackGround(spriteMaterial, map != null ? map.groundTint : new Color(0.34f, 0.5f, 0.32f));
        }

        Color wallColor = map != null ? map.wallTint : new Color(0.55f, 0.52f, 0.48f);
        Color trimColor = map != null ? map.wallTrimTint : new Color(0.85f, 0.8f, 0.7f);
        bool hasWallTiles = map != null && map.wallTiles != null && map.wallTiles.FindAll(t => t != null).Count >= 9;
        if (hasWallTiles) BuildWallBand(map, spriteMaterial);
        BuildWalls(wallColor, spriteMaterial, !hasWallTiles);
        BuildTrim(trimColor, spriteMaterial);

        if (map != null) ScatterDecor(map, spriteMaterial, rng);
    }

    private Tilemap CreateTilemap(string layerName, int sortingOrder, Material material, Color tint)
    {
        Grid grid = GetComponentInChildren<Grid>();
        if (grid == null)
        {
            GameObject gridObject = new GameObject("Grid");
            gridObject.transform.SetParent(transform, false);
            grid = gridObject.AddComponent<Grid>();
        }

        GameObject tilemapObject = new GameObject(layerName);
        tilemapObject.transform.SetParent(grid.transform, false);
        Tilemap tilemap = tilemapObject.AddComponent<Tilemap>();
        TilemapRenderer tilemapRenderer = tilemapObject.AddComponent<TilemapRenderer>();
        tilemapRenderer.sortingOrder = sortingOrder;
        if (material != null) tilemapRenderer.sharedMaterial = material;
        tilemap.color = tint;
        return tilemap;
    }

    private void BuildWallBand(MapSO map, Material material)
    {
        Tilemap tilemap = CreateTilemap("Wall Band", -45, material, map.wallTint);
        List<TileBase> pattern = map.wallTiles.FindAll(t => t != null);

        float halfW = Width * 0.5f;
        float halfH = Height * 0.5f;
        int minX = Mathf.FloorToInt(-halfW) - BorderCells;
        int maxX = Mathf.CeilToInt(halfW) + BorderCells;
        int minY = Mathf.FloorToInt(-halfH) - BorderCells;
        int maxY = Mathf.CeilToInt(halfH) + BorderCells;

        List<Vector3Int> positions = new List<Vector3Int>(512);
        List<TileBase> tiles = new List<TileBase>(512);
        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                bool inside = x >= -halfW && x + 1 <= halfW && y >= -halfH && y + 1 <= halfH;
                if (inside) continue;

                int column = Mod(x, 3);
                int row = 2 - Mod(y, 3);
                positions.Add(new Vector3Int(x, y, 0));
                tiles.Add(pattern[row * 3 + column]);
            }
        }

        tilemap.SetTiles(positions.ToArray(), tiles.ToArray());
    }

    private void BuildTrim(Color trimColor, Material material)
    {
        float halfW = Width * 0.5f;
        float halfH = Height * 0.5f;
        const float trim = 0.14f;
        const float shade = 0.45f;
        Color shadow = new Color(0f, 0f, 0f, 0.3f);

        CreateStrip("Trim Top", new Vector2(0f, halfH), new Vector2(Width + trim, trim), trimColor, -38, material);
        CreateStrip("Trim Bottom", new Vector2(0f, -halfH), new Vector2(Width + trim, trim), trimColor, -38, material);
        CreateStrip("Trim Left", new Vector2(-halfW, 0f), new Vector2(trim, Height + trim), trimColor, -38, material);
        CreateStrip("Trim Right", new Vector2(halfW, 0f), new Vector2(trim, Height + trim), trimColor, -38, material);

        CreateStrip("Shade Top", new Vector2(0f, halfH - shade * 0.5f - trim * 0.5f), new Vector2(Width, shade), shadow, -39, material);
        CreateStrip("Shade Left", new Vector2(-halfW + shade * 0.5f + trim * 0.5f, 0f), new Vector2(shade, Height), shadow, -39, material);
        CreateStrip("Shade Right", new Vector2(halfW - shade * 0.5f - trim * 0.5f, 0f), new Vector2(shade, Height), shadow, -39, material);
    }

    private void CreateStrip(string stripName, Vector2 localPosition, Vector2 size, Color color, int order, Material material)
    {
        GameObject strip = new GameObject(stripName);
        strip.transform.SetParent(transform, false);
        strip.transform.localPosition = localPosition;
        SpriteRenderer renderer = strip.AddComponent<SpriteRenderer>();
        renderer.sprite = RuntimeSprites.White;
        renderer.drawMode = SpriteDrawMode.Sliced;
        renderer.size = size;
        renderer.color = color;
        renderer.sortingOrder = order;
        if (material != null) renderer.sharedMaterial = material;
    }

    private void BuildTileGround(MapSO map, Material material, System.Random rng)
    {
        Tilemap tilemap = CreateTilemap("Ground", -100, material, map.groundTint);

        List<TileBase> ground = map.groundTiles.FindAll(t => t != null);
        List<TileBase> accents = map.accentTiles != null ? map.accentTiles.FindAll(t => t != null) : new List<TileBase>();
        bool pattern = map.groundIsPattern3x3 && ground.Count >= 9;

        int halfW = Mathf.CeilToInt(Width * 0.5f) + 1;
        int halfH = Mathf.CeilToInt(Height * 0.5f) + 1;
        int count = (halfW * 2) * (halfH * 2);
        Vector3Int[] positions = new Vector3Int[count];
        TileBase[] tiles = new TileBase[count];

        int index = 0;
        for (int y = -halfH; y < halfH; y++)
        {
            for (int x = -halfW; x < halfW; x++)
            {
                positions[index] = new Vector3Int(x, y, 0);
                if (accents.Count > 0 && rng.NextDouble() < map.accentChance)
                {
                    tiles[index] = accents[rng.Next(accents.Count)];
                }
                else if (pattern)
                {
                    int column = Mod(x, 3);
                    int row = 2 - Mod(y, 3);
                    tiles[index] = ground[row * 3 + column];
                }
                else
                {
                    tiles[index] = ground[rng.Next(ground.Count)];
                }
                index++;
            }
        }

        tilemap.SetTiles(positions, tiles);
    }

    private void BuildFallbackGround(Material material, Color tint)
    {
        GameObject ground = new GameObject("Ground");
        ground.transform.SetParent(transform, false);
        SpriteRenderer groundRenderer = ground.AddComponent<SpriteRenderer>();
        groundRenderer.sprite = GetFallbackGroundSprite();
        groundRenderer.drawMode = SpriteDrawMode.Tiled;
        groundRenderer.size = new Vector2(Width, Height);
        groundRenderer.color = tint;
        groundRenderer.sortingOrder = -100;
        if (material != null) groundRenderer.sharedMaterial = material;
    }

    private void BuildWalls(Color wallColor, Material material, bool visible)
    {
        float halfW = Width * 0.5f;
        float halfH = Height * 0.5f;
        float thickness = visible ? BorderCells : WallThickness;
        float half = thickness * 0.5f;
        CreateWall("Wall Top", new Vector2(0f, halfH + half), new Vector2(Width + thickness * 2f, thickness), wallColor, material, visible);
        CreateWall("Wall Bottom", new Vector2(0f, -halfH - half), new Vector2(Width + thickness * 2f, thickness), wallColor, material, visible);
        CreateWall("Wall Left", new Vector2(-halfW - half, 0f), new Vector2(thickness, Height), wallColor, material, visible);
        CreateWall("Wall Right", new Vector2(halfW + half, 0f), new Vector2(thickness, Height), wallColor, material, visible);
    }

    private void ScatterDecor(MapSO map, Material material, System.Random rng)
    {
        if (map.decorPrefabs == null) return;
        List<GameObject> prefabs = map.decorPrefabs.FindAll(p => p != null);
        if (prefabs.Count == 0) return;

        GameObject decorRoot = new GameObject("Decor");
        decorRoot.transform.SetParent(transform, false);

        List<Vector2> placed = new List<Vector2>(map.decorCount);
        float marginX = Width * 0.5f - 1.8f;
        float marginY = Height * 0.5f - 1.8f;

        for (int attempt = 0; attempt < map.decorCount * 12 && placed.Count < map.decorCount; attempt++)
        {
            Vector2 local = new Vector2(
                (float)(rng.NextDouble() * 2.0 - 1.0) * marginX,
                (float)(rng.NextDouble() * 2.0 - 1.0) * marginY);

            if (local.magnitude < SafeRadius) continue;
            if (placed.Exists(p => (p - local).sqrMagnitude < 6.25f)) continue;

            placed.Add(local);
            GameObject prefab = prefabs[rng.Next(prefabs.Count)];
            GameObject decor = Instantiate(prefab, decorRoot.transform);
            decor.transform.localPosition = local;
            decor.transform.localScale = Vector3.one * DecorScale;
            MakeDecorative(decor, map.decorTint, material);
        }
    }

    private static void MakeDecorative(GameObject decor, Color tint, Material material)
    {
        foreach (Collider2D col in decor.GetComponentsInChildren<Collider2D>(true)) col.enabled = false;
        foreach (Rigidbody2D body in decor.GetComponentsInChildren<Rigidbody2D>(true)) body.simulated = false;
        foreach (MonoBehaviour behaviour in decor.GetComponentsInChildren<MonoBehaviour>(true)) behaviour.enabled = false;

        foreach (SpriteRenderer renderer in decor.GetComponentsInChildren<SpriteRenderer>(true))
        {
            bool isShadow = renderer.sharedMaterial != null && renderer.sharedMaterial.name.Contains("Shadow");
            if (material != null) renderer.sharedMaterial = material;
            renderer.sortingLayerID = 0;

            if (isShadow)
            {
                renderer.color = new Color(0f, 0f, 0f, 0.28f);
                renderer.sortingOrder = -80;
            }
            else
            {
                renderer.color = tint;
                renderer.sortingOrder = -60 + renderer.sortingOrder;
            }
        }
    }

    private void CreateWall(string wallName, Vector2 localPosition, Vector2 size, Color color, Material material, bool visible)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(transform, false);
        wall.transform.localPosition = localPosition;

        if (visible)
        {
            SpriteRenderer renderer = wall.AddComponent<SpriteRenderer>();
            renderer.sprite = RuntimeSprites.White;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
            renderer.color = color;
            renderer.sortingOrder = -45;
            if (material != null) renderer.sharedMaterial = material;
        }

        BoxCollider2D box = wall.AddComponent<BoxCollider2D>();
        box.size = size;
    }

    private static int Mod(int value, int modulus)
    {
        int result = value % modulus;
        return result < 0 ? result + modulus : result;
    }

    private static Material FindSpriteMaterial()
    {
        if (PlayerController.Instance == null) return null;
        SpriteRenderer renderer = PlayerController.Instance.GetComponent<SpriteRenderer>();
        return renderer != null ? renderer.sharedMaterial : null;
    }

    private static Sprite GetFallbackGroundSprite()
    {
        if (fallbackGroundSprite != null) return fallbackGroundSprite;

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color dark = new Color(0.88f, 0.88f, 0.88f, 1f);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool checker = (x < size / 2) ^ (y < size / 2);
                pixels[y * size + x] = checker ? Color.white : dark;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Point;

        fallbackGroundSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size / 4f, 0, SpriteMeshType.FullRect);
        return fallbackGroundSprite;
    }
}
