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
        unit.SetAnimation(UnitAnimations.Running);
      };
    }

    public override void Execute(float delta)
    {
      Move(delta);
    }

    protected override IEnumerable<GridPosition> GetValidActionGridPositions()
    {
      for (var x = -unit.MaxMoveDistance; x <= unit.MaxMoveDistance; ++x)
      {
        for (var z = -unit.MaxMoveDistance; z <= unit.MaxMoveDistance; ++z)
        {
          var offset = new GridPosition(x, z);
          var testPosition = unit.GridPosition + offset;
          
          var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
          if (testDistance > unit.MaxMoveDistance) continue;

          if (!unit.LevelGrid.IsValidPosition(testPosition)) continue;
          if (unit.GridPosition == testPosition) continue;
          if (unit.LevelGrid.IsOccupied(testPosition)) continue;

          yield return testPosition;
        }
      }
    }

    private void Move(float delta)
    {
      if (unit.Translation.DistanceTo(unit.TargetPosition) > unit.StoppingDistance)
      {
        var moveDirection = unit.Translation.DirectionTo(unit.TargetPosition);
        unit.Translation += moveDirection * (unit.MovementSpeed * delta);

        var newTransform = unit.Transform.LookingAt(unit.GlobalTransform.origin - moveDirection, Vector3.Up);
        unit.Transform = unit.Transform.InterpolateWith(newTransform, unit.RotateSpeed * delta);
      }
      else
      {
        unit.ChangeAction(unit.DefaultAction.ActionName);
      }
    }
  }
}