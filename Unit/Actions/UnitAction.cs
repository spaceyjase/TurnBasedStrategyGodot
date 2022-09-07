using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public abstract class UnitAction : Node
  {
    protected Unit unit;
    
    public Action OnEnter { get; set; }
    public Action OnExit { get; set; }

    public override void _Ready()
    {
      base._Ready();

      unit = Owner as Unit;
    }

    public abstract void Execute(float delta);
    protected abstract IEnumerable<GridPosition> GetValidActionGridPositions();
    
    public bool IsValidGridPosition(GridPosition position)
    {
      return GetValidActionGridPositions().Contains(position);
    }
    
    public IEnumerable<GridPosition> ValidPositions => GetValidActionGridPositions();

    public string ActionName => Name;

    public virtual int ActionPointCost => 1;
  }
}