using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public class MoveAction : UnitAction
  {
    private Vector3 targetPosition = Vector3.Zero;

    public override void _Ready()
    {
      base._Ready();

      targetPosition = unit.Translation;
    }

    public void Move(float delta)
    {
      if (unit.Translation.DistanceTo(targetPosition) > unit.StoppingDistance)
      {
        var moveDirection = (targetPosition - unit.Translation).Normalized();
        unit.Translation += moveDirection * (unit.MovementSpeed * delta);

        var newTransform = unit.Transform.LookingAt(unit.GlobalTransform.origin - moveDirection, Vector3.Up);
        unit.Transform = unit.Transform.InterpolateWith(newTransform, unit.RotateSpeed * delta);

        unit.SetAnimation(UnitAnimations.Running);
      }
      else
      {
        unit.SetAnimation(UnitAnimations.Idle);
      }
    }

    public void SetMovementTarget(Vector3 direction)
    {
      if (!IsValidGridPosition(unit.LevelGrid.GetGridPosition(direction))) return;

      targetPosition = direction;
    }

    private bool IsValidGridPosition(GridPosition position)
    {
      return GetValidGridPosition().Contains(position);
    }

    public IEnumerable<GridPosition> GetValidGridPosition()
    {
      for (var x = -unit.MaxMoveDistance; x <= unit.MaxMoveDistance; ++x)
      {
        for (var z = -unit.MaxMoveDistance; z <= unit.MaxMoveDistance; ++z)
        {
          var offset = new GridPosition(x, z);
          var testPosition = unit.GridPosition + offset;

          if (!unit.LevelGrid.IsValidPosition(testPosition)) continue;
          if (unit.GridPosition == testPosition) continue;
          if (unit.LevelGrid.IsOccupied(testPosition)) continue;

          yield return testPosition;
        }
      }
    }
  }
}