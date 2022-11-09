using Godot;
using System;
using System.Collections.Generic;

namespace TurnBasedStrategyCourse_godot.Grid;

public class GridSystem<TGridObject>
{
  private readonly TGridObject[,] grid;

  public GridSystem(int width, int height, float cellSize, Func<GridSystem<TGridObject>, GridPosition, TGridObject> factory)
  {
    Width = width;
    Height = height;
    CellSize = cellSize;

    grid = new TGridObject[Width, Height];

    for (var x = 0; x < Width; ++x)
    {
      for (var z = 0; z < Height; ++z)
      {
        grid[x, z] = factory(this, new GridPosition(x, z));
      }
    }
  }

  private float CellSize { get; }

  public int Height { get; }

  public int Width { get; }

  public IEnumerable<Spatial> CreateDebugObjects(PackedScene cellScene)
  {
    for (var x = 0; x < Width; ++x)
    {
      for (var z = 0; z < Height; ++z)
      {
        var position = new GridPosition(x, z);
          
        var cell = cellScene.Instance<Spatial>();
        cell.Translation = GetWorldPosition(position);
          
        yield return cell;
      }
    }
  }

  public IEnumerable<TGridObject> GridObjects()
  {
    for (var x = 0; x < Width; ++x)
    {
      for (var z = 0; z < Height; ++z)
      {
        var gridPosition = new GridPosition(x, z);
        yield return GetGridObject(gridPosition);
      }
    }
  }

  public Vector3 GetWorldPosition(GridPosition position) => new Vector3(position.X, 0, position.Z) * CellSize;

  public GridPosition GetGridPosition(Vector3 worldPosition) =>
    new(Mathf.RoundToInt(worldPosition.x / CellSize),
      Mathf.RoundToInt(worldPosition.z / CellSize));

  public TGridObject GetGridObject(GridPosition position) => grid[position.X, position.Z];

  public bool IsValidPosition(GridPosition position) =>
    position.X >= 0 && position.X < Width && position.Z >= 0 && position.Z < Height;
}