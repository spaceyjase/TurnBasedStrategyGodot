using System.Collections.Generic;
using System.Linq;

namespace TurnBasedStrategyCourse_godot.Grid
{
  public class GridObject
  {
    private readonly GridPosition gridPosition;

    private readonly List<Unit.Unit> units = new List<Unit.Unit>();
    private GridSystem gridSystem;

    public GridObject(GridSystem system, GridPosition position)
    {
      gridSystem = system;
      gridPosition = position;
    }

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

    public bool IsEmpty() => units.Count == 0;
  }
}