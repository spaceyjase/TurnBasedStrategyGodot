namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public class IdleAction : UnitAction
  {
    public override void _Ready()
    {
      base._Ready();
      
      OnEnter += () =>
      {
        unit.SetAnimation(UnitAnimations.Idle);
      };
    }

    public override void Execute(float delta)
    {
      // nothing!
    }

    public override string ActionName => "Idle";
  }
}