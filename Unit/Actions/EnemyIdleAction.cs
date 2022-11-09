using System.Collections.Generic;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions;

public class EnemyIdleAction : UnitAction
{
  public override void _Ready()
  {
    base._Ready();
      
    OnEnter += () =>
    {
      Unit.SetAnimation(UnitAnimations.Idle);
    };
  }
    
  public override void Execute(float delta)
  {
    if (Unit.IsPlayerTurn) return;
      
    Unit.ChangeAction("TakingTurn");  // TODO: Enemy State Names
  }

  protected override IEnumerable<GridPosition> GetValidActionGridPositions() => new List<GridPosition>();
}