using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Unit.Actions;

namespace TurnBasedStrategyCourse_godot.Level;

  public class LevelManager : Node
  {
    [Signal]
    public delegate void GroundClicked(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx);

    private GridCell[] cells;

    [Export] private PackedScene cellScene;
    [Export] private float cellSize = 2f;

    private GridSystem<GridObject> gridSystem;
    [Export] private int height = 10;
    private Pathfinding.Pathfinding pathFinding;
    [Export] private int width = 10;

    public override void _Ready()
    {
      gridSystem = new GridSystem<GridObject>(width, height, cellSize,
        (system, position) => new GridObject(position));
      cells = new GridCell[width * height];

      ConfigureUnits();
      ConfigureDoors();
      
      EventBus.Instance.Connect(nameof(EventBus.TurnChanged), this, nameof(OnTurnChanged));

      CreateVisualElements();

      pathFinding = GetNode<Pathfinding.Pathfinding>("Pathfinding");
      pathFinding.Init(this);
    }

    private void ConfigureDoors()
    {
      foreach (Door.Door door in GetTree().GetNodesInGroup("Doors"))
      {
        AddDoorAtGridPosition(GetGridPosition(door.GlobalTransform.origin), door);
      }
    }

    private void ConfigureUnits()
    {
      foreach (Unit.Unit unit in GetTree().GetNodesInGroup("Units"))
      {
        unit.Connect(nameof(Unit.Unit.Moving), this, nameof(OnUnitMoving));
        unit.Connect(nameof(Unit.Unit.Dead), this, nameof(OnUnitDead));
        AddUnitAtGridPosition(GetGridPosition(unit.GlobalTransform.origin), unit);
      }
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
          cell.Visible = false;
        }
      }
    }

    private void AddUnitAtGridPosition(GridPosition gridPosition, Unit.Unit unit)
    {
      var gridObject = gridSystem.GetGridObject(gridPosition);
      gridObject.AddUnit(unit);
    }

    private void RemoveUnitAtGridPosition(GridPosition gridPosition, Unit.Unit unit)
    {
      var gridObject = gridSystem.GetGridObject(gridPosition);
      gridObject.RemoveUnit(unit);
      HideGridPositions(new[] { gridPosition });
    }

    public GridPosition GetGridPosition(Vector3 worldPosition) => gridSystem.GetGridPosition(worldPosition);
    public Vector3 GetWorldPosition(GridPosition gridPosition) => gridSystem.GetWorldPosition(gridPosition);

    private void OnUnitMoving(Unit.Unit unit, GridPosition oldPosition, GridPosition newPosition)
    {
      RemoveUnitAtGridPosition(oldPosition, unit);
      AddUnitAtGridPosition(newPosition, unit);
      if (unit.IsEnemy) return;
      ShowUnitActionRange(unit.CurrentAction);
    }

    private void OnUnitDead(Unit.Unit unit)
    {
      RemoveUnitAtGridPosition(unit.GridPosition, unit);
    }

    // ReSharper disable once UnusedMember.Local
    private void OnUnitManagerActionSelected(UnitAction action) => ShowUnitActionRange(action);

    // ReSharper disable once UnusedMember.Local
    private void OnUnitManagerUnitSelected(Unit.Unit unit) => HideAllGridPositions();

    public bool IsValidPosition(GridPosition position) => gridSystem.IsValidPosition(position);

    public bool IsOccupied(GridPosition position) => !gridSystem.GetGridObject(position).IsEmpty();

    public Unit.Unit GetUnitAtPosition(GridPosition position) =>
      gridSystem.GetGridObject(position).GetUnitList().FirstOrDefault();

    private void ShowUnitActionRange(UnitAction action)
    {
      HideAllGridPositions();
      HighlightGridPositions(action.ValidRangePositions, action.RangeColour);
      HighlightGridPositions(action.ValidPositions, action.Colour);
    }

    private void HighlightGridPositions(IEnumerable<GridPosition> positions, SpatialMaterial material)
    {
      foreach (var gridPosition in positions)
      {
        var cell = cells[gridPosition.Z * width + gridPosition.X];
        cell.Visible = true;
        cell.SetMaterial(material);
      }
    }

    private void HideGridPositions(IEnumerable<GridPosition> positions)
    {
      foreach (var gridPosition in positions)
      {
        cells[gridPosition.Z * width + gridPosition.X].Visible = false;
      }
    }

    private void HideAllGridPositions()
    {
      foreach (var cell in cells)
      {
        cell.Visible = false;
      }
    }

    private void OnTurnChanged(int turn, bool isPlayerTurn)
    {
      HideAllGridPositions();
    }

    private void _on_Ground_input_event(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
    {
      EmitSignal(nameof(GroundClicked), camera, @event, position, normal, shape_idx);
    }

    public IEnumerable<GridPosition> CalculatePath(GridPosition startGridPosition, Vector3 endPosition) =>
      pathFinding.FindPath(startGridPosition, GetGridPosition(endPosition));

    public bool HasPath(GridPosition start, GridPosition end, IEnumerable<GridPosition> positions) =>
      pathFinding.HasPath(start, end, positions);

    private Door.Door GetDoorAtGridPosition(GridPosition gridPosition) => gridSystem?.GetGridObject(gridPosition).Door;

    private void AddDoorAtGridPosition(GridPosition gridPosition, Door.Door door)
    {
      var gridObject = gridSystem.GetGridObject(gridPosition);
      gridObject.Door = door;
    }

    public bool IsDoorAtPosition(GridPosition testPosition) => GetDoorAtGridPosition(testPosition) != null;

    public void Interact(Vector3 position)
    {
      GetDoorAtGridPosition(GetGridPosition(position))?.Interact();
    }
  }