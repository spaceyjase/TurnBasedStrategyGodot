using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public abstract class UnitAction : Node
  {
    public Unit Unit { get; protected set; }

    protected Action OnEnter { get; set; }
    private Action OnExit { get; set; }

    public override void _Ready()
    {
      base._Ready();

      Unit = Owner as Unit;
    }
    
    public void Enter()
    {
      EventBus.Instance.EmitSignal(nameof(EventBus.ActionStarted), this);
      
      OnEnter?.Invoke();
    }

    public void Exit()
    {
      EventBus.Instance.EmitSignal(nameof(EventBus.ActionCompleted), this);
      
      OnExit?.Invoke();
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