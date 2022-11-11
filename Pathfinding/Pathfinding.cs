using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Level;
using TurnBasedStrategyCourse_godot.Unit.Actions;

namespace TurnBasedStrategyCourse_godot.Pathfinding
{
  public class Pathfinding : Navigation
  {
    private LevelManager levelGrid;
    private NavigationMeshInstance navigationMeshInstance;
    private bool bake;

    private void OnObstacleDestroyed(Spatial destroyable)
    {
      bake = true;
    }
    
    private void OnActionCompleted(UnitAction action)
    {
      if (!bake) return;
      
      BakeNavigationMesh();
      bake = false;
    }

    public override void _Ready()
    {
      base._Ready();
      
      EventBus.Instance.Connect(nameof(EventBus.ObstacleDestroyed), this, nameof(OnObstacleDestroyed));
      EventBus.Instance.Connect(nameof(EventBus.ActionCompleted), this, nameof(OnActionCompleted));
      
      navigationMeshInstance = GetNode<NavigationMeshInstance>("NavigationMeshInstance");
    }

    private void BakeNavigationMesh()
    {
      navigationMeshInstance.BakeNavigationMesh();
    }

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