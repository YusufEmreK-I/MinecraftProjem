using UnityEngine;
using System.Collections.Generic; // List için eklendi

public class WorldGenerator : MonoBehaviour
{
    [Header("World Dimensions")]
    public int worldWidth = 32;  // Önceki optimizasyonlardan gelen değerler
    public int worldDepth = 32;
    public int maxWorldHeight = 24;

    [Header("Generation Settings")]
    public int baseGroundHeight = 5;

    [Header("Noise Settings")]
    public float noiseScale = 200f; // Yumuşak geçişler için yeni değerler
    public int octaves = 4;
    public float persistence = 0.3f;
    public float lacunarity = 2f;
    public int seed;

    [Header("Surface Details")] // Yeni eklendi
    public float dirtExposureThreshold = 0.4f; // Toprak görünme eşiği
    public float surfaceNoiseScale = 20f;     // Yüzey detayları için ayrı gürültü ölçeği

    [Header("Block Prefabs")]
    public GameObject grassBlockPrefab;
    public GameObject dirtBlockPrefab;

    void Start()
    {
        GenerateWorld();
    }

    void GenerateWorld()
    {
        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in transform)
        {
            childrenToDestroy.Add(child.gameObject);
        }
        foreach (GameObject child in childrenToDestroy)
        {
            DestroyImmediate(child);
        }

        System.Random prng = new System.Random(seed == 0 ? Time.time.GetHashCode() : seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        for (int x = 0; x < worldWidth; x++)
        {
            for (int z = 0; z < worldDepth; z++)
            {
                float heightValue = GetPerlinNoiseHeight(x, z, noiseScale, octaves, persistence, lacunarity, octaveOffsets);
                int terrainHeight = Mathf.FloorToInt(heightValue * (maxWorldHeight - baseGroundHeight)) + baseGroundHeight;
                terrainHeight = Mathf.Max(terrainHeight, baseGroundHeight);

                // Yüzeydeki bloğun çimen mi toprak mı olacağına karar vermek için YENİ Gürültü Değeri
                // Aynı GetPerlinNoiseHeight fonksiyonunu kullanabiliriz, ancak farklı bir "offset" ile,
                // böylece yüzey deseni arazi yüksekliğinden bağımsız olur.
                // Veya basitleştirmek için tek bir PerlinNoise çağrısı da yapabiliriz.
                // Basit bir PerlinNoise çağrısı daha anlaşılır olacaktır:
                float surfaceNoiseValue = Mathf.PerlinNoise((x + 0.1f) / surfaceNoiseScale, (z + 0.1f) / surfaceNoiseScale);
                // (x + 0.1f) ve (z + 0.1f) eklememin nedeni, yüzey deseninin tam olarak arazi yüksekliğinin deseniyle çakışmamasını sağlamak.

                for (int y = 0; y < terrainHeight; y++)
                {
                    GameObject blockToPlace;
                    if (y == terrainHeight - 1) // En üst katman
                    {
                        // Eğer yüzey gürültü değeri eşikten düşükse, toprak koy
                        if (surfaceNoiseValue < dirtExposureThreshold)
                        {
                            blockToPlace = dirtBlockPrefab;
                        }
                        else
                        {
                            blockToPlace = grassBlockPrefab;
                        }
                    }
                    else // Alt katmanlar her zaman toprak
                    {
                        blockToPlace = dirtBlockPrefab;
                    }

                    Vector3 blockPosition = new Vector3(x + 0.5f, y + 0.5f, z + 0.5f);
                    Instantiate(blockToPlace, blockPosition, Quaternion.identity, transform);
                }
            }
        }
    }

    // GetPerlinNoiseHeight fonksiyonu değişmedi

    float GetPerlinNoiseHeight(int x, int z, float scale, int octaves, float persistence, float lacunarity, Vector2[] octaveOffsets)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (x + octaveOffsets[i].x) / scale * frequency;
            float sampleZ = (z + octaveOffsets[i].y) / scale * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2 - 1;
            noiseHeight += perlinValue * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }
        return noiseHeight;
    }

    [ContextMenu("Generate New World")]
    void RegenerateWorld()
    {
        GenerateWorld();
    }
}