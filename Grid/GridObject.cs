using System.Collections.Generic;
using System.Linq;

namespace TurnBasedStrategyCourse_godot.Grid;

public class GridObject
{
  private readonly GridPosition gridPosition;

  private readonly List<Unit.Unit> units = new();

  public GridObject(GridPosition position)
  {
    gridPosition = position;
  }

  public Door.Door Door { get; set; }  // TODO: is there a unit here? ignore (or throw?)

  public void AddUnit(Unit.Unit unit)
  {
    units.Add(unit);
  }

  public void RemoveUnit(Unit.Unit unit)
  {
    units.Remove(unit);
  }

  public IEnumerable<Unit.Unit> GetUnitList() => units;

  public override string ToString() => $"{gridPosition}\n{string.Join("\n", units.Select(u => u.Name))}";

  public bool IsEmpty() => (units.Count == 0 && Door == null) || Door?.IsOpen == true;
}