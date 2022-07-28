using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public class MoveAction : UnitAction
  {
    // TODO: could use a resource here, perhaps belongs to the parent unit
    [Export] private float movementSpeed = 4f;
    [Export] private float rotateSpeed = 15f;
    [Export] private float stoppingDistance = .1f;
    [Export] private int maxMoveDistance = 4;

    private Vector3 targetPosition = Vector3.Zero;

    public override void _Ready()
    {
      base._Ready();

      targetPosition = unit.Translation;
    }

    public void Move(float delta)
    {
      if (unit.Translation.DistanceTo(targetPosition) > stoppingDistance)
      {
        var moveDirection = (targetPosition - unit.Translation).Normalized();
        unit.Translation += moveDirection * (movementSpeed * delta);

        var newTransform = unit.Transform.LookingAt(unit.GlobalTransform.origin - moveDirection, Vector3.Up);
        unit.Transform = unit.Transform.InterpolateWith(newTransform, rotateSpeed * delta);

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

    private IEnumerable<GridPosition> GetValidGridPosition()
    {
      for (var x = -maxMoveDistance; x <= maxMoveDistance; ++x)
      {
        for (var z = -maxMoveDistance; z <= maxMoveDistance; ++z)
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