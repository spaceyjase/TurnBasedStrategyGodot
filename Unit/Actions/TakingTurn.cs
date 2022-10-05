using System.Collections.Generic;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;
using Timer = System.Timers.Timer;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public class TakingTurn : UnitAction
  {
    [Export] private float turnIntervalMs = 500;

    private Timer enemyTimer;

    public override void _Ready()
    {
      base._Ready();

      enemyTimer = new Timer(turnIntervalMs);
      enemyTimer.Elapsed += (sender, args) =>
      {
        enemyTimer.Stop();
        TakeAiAction();
      };

      OnEnter += () => { Unit.SetAnimation(UnitAnimations.Idle); };
    }

    private void TakeAiAction()
    {
      if (Unit.TryChangeAction("Spin")) return;

      Unit.EmitSignal(nameof(Unit.FinishedAiTurn));

      Unit.ChangeAction(Unit.DefaultAction.ActionName);
    }

    public override void Execute(float delta)
    {
      if (Unit.IsPlayerTurn) return;

      // TODO: transition to next state
      enemyTimer.Start();
    }

    protected override IEnumerable<GridPosition> GetValidActionGridPositions() => new List<GridPosition>();
  }
}