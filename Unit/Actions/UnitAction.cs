using Godot;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public class UnitAction : Node
  {
    protected Unit unit;

    public override void _Ready()
    {
      base._Ready();

      unit = Owner as Unit;
    }
  }
}