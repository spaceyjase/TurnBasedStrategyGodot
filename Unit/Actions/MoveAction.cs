using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Unit.Ai;

namespace TurnBasedStrategyCourse_godot.Unit.Actions;

public class MoveAction : UnitAction
{
  private IEnumerable<GridPosition> targetPositions;
  private IEnumerator<GridPosition> currentTargetPosition;

  public override void _Ready()
  {
    base._Ready();

    OnEnter += () =>
    {
      Unit.SetAnimation(UnitAnimations.Running);

      targetPositions = Unit.LevelManager.CalculatePath(Unit.GridPosition, Unit.TargetPosition) ??
                        System.Array.Empty<GridPosition>();
      currentTargetPosition = targetPositions.GetEnumerator();
      currentTargetPosition.MoveNext();
    };
  }

  public override void Execute(float delta)
  {
    Move(delta);
  }

  private IEnumerable<GridPosition> GetAllMovePositions(GridPosition gridPosition)
  {
    for (var x = -Unit.MaxMoveDistance; x <= Unit.MaxMoveDistance; ++x)
    {
      for (var z = -Unit.MaxMoveDistance; z <= Unit.MaxMoveDistance; ++z)
      {
        var offset = new GridPosition(x, z);
        var testPosition = gridPosition + offset;

        var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
        if (testDistance > Unit.MaxMoveDistance) continue;

        yield return testPosition;
      }
    }
  }

  protected override IEnumerable<GridPosition> GetValidActionGridPositions()
  {
    var positions = GetAllMovePositions(Unit.GridPosition);
    var gridPositions = positions as GridPosition[] ?? positions.ToArray();
    return from testPosition in gridPositions
      where Unit.LevelManager.IsValidPosition(testPosition)
      where Unit.GridPosition != testPosition
      where !Unit.LevelManager.IsOccupied(testPosition)
      where Unit.LevelManager.HasPath(Unit.GridPosition, testPosition, gridPositions)
      select testPosition;
  }

  protected override EnemyAiAction GetEnemyAiActionForPosition(GridPosition gridPosition)
  {
    var cost = (from testPosition in GetAllMovePositions(gridPosition)
      where Unit.LevelManager.IsValidPosition(testPosition)
      where Unit.LevelManager.IsOccupied(testPosition)
      let targetUnit = Unit.LevelManager.GetUnitAtPosition(testPosition)
      where targetUnit.IsEnemy != Unit.IsEnemy
      select testPosition).Count();

    return new EnemyAiAction
    {
      GridPosition = gridPosition,
      Score = cost * 10, // TODO: magic number
    };
  }

  private void Move(float delta)
  {
    var targetPosition = Unit.LevelManager.GetWorldPosition(currentTargetPosition.Current);
    if (Unit.Translation.DistanceTo(targetPosition) > Unit.StoppingDistance)
    {
      Unit.Translation = Unit.Translation.MoveToward(targetPosition, Unit.MovementSpeed * delta);

      var moveDirection = Unit.Translation.DirectionTo(targetPosition);
      var newTransform = Unit.Transform.LookingAt(Unit.GlobalTransform.origin - moveDirection, Vector3.Up);
      Unit.Transform = Unit.Transform.InterpolateWith(newTransform, Unit.RotateSpeed * delta);
    }
    else if (currentTargetPosition.Current == targetPositions.Last())
    {
      Unit.ChangeAction(Unit.DefaultAction.ActionName);
    }
    else
    {
      currentTargetPosition.MoveNext();
    }
  }
}