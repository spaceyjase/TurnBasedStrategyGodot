using System.Collections.Generic;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public class SpinAction : UnitAction
  {
    private float totalSpin;
    private readonly float spinLimit = Mathf.Deg2Rad(360f);

    public override void Execute(float delta)
    {
      var spinAmount = Mathf.Deg2Rad(360f * delta);
      totalSpin += spinAmount;

      unit.RotateY(spinAmount);

      if (!(totalSpin >= spinLimit)) return;

      totalSpin = 0f;

      unit.ChangeAction(unit.DefaultAction.ActionName);
    }

    protected override IEnumerable<GridPosition> GetValidActionGridPositions() =>
      new List<GridPosition>() { unit.GridPosition };

    public override int ActionPointCost => 2;
  }
}