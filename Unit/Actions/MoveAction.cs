using System.Collections.Generic;
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
        Unit.SetAnimation(UnitAnimations.Running);
      };
    }

    public override void Execute(float delta)
    {
      Move(delta);
    }

    protected override IEnumerable<GridPosition> GetValidActionGridPositions()
    {
      for (var x = -Unit.MaxMoveDistance; x <= Unit.MaxMoveDistance; ++x)
      {
        for (var z = -Unit.MaxMoveDistance; z <= Unit.MaxMoveDistance; ++z)
        {
          var offset = new GridPosition(x, z);
          var testPosition = Unit.GridPosition + offset;
          
          var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
          if (testDistance > Unit.MaxMoveDistance) continue;

          if (!Unit.LevelGrid.IsValidPosition(testPosition)) continue;
          if (Unit.GridPosition == testPosition) continue;
          if (Unit.LevelGrid.IsOccupied(testPosition)) continue;

          yield return testPosition;
        }
      }
    }

    private void Move(float delta)
    {
      if (Unit.Translation.DistanceTo(Unit.TargetPosition) > Unit.StoppingDistance)
      {
        var moveDirection = Unit.Translation.DirectionTo(Unit.TargetPosition);
        Unit.Translation += moveDirection * (Unit.MovementSpeed * delta);

        var newTransform = Unit.Transform.LookingAt(Unit.GlobalTransform.origin - moveDirection, Vector3.Up);
        Unit.Transform = Unit.Transform.InterpolateWith(newTransform, Unit.RotateSpeed * delta);
      }
      else
      {
        Unit.ChangeAction(Unit.DefaultAction.ActionName);
      }
    }
  }
}