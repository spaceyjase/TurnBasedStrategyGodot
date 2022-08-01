using System.Collections.Generic;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Level
{
  public class LevelGrid : Node
  {
    [Export] private int width = 10;
    [Export] private int height = 10;
    [Export] private float cellSize = 2f;
    [Export] private PackedScene cellScene;

    private GridSystem gridSystem;
    private GridCell[] cells;

    public override void _Ready()
    {
      gridSystem = new GridSystem(width, height, cellSize);
      cells = new GridCell[width * height];

      foreach (Unit.Unit unit in GetTree().GetNodesInGroup("Units"))
      {
        unit.Connect(nameof(Unit.Unit.OnUnitMoving), this, nameof(OnUnitMoving));
        unit.Connect(nameof(Unit.Unit.OnUnitSelected), this, nameof(OnUnitSelected));
        AddUnitAtGridPosition(GetGridPosition(unit.GlobalTransform.origin), unit);
      }

      CreateVisualElements();
    }

    private void CreateVisualElements()
    {
      for (var x = 0; x < width; ++x)
      {
        for (var z = 0; z < height; ++z)
        {
          var gridPosition = new GridPosition(x, z);
          var cell = cellScene.Instance<GridCell>();
          cell.Translation = gridSystem.GetWorldPosition(gridPosition);
          cells[z * width + x] = cell;
          AddChild(cell);
        }
      }
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
    public Vector3 GetWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);

    private void OnUnitMoving(Unit.Unit unit, GridPosition oldPosition, GridPosition newPosition)
    {
      HideAllGridPositions();
      RemoveUnitAsGridPosition(oldPosition, unit);
      AddUnitAtGridPosition(newPosition, unit);
    }

    private void OnUnitSelected(Unit.Unit unit)
    {
      ShowUnitRange(unit);
    }

    public bool IsValidPosition(GridPosition position) => gridSystem.IsValidPosition(position);

    public bool IsOccupied(GridPosition position) => !gridSystem.GetGridObject(position).IsEmpty();

    public void ShowUnitRange(Unit.Unit unit)
    {
      HideAllGridPositions();
      ShowUnitGridPositions(unit.ValidPositions);
    }

    private void ShowUnitGridPositions(IEnumerable<GridPosition> positions)
    {
      foreach (var gridPosition in positions)
      {
        cells[gridPosition.Z * width + gridPosition.X].Visible = true;
      }
    }

    private void HideAllGridPositions()
    {
      foreach (var cell in cells)
      {
        cell.Visible = false;
      }
    }
  }
}