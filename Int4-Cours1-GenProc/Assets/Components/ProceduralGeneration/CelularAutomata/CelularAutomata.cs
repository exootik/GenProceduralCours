using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using VTools.Grid;
using VTools.RandomService;
using VTools.ScriptableObjectDatabase;

[CreateAssetMenu(menuName = "Procedural Generation Method/CelularAutomata")]
public class CelularAutomata : ProceduralGenerationMethod
{
    public float probaInitialAuBuild;
    public int requiredGround;
    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(GRASS_TILE_NAME);
        var waterTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>(WATER_TILE_NAME);

        BuildGround();

        // ETAT ACTUEL
        bool[,] current = new bool[Grid.Width, Grid.Lenght];
        for (int x = 0; x < Grid.Width; x++)
        {
            for (int y = 0; y < Grid.Lenght; y++)
            {
                if (Grid.TryGetCellByCoordinates(x, y, out var cell) && cell.ContainObject &&
                    cell.GridObject.Template.Name == GRASS_TILE_NAME)
                {
                    current[x, y] = true;
                }
                else
                {
                    current[x, y] = false;
                }
            }
        }

        for (int  i = 0; i < _maxSteps; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // ALGO
            bool[,] isGround = new bool[Grid.Width, Grid.Lenght];

            for (int ix = 0; ix < Grid.Width; ix++)
            {
                for (int iy = 0; iy < Grid.Lenght; iy++)
                {
                    int groundNeighbors = CountGroundNeighbors(current, ix, iy, includeSelf: false);
                    isGround[ix, iy] = groundNeighbors >= requiredGround;
                }
            }

            for (int ix = 0; ix < Grid.Width; ix++)
            {
                for (int iy = 0; iy < Grid.Lenght; iy++)
                {
                    if (!Grid.TryGetCellByCoordinates(ix, iy, out var cell))
                        continue;

                    if (isGround[ix, iy])
                        GridGenerator.AddGridObjectToCell(cell, groundTemplate, true);
                    else
                        GridGenerator.AddGridObjectToCell(cell, waterTemplate, true);
                }
            }

            current = isGround;

            //for (int ix = 0;  ix < Grid.Width; ix++)
            //{
            //    for (int iy = 0; iy < Grid.Lenght; iy++)
            //    {
            //        if (Grid.TryGetCellByCoordinates(ix, iy, out var cell))
            //        {
            //            if (VerifyIsGround(cell, requiredGround))
            //            {
            //                GridGenerator.AddGridObjectToCell(cell, groundTemplate, true);
            //            }
            //            else
            //            {
            //                GridGenerator.AddGridObjectToCell(cell, waterTemplate, true);
            //            }
            //        }

            //    }
            //}

            await UniTask.Delay(GridGenerator.StepDelay, cancellationToken: cancellationToken);
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

    private bool VerifyIsGround(Cell cell, int requiredGround)
    {
        int GroundCellCount = 0;
        if (Grid.TryGetCellByCoordinates(cell.Coordinates.x - 1, cell.Coordinates.y - 1, out var cellVerif1))
        {
            if (cellVerif1.GridObject.Template.Name == GRASS_TILE_NAME)
                GroundCellCount++;
        }
        if (Grid.TryGetCellByCoordinates(cell.Coordinates.x - 1, cell.Coordinates.y, out var cellVerif2))
        {
            if (cellVerif2.GridObject.Template.Name == GRASS_TILE_NAME)
                GroundCellCount++;
        }
        if (Grid.TryGetCellByCoordinates(cell.Coordinates.x - 1, cell.Coordinates.y + 1, out var cellVerif3))
        {
            if (cellVerif3.GridObject.Template.Name == GRASS_TILE_NAME)
                GroundCellCount++;
        }

        if (Grid.TryGetCellByCoordinates(cell.Coordinates.x, cell.Coordinates.y - 1, out var cellVerif4))
        {
            if (cellVerif4.GridObject.Template.Name == GRASS_TILE_NAME)
                GroundCellCount++;
        }
        if (Grid.TryGetCellByCoordinates(cell.Coordinates.x, cell.Coordinates.y, out var cellVerif5))
        {
            if (cellVerif5.GridObject.Template.Name == GRASS_TILE_NAME)
                GroundCellCount++;
        }
        if (Grid.TryGetCellByCoordinates(cell.Coordinates.x, cell.Coordinates.y + 1, out var cellVerif6))
        {
            if (cellVerif6.GridObject.Template.Name == GRASS_TILE_NAME)
                GroundCellCount++;
        }

        if (Grid.TryGetCellByCoordinates(cell.Coordinates.x + 1, cell.Coordinates.y - 1, out var cellVerif7))
        {
            if (cellVerif7.GridObject.Template.Name == GRASS_TILE_NAME)
                GroundCellCount++;
        }
        if (Grid.TryGetCellByCoordinates(cell.Coordinates.x + 1, cell.Coordinates.y, out var cellVerif8))
        {
            if (cellVerif8.GridObject.Template.Name == GRASS_TILE_NAME)
                GroundCellCount++;
        }
        if (Grid.TryGetCellByCoordinates(cell.Coordinates.x + 1, cell.Coordinates.y + 1, out var cellVerif9))
        {
            if (cellVerif9.GridObject.Template.Name == GRASS_TILE_NAME)
                GroundCellCount++;
        }

        if (GroundCellCount >= requiredGround)
        {
            return true;
        }
        else
        {
            return false;
        }
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
