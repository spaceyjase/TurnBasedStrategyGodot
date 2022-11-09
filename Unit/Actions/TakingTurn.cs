using System.Collections.Generic;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Unit.Ai;

namespace TurnBasedStrategyCourse_godot.Unit.Actions;

public class TakingTurn : UnitAction
{
  public override void _Ready()
  {
    base._Ready();

    OnEnter += () => { Unit.SetAnimation(UnitAnimations.Idle); };
  }

  private void TakeAiAction()
  {
    EnemyAiAction? bestAction = null;
    UnitAction bestUnitAction = null;
    foreach (var unitAction in Unit.Actions)
    {
      if (Unit.DefaultAction == unitAction) continue;
      if (this == unitAction) continue;

      if (bestAction == null)
      {
        bestAction = unitAction.GetBestEnemyAiAction();
        bestUnitAction = unitAction;
      }
      else
      {
        var candidateAction = unitAction.GetBestEnemyAiAction();
        if (candidateAction == null || candidateAction.Value.Score <= bestAction.Value.Score) continue;
          
        bestAction = candidateAction;
        bestUnitAction = unitAction;
      }
    }

    if (bestAction != null)
    {
      Unit.TrySetTargetPositionForAction(bestUnitAction, bestAction.Value.GridPosition);
      if (Unit.TryChangeAction(bestUnitAction.ActionName)) return;
    }

    Unit.EmitSignal(nameof(Unit.FinishedAiTurn));

    Unit.ChangeAction(Unit.DefaultAction.ActionName);
  }

  public override void Execute(float delta)
  {
    if (Unit.IsPlayerTurn) return;
      
    TakeAiAction();
  }

  protected override IEnumerable<GridPosition> GetValidActionGridPositions() => new List<GridPosition>();
}