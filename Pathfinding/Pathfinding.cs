using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Level;

namespace TurnBasedStrategyCourse_godot.Pathfinding
{
  public class Pathfinding : Navigation
  {
    private LevelManager levelGrid;

    public IEnumerable<GridPosition> FindPath(GridPosition start, GridPosition end)
    {
      var path = GetSimplePath(levelGrid.GetWorldPosition(start), levelGrid.GetWorldPosition(end));
      return path.Select(p => levelGrid.GetGridPosition(p));
    }

    public void Init(LevelManager level)
    {
      levelGrid = level;
    }

    public bool HasPath(GridPosition start, GridPosition end, IEnumerable<GridPosition> gridPositions)
    {
      var path = FindPath(start, end);
      var enumerable = path as GridPosition[] ?? path.ToArray();
      return enumerable.Contains(end) && enumerable.All(gridPositions.Contains);
    }
  }
}