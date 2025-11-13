using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;
using UnityEngine.UIElements;
using Unity.VisualScripting;

[CreateAssetMenu(menuName = "Procedural Generation Method/NoiseGenerator")]
public class NoiseGenerator : ProceduralGenerationMethod
{
    public float probaInitialAuBuild;
    public int requiredGround;

    [Header("Noise Parameters")]
    public int noiseType;
    [Range(0, 1)] public float frequency;
    [Range(0, 2)] public float amplitude;

    [Header("Fractal Parameters")]
    public int fractalType;
    [Range(0, 10)] public float octaves;
    [Range(0, 10)] public float lacunarity;
    [Range(0, 10)] public float persistence;

    [Header("Hights")]
    [Range(-1, 1)] public float waterHight;
    [Range(-1, 1)] public float sandHight;
    [Range(-1, 1)] public float grassHight;
    [Range(-1, 1)] public float rockHight;


    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        var waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(WATER_TILE_NAME);
        var sandTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(SAND_TILE_NAME);
        var grassTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(GRASS_TILE_NAME);
        var rockTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(ROCK_TILE_NAME);

        // Create and configure FastNoise object
        FastNoiseLite noise = new FastNoiseLite(RandomService.Seed);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);

        noise.SetFrequency(frequency);
        // Gather noise data
        float[,] noiseData = new float[Grid.Width, Grid.Lenght];

        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Lenght; y++)
            {
                noiseData[x, y] = noise.GetNoise(x, y);
                float currentHigh = noiseData[x, y];

                var curentTemplate = waterTemplate;

                if(!Grid.TryGetCellByCoordinates(x, y, out var chosenCell))
                {
                    Debug.LogError($"Unable to get cell on coordinates : ({x}, {y})");
                    continue;
                }
                else if(currentHigh < waterHight)
                {
                    curentTemplate = waterTemplate; 
                }
                else if(currentHigh < sandHight)
                {
                    curentTemplate = sandTemplate;
                }
                else if(currentHigh < grassHight)
                {
                    curentTemplate = grassTemplate;
                }
                else if(currentHigh < rockHight)
                {
                    curentTemplate = rockTemplate;
                }
                else
                {
                    curentTemplate = rockTemplate;
                }

                GridGenerator.AddGridObjectToCell(chosenCell, curentTemplate, true);
            }
        }
    }

    private int CountGroundNeighbors(bool[,] snapshot, int x, int y, bool includeSelf)
    {
        int count = 0;
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (!includeSelf && dx == 0 && dy == 0)
                    continue;

                int nx = x + dx;
                int ny = y + dy;

                if (nx < 0 || ny < 0 || nx >= Grid.Width || ny >= Grid.Lenght)
                    continue;

                if (snapshot[nx, ny]) count++;
            }
        }
        return count;
    }

    private void BuildGround()
    {
        var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(GRASS_TILE_NAME);
        var waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(WATER_TILE_NAME);

        // Instantiate ground blocks
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int z = 0; z < Grid.Lenght; z++)
            {
                bool poseTile = RandomService.Chance(probaInitialAuBuild);

                if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                {
                    Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                    continue;
                }
                if (poseTile)
                {
                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
                }
                else
                {
                    GridGenerator.AddGridObjectToCell(chosenCell, waterTemplate, false);
                }
            }
        }
    }

}
