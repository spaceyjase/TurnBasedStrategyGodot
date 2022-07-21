using System.Collections.Generic;
using System.Linq;

namespace TurnBasedStrategyCourse_godot.Grid
{
  public class GridObject
  {
    private GridSystem gridSystem;
    private GridPosition gridPosition;

    private readonly List<Unit.Unit> units = new List<Unit.Unit>();

    public void AddUnit(Unit.Unit unit)
    {
      units.Add(unit);
    }

    public void RemoveUnit(Unit.Unit unit)
    {
      units.Remove(unit);
    }

    public IEnumerable<Unit.Unit> GetUnitList() => units;

    public GridObject(GridSystem system, GridPosition position)
    {
      gridSystem = system;
      gridPosition = position;
    }

    public override string ToString()
    {
      return $"{gridPosition.ToString()}\n{string.Join("\n", units.Select(u => u.Name))}";
    }
  }
}