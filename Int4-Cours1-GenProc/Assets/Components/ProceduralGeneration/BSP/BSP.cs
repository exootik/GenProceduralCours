using UnityEngine;
using Components.ProceduralGeneration;
using Cysharp.Threading.Tasks;
using System.Threading;
using VTools.RandomService;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;

[CreateAssetMenu(menuName = "Procedural Generation Method/BSP")]
public class BSP : ProceduralGenerationMethod
{
    public List<RectInt> placedRooms = new();
    public int roomsPlacedCount = 0;
    public int attempts = 0;

    protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
    {
        var allGrid = new RectInt(0, 0, Grid.Width, Grid.Lenght);
        var root = new BspNode(allGrid, RandomService, this);
    }

    public void PlaceRoom(RectInt rectIntRoom)
    {
        for (int ix = rectIntRoom.xMin; ix < rectIntRoom.xMax; ix++)
        {
            for (int iy = rectIntRoom.yMin; iy < rectIntRoom.yMax; iy++)
            {
                if (Grid.TryGetCellByCoordinates(ix, iy, out var cell))
                {
                    AddTileToCell(cell, ROOM_TILE_NAME, true);
                }
            }
        }
    }

    // ------------------------------------------------ CORRIDOR ----------------------------------------------------
    public void CorridorBetweenTwoRoom(RectInt room1, RectInt room2)
    {
        Vector2Int room1Center = room1.GetCenter();
        Vector2Int room2Center = room2.GetCenter();

        int room1CenterX = room1.GetCenter().x;
        int room1CenterY = room1.GetCenter().y;
        int room2CenterX = room2.GetCenter().x;
        int room2CenterY = room2.GetCenter().y;

        int startX = room1Center.x;
        int startY = room1Center.y;
        int endX = room2Center.x;
        int endY = room2Center.y;

        int minX = Mathf.Min(startX, endX);
        int maxX = Mathf.Max(startX, endX);

        for (int x = minX; x <= maxX; x++)
        {
            if (Grid.TryGetCellByCoordinates(x, startY, out var cell))
            {
                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }

        int minY = Mathf.Min(startY, endY);
        int maxY = Mathf.Max(startY, endY);
        for (int y = minY; y <= maxY; y++)
        {
            if (Grid.TryGetCellByCoordinates(endX, y, out var cell))
            {
                AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
            }
        }
    }

    /// Creates an L-shaped corridor between two points, randomly choosing horizontal-first or vertical-first
    public void CreateDogLegCorridor(Vector2Int start, Vector2Int end)
    {
        bool horizontalFirst = RandomService.Chance(0.5f);

        if (horizontalFirst)
        {
            // Draw horizontal line first, then vertical
            CreateHorizontalCorridor(start.x, end.x, start.y);
            CreateVerticalCorridor(start.y, end.y, end.x);
        }
        else
        {
            // Draw vertical line first, then horizontal
            CreateVerticalCorridor(start.y, end.y, start.x);
            CreateHorizontalCorridor(start.x, end.x, end.y);
        }
    }

    /// Creates a horizontal corridor from x1 to x2 at the given y coordinate
    public void CreateHorizontalCorridor(int x1, int x2, int y)
    {
        int xMin = Mathf.Min(x1, x2);
        int xMax = Mathf.Max(x1, x2);

        for (int x = xMin; x <= xMax; x++)
        {
            if (!Grid.TryGetCellByCoordinates(x, y, out var cell))
                continue;

            AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
        }
    }

    /// Creates a vertical corridor from y1 to y2 at the given x coordinate
    public void CreateVerticalCorridor(int y1, int y2, int x)
    {
        int yMin = Mathf.Min(y1, y2);
        int yMax = Mathf.Max(y1, y2);

        for (int y = yMin; y <= yMax; y++)
        {
            if (!Grid.TryGetCellByCoordinates(x, y, out var cell))
                continue;

            AddTileToCell(cell, CORRIDOR_TILE_NAME, true);
        }
    }
}

public class BspNode
{
    private RectInt _bounds;
    private RandomService _randomService;
    private BSP _parent;
    private BspNode _child1, _child2;

    private Vector2Int _roomMinSize = new Vector2Int(5, 5);


    public BspNode(RectInt bounds, RandomService randomService, BSP parent)
    {
        Debug.Log("BspNode created");
        _bounds = bounds;
        _randomService = randomService;
        _parent = parent;

        bool splitLeft = randomService.Chance(0.5f);

        int diviseurDeSplit = randomService.Range(2, 3);
        int marge = randomService.Range(1, 4);
        Vector2Int temp = new Vector2Int(0, 0);

        RectInt splitBoundsLeft;
        RectInt splitBoundsRight;

        if (splitLeft)
        {
            splitBoundsLeft = new RectInt(_bounds.xMin, _bounds.yMin, _bounds.width / diviseurDeSplit, _bounds.height);
            splitBoundsRight = new RectInt(_bounds.xMin + _bounds.width / diviseurDeSplit, _bounds.yMin, _bounds.width / diviseurDeSplit, _bounds.height);
        }
        else
        {
            splitBoundsLeft = new RectInt(_bounds.xMin, _bounds.yMin, _bounds.width, _bounds.height / diviseurDeSplit);
            splitBoundsRight = new RectInt(_bounds.xMin, _bounds.yMin + _bounds.height / diviseurDeSplit, _bounds.width, _bounds.height / diviseurDeSplit);
        }
        

        if (splitBoundsLeft.width < _roomMinSize.x || splitBoundsRight.height < _roomMinSize.y || splitBoundsLeft.height < _roomMinSize.y)
        {
            // On place ici 
            _bounds.width = _bounds.width - marge;
            _bounds.height = _bounds.height - marge;
            _parent.PlaceRoom(_bounds);
            _parent.placedRooms.Add(_bounds);

            for (int i = 0; i < _parent.placedRooms.Count - 1; i++)
            {
                Vector2Int start = _parent.placedRooms[i].GetCenter();
                Vector2Int end = _parent.placedRooms[i + 1].GetCenter();
                _parent.CreateDogLegCorridor(start, end);
            }

            return;
        }

        _child1 = new BspNode(splitBoundsLeft, _randomService, _parent);
        _child2 = new BspNode(splitBoundsRight, _randomService, _parent);

        // V1
        //_parent.CorridorBetweenTwoRoom(splitBoundsLeft, splitBoundsRight);

        // V2
        //Vector2Int start = splitBoundsLeft.GetCenter() - temp;
        //Vector2Int end = splitBoundsRight.GetCenter() - temp;

        //_parent.CreateDogLegCorridor(start, end);

    }
}


