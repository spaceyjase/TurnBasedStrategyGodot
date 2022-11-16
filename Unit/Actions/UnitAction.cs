using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Unit.Ai;

namespace TurnBasedStrategyCourse_godot.Unit.Actions;

public abstract class UnitAction : Node
{
  [Export] protected SpatialMaterial baseColorMaterial;
  [Export] protected SpatialMaterial baseRangeColorMaterial;

  public Unit Unit { get; private set; }

  protected Action OnEnter { get; set; }
  protected Action OnExit { get; set; }

  public SpatialMaterial RangeColour => baseRangeColorMaterial;
  public SpatialMaterial Colour => baseColorMaterial;

  public virtual IEnumerable<GridPosition> ValidRangePositions => new List<GridPosition>();
  public IEnumerable<GridPosition> ValidPositions => GetValidActionGridPositions();

  public string ActionName => Name;

  public virtual int ActionPointCost => 1;

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


  public bool IsValidGridPosition(GridPosition position)
  {
    return GetValidActionGridPositions().Contains(position);
  }

  public EnemyAiAction? GetBestEnemyAiAction()
  {
    var enemyAiActions = GetValidActionGridPositions().Select(GetEnemyAiActionForPosition).ToList();

    if (!enemyAiActions.Any()) return null;
      
    enemyAiActions.Sort((a, b) => b.Score - a.Score);
    return enemyAiActions.First();
  }

  public abstract void Execute(float delta);
    
  protected abstract IEnumerable<GridPosition> GetValidActionGridPositions();
    
  protected virtual EnemyAiAction GetEnemyAiActionForPosition(GridPosition gridPosition) => new()
  {
    GridPosition = gridPosition,
    Score = -1,
  };
}