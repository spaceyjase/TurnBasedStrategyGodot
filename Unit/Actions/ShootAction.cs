using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Unit.Ai;

namespace TurnBasedStrategyCourse_godot.Unit.Actions;

public class ShootAction : UnitAction
{
  [Export] private PackedScene bulletScene;
  [Export] private int damage;
  [Export] private float shakeIntensity = 0.7f;

  private const float aimTime = 1f;
  private const float shootTime = 0.5f;
  private const float coolOffTime = 0.1f;

  private enum State
  {
    Aiming,
    Shooting,
    CoolOff,
  }

  private State state;
  private float timer;
  private Unit target;
  private bool canShoot;

  public override void _Ready()
  {
    base._Ready();

    OnEnter += () =>
    {
      state = State.Aiming;
      timer = aimTime;
      target = Unit.LevelManager.GetUnitAtPosition(
        Unit.LevelManager.GetGridPosition(Unit.TargetPosition));
      canShoot = true;
      Unit.ChangeToGunWeapon();
    };
    
    OnExit += () =>
    {
      Unit.ChangeToDefaultWeapon();
    };
  }

  public override void Execute(float delta)
  {
    Shoot(delta);
  }

  public override IEnumerable<GridPosition> ValidRangePositions => GetActionRangeGridPositions();

  private IEnumerable<GridPosition> GetAllGridPositions()
  {
    for (var x = -Unit.MaxShootDistance; x <= Unit.MaxShootDistance; ++x)
    {
      for (var z = -Unit.MaxShootDistance; z <= Unit.MaxShootDistance; ++z)
      {
        var offset = new GridPosition(x, z);

        var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
        if (testDistance > Unit.MaxShootDistance) continue;

        yield return Unit.GridPosition + offset;
      }
    }
  }

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

  protected override IEnumerable<GridPosition> GetValidActionGridPositions()
  {
    return from testPosition in GetAllGridPositions()
      where Unit.LevelManager.IsValidPosition(testPosition)
      where Unit.LevelManager.IsOccupied(testPosition)
      where Unit.HasLineOfSight(testPosition)
      let targetUnit = Unit.LevelManager.GetUnitAtPosition(testPosition)
      where targetUnit != null
      where targetUnit.IsEnemy != Unit.IsEnemy
      select testPosition;
  }

  protected override EnemyAiAction GetEnemyAiActionForPosition(GridPosition gridPosition)
  {
    var unit = Unit.LevelManager.GetUnitAtPosition(gridPosition);
    return new EnemyAiAction()
    {
      GridPosition = gridPosition,
      Score = 100 + Mathf.RoundToInt((1 - (float)unit.CurrentHealth / unit.MaxHealth) * 100f),
    };
  }

  private void Shoot(float delta)
  {
    timer -= delta;

    switch (state)
    {
      case State.Aiming:
        var aimDirection = Unit.Translation.DirectionTo(Unit.TargetPosition);
        var newTransform = Unit.Transform.LookingAt(Unit.GlobalTransform.origin - aimDirection, Vector3.Up);
        Unit.Transform = Unit.Transform.InterpolateWith(newTransform, Unit.RotateSpeed * delta);
        break;
      case State.Shooting:
        if (canShoot)
        {
          canShoot = false;
          ShootTarget();
        }
        break;
      case State.CoolOff:
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }

    if (timer <= 0f)
    {
      NextState();
    }
  }

  private void ShootTarget()
  {
    var bullet = bulletScene.Instance<Bullet.Bullet>();

    bullet.Init(Unit.BulletSpawnPosition, target.GlobalTranslation);

    GetTree().Root.AddChild(bullet);

    target.Damage(damage);
    EventBus.Instance.EmitSignal(nameof(EventBus.CameraShake), shakeIntensity);
  }

  private void NextState()
  {
    switch (state)
    {
      case State.Aiming:
        state = State.Shooting;
        timer = shootTime;
        break;
      case State.Shooting:
        Unit.SetAnimation(UnitAnimations.Shooting);
        state = State.CoolOff;
        timer = coolOffTime;
        break;
      case State.CoolOff:
        Unit.ChangeAction(Unit.DefaultAction.ActionName);
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }
}