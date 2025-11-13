using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using VTools.Grid;
using VTools.ScriptableObjectDatabase;
using VTools.Utility;

namespace Components.ProceduralGeneration.SimpleRoomPlacement
{
    [CreateAssetMenu(menuName = "Procedural Generation Method/Simple Room Placement")]
    public class SimpleRoomPlacement : ProceduralGenerationMethod
    {
        [Header("Room Parameters")]
        [SerializeField] private int _maxRooms = 10;
        private RectInt lastRoom;
        
        protected override async UniTask ApplyGeneration(CancellationToken cancellationToken)
        {
            // Declare variables here
            // ........

            //List<RectInt> placedRooms = new();
            int roomsPlacedCount = 0;
            int attempts = 0;

            lastRoom = new RectInt();

            for (int i = 0; i < _maxSteps; i++)
            {
                // Check for cancellation
                cancellationToken.ThrowIfCancellationRequested();
                
                // Your algorithm here

                int x = RandomService.Range(0, Grid.Width);
                int y = RandomService.Range(0, Grid.Lenght);
                int sizeX = RandomService.Range(4, 10);
                int sizeY = RandomService.Range(4, 10);
                RectInt testRoom = new RectInt(x, y, sizeX, sizeY);
                if (CanPlaceRoom(testRoom, 1))
                {
                    PlaceRoom(testRoom);

                    CorridorBetweenTwoRoom(testRoom, lastRoom);

                    lastRoom = testRoom;
                }

                // Waiting between steps to see the result.
                await UniTask.Delay(GridGenerator.StepDelay, cancellationToken : cancellationToken);
            }
            
            // Final ground building.
            BuildGround();
        }

        private void CorridorBetweenTwoRoom(RectInt room1, RectInt room2)
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

        private void PlaceRoom(RectInt rectIntRoom, string roomTileType = ROOM_TILE_NAME)
        {
            for (int ix = rectIntRoom.xMin; ix < rectIntRoom.xMax; ix++)
            {
                for (int iy = rectIntRoom.yMin; iy < rectIntRoom.yMax; iy++)
                {
                    if (Grid.TryGetCellByCoordinates(ix, iy, out var cell))
                    {
                        AddTileToCell(cell, roomTileType, true);
                    }
                }
            }
        }
        
        private void BuildGround()
        {
            var groundTemplate = ScriptableObjectDatabase.GetScriptableObject<GridObjectTemplate>("Grass");
            
            // Instantiate ground blocks
            for (int x = 0; x < Grid.Width; x++)
            {
                for (int z = 0; z < Grid.Lenght; z++)
                {
                    if (!Grid.TryGetCellByCoordinates(x, z, out var chosenCell))
                    {
                        Debug.LogError($"Unable to get cell on coordinates : ({x}, {z})");
                        continue;
                    }
                    
                    GridGenerator.AddGridObjectToCell(chosenCell, groundTemplate, false);
                }
            }
        }
    }
}