using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Unit.Ai;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public class MoveAction : UnitAction
  {
    public override void _Ready()
    {
      base._Ready();

      OnEnter += () => { Unit.SetAnimation(UnitAnimations.Running); };
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
      return from testPosition in GetAllMovePositions(Unit.GridPosition)
        where Unit.LevelGrid.IsValidPosition(testPosition)
        where Unit.GridPosition != testPosition
        where !Unit.LevelGrid.IsOccupied(testPosition)
        select testPosition;
    }

    protected override EnemyAiAction GetEnemyAiActionForPosition(GridPosition gridPosition)
    {
      var cost = (from testPosition in GetAllMovePositions(gridPosition)
        where Unit.LevelGrid.IsValidPosition(testPosition)
        where Unit.LevelGrid.IsOccupied(testPosition)
        let targetUnit = Unit.LevelGrid.GetUnitAtPosition(testPosition)
        where targetUnit.IsEnemy != Unit.IsEnemy
        select testPosition).Count();

      return new EnemyAiAction
      {
        GridPosition = gridPosition,
        Score = cost * 10,  // TODO: magic number
      };
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