using Godot;

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
      
      unit.ChangeAction(unit.IdleAction.ActionName);
    }

    public override int ActionPointCost => 2;
  }
}