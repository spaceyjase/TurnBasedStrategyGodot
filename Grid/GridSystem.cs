using Godot;

namespace TurnBasedStrategyCourse_godot.Grid
{
  public class GridSystem
  {
    private readonly int width;
    private readonly int height;
    private readonly float cellSize;

    private readonly GridObject[,] grid;

    public GridSystem(int width, int height, float cellSize = 2f)
    {
      this.width = width;
      this.height = height;
      this.cellSize = cellSize;

      grid = new GridObject[width, height];

      for (var x = 0; x < width; ++x)
      {
        for (var z = 0; z < height; ++z)
        {
          grid[x, z] = new GridObject(this, new GridPosition(x, z));
        }
      }
    }

    private Vector3 GetWorldPosition(GridPosition position) => new Vector3(position.X, 0, position.Z) * cellSize;

    public GridPosition GetGridPosition(Vector3 worldPosition) =>
      new GridPosition(Mathf.RoundToInt(worldPosition.x / cellSize),
        Mathf.RoundToInt(worldPosition.z / cellSize));

    public GridObject GetGridObject(GridPosition position) => grid[position.X, position.Z];

    public void CreateDebugObjects(Node parent, PackedScene scene)
    {
      for (var x = 0; x < width; ++x)
      {
        for (var z = 0; z < height; ++z)
        {
          var gridPosition = new GridPosition(x, z);

          if (!(scene.Instance() is Spatial debug)) continue;
          
          debug.Name = $"{gridPosition.X}_{gridPosition.Z}";
          parent.AddChild(debug);
          debug.Translation = GetWorldPosition(gridPosition);
        }
      }
    }

    public bool IsValidPosition(GridPosition position) =>
      position.X >= 0 && position.X < width && position.Z >= 0 && position.Z < height;
  }
}