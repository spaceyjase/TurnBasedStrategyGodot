using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public class MoveAction : UnitAction
  {
    public override void _Ready()
    {
      base._Ready();

      OnEnter += () =>
      {
        unit.SetAnimation(UnitAnimations.Running);
      };
    }

    public override void Execute(float delta)
    {
      Move(delta);
    }

    public override string ActionName => "Move";

    private void Move(float delta)
    {
      if (unit.Translation.DistanceTo(unit.TargetPosition) > unit.StoppingDistance)
      {
        var moveDirection = (unit.TargetPosition - unit.Translation).Normalized();
        unit.Translation += moveDirection * (unit.MovementSpeed * delta);

        var newTransform = unit.Transform.LookingAt(unit.GlobalTransform.origin - moveDirection, Vector3.Up);
        unit.Transform = unit.Transform.InterpolateWith(newTransform, unit.RotateSpeed * delta);
      }
      else
      {
        unit.ChangeAction(nameof(IdleAction));
      }
    }
    
  }
}