using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Unit.Ai;

namespace TurnBasedStrategyCourse_godot.Unit.Actions;

public class SwordAction : UnitAction
{
  [Export] private int damage = 200;
  [Export] private float shakeIntensity = 0.7f;
  [Export] private float slashTime = 0.5f;
  [Export] private float cooldownTime = 0.9f;

  private Unit target;
  private State state;
  private float timer;

  private enum State
  {
    Idle,
    Turning,
    Attack,
    Swinging,
    Hit,
    Cooldown,
  }

  public override void _Ready()
  {
    base._Ready();

    OnEnter += () =>
    {
      state = State.Turning;
      target = Unit.LevelManager.GetUnitAtPosition(
        Unit.LevelManager.GetGridPosition(Unit.TargetPosition));
    };
    
    OnExit += () =>
    {
      Unit.ChangeToDefaultWeapon();
    };
  }

  public override void Execute(float delta)
  {
    Attack(delta);
  }

  private void Attack(float delta)
  {
    switch (state)
    {
      case State.Turning:
        var aimDirection = Unit.Translation.DirectionTo(Unit.TargetPosition);
        var newTransform = Unit.Transform.LookingAt(Unit.GlobalTransform.origin - aimDirection, Vector3.Up);
        Unit.Transform = Unit.Transform.InterpolateWith(newTransform, Unit.RotateSpeed * delta);
        if (Unit.Transform.IsEqualApprox(newTransform))
        {
          state = State.Attack;
        }
        break;
      case State.Attack:
        Unit.ChangeToMeleeWeapon();
        Unit.SetAnimation(UnitAnimations.Slash);
        state = State.Swinging;
        timer = slashTime;
        break;
      case State.Swinging:
        timer -= delta;
        if (timer <= 0)
        {
          state = State.Hit;
        }
        break;
      case State.Hit:
        target.Damage(damage);
        EventBus.Instance.EmitSignal(nameof(EventBus.CameraShake), shakeIntensity);
        timer = cooldownTime;
        state = State.Cooldown;
        break;
      case State.Cooldown:
        timer -= delta;
        if (timer <= 0)
        {
          state = State.Idle;
        }
        break;
      case State.Idle:
        Unit.ChangeAction(Unit.DefaultAction.Name);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  protected override EnemyAiAction GetEnemyAiActionForPosition(GridPosition gridPosition)
  {
    return new EnemyAiAction
    {
      GridPosition = gridPosition,
      Score = 200, // TODO: magic numbers
    };
  }

  private IEnumerable<GridPosition> GetAllGridPositions()
  {
    for (var x = -Unit.MaxMeleeDistance; x <= Unit.MaxMeleeDistance; ++x)
    {
      for (var z = -Unit.MaxMeleeDistance; z <= Unit.MaxMeleeDistance; ++z)
      {
        var offset = new GridPosition(x, z);

        yield return Unit.GridPosition + offset;
      }
    }
  }

  public override IEnumerable<GridPosition> ValidRangePositions => GetActionRangeGridPositions();

  private IEnumerable<GridPosition> GetActionRangeGridPositions()
  {
    var actionPositions = GetValidActionGridPositions().ToArray();
    foreach (var testPosition in GetAllGridPositions())
    {
      if (actionPositions.Contains(testPosition)) continue;
      if (!Unit.LevelManager.IsValidPosition(testPosition)) continue;
      if (Unit.GridPosition == testPosition) continue;
      if (Unit.LevelManager.IsOccupied(testPosition)) continue;

      yield return testPosition;
    }
  }

  protected override IEnumerable<GridPosition> GetValidActionGridPositions() =>
    from testPosition in GetAllGridPositions()
    where Unit.LevelManager.IsValidPosition(testPosition)
    where Unit.LevelManager.IsOccupied(testPosition)
    let targetUnit = Unit.LevelManager.GetUnitAtPosition(testPosition)
    where targetUnit != null
    where targetUnit.IsEnemy != Unit.IsEnemy
    select testPosition;
}