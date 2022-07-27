using Godot;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Level
{
  public class LevelGrid : Node
  {
    [Export] private PackedScene debugScene;
    [Export] private bool showDebug;
    [Export] private int width = 10;
    [Export] private int height = 10;
    [Export] private float cellSize = 2f;
    
    private GridSystem gridSystem;

    public override void _Ready()
    {
      gridSystem = new GridSystem(width, height, cellSize);
      
      foreach (Unit.Unit unit in GetTree().GetNodesInGroup("Units"))
      {
        unit.Connect(nameof(Unit.Unit.OnUnitMoving), this, nameof(Unit.Unit.OnUnitMoving));
        AddUnitAtGridPosition(GetGridPosition(unit.GlobalTransform.origin), unit);
      }

      if (!showDebug) return;
      
      gridSystem.CreateDebugObjects(this, debugScene);
    }

    private void AddUnitAtGridPosition(GridPosition gridPosition, Unit.Unit unit)
    {
      var gridObject = gridSystem.GetGridObject(gridPosition);
      gridObject.AddUnit(unit);
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

    public bool IsValidPosition(GridPosition position) => gridSystem.IsValidPosition(position);

    public bool IsOccupied(GridPosition position) => !gridSystem.GetGridObject(position).IsEmpty();
  }
}