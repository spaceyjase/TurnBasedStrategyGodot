using System.Collections.Generic;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Level
{
  public class LevelGrid : Node
  {
    [Export] private PackedScene debugScene;
    
    private GridSystem gridSystem;

    public override void _Ready()
    {
      gridSystem = new GridSystem(10, 10);
      
      foreach (Unit.Unit unit in GetTree().GetNodesInGroup("Units"))
      {
        unit.Connect(nameof(Unit.Unit.OnUnitMoving), this, nameof(Unit.Unit.OnUnitMoving));
        AddUnitAtGridPosition(GetGridPosition(unit.GlobalTransform.origin), unit);
      }

      gridSystem.CreateDebugObjects(this, debugScene);
    }

    public void AddUnitAtGridPosition(GridPosition gridPosition, Unit.Unit unit)
    {
      var gridObject = gridSystem.GetGridObject(gridPosition);
      gridObject.AddUnit(unit);
      GD.Print($"{unit.Name} added at {gridPosition}");
    }

    public IEnumerable<Unit.Unit> GetUnitListAtGridPosition(GridPosition gridPosition)
    {
      var gridObject = gridSystem.GetGridObject(gridPosition);
      return gridObject.GetUnitList();
    }

    private void RemoveUnitAsGridPosition(GridPosition gridPosition, Unit.Unit unit)
    {
      var gridObject = gridSystem.GetGridObject(gridPosition);
      gridObject.RemoveUnit(unit);
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);

    private void OnUnitMoving(Unit.Unit unit, GridPosition oldPosition, GridPosition newPosition)
    {
      RemoveUnitAsGridPosition(oldPosition, unit);
      AddUnitAtGridPosition(newPosition, unit);
    }
  }
}