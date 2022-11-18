using System;
using System.Collections.Generic;
using System.Linq;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions;

public class InteractAction : UnitAction
{
  enum State
  {
    Idle,
    Interacting,
    Finished,
  }

  private State state;
  private float timer;

  public override void Execute(float delta)
  {
    switch (state)
    {
      case State.Idle:
        state = State.Interacting;
        timer = 0.5f; // TODO: magic
        Unit.LevelManager.Interact(Unit.TargetPosition);
        break;
      case State.Interacting:
        if (timer <= 0)
        {
          state = State.Finished;
        }
        else
        {
          timer -= delta;
        }
        break;
      case State.Finished:
        Unit.ChangeAction(Unit.DefaultAction.ActionName);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }
  
  protected override IEnumerable<GridPosition> GetValidActionGridPositions()
  {
    return from testPosition in GetAllGridPositions()
      where Unit.LevelManager.IsValidPosition(testPosition)
      where Unit.LevelManager.IsDoorAtPosition(testPosition)
      select testPosition;
  }

  private IEnumerable<GridPosition> GetAllGridPositions()
  {
    for (var x = -Unit.MaxMeleeDistance; x <= Unit.MaxMeleeDistance; ++x) // TODO: interaction distance
    {
      for (var z = -Unit.MaxMeleeDistance; z <= Unit.MaxMeleeDistance; ++z)
      {
        var offset = new GridPosition(x, z);

        yield return Unit.GridPosition + offset;
      }
    }
  }
}
