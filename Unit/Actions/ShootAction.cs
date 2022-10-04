using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public class ShootAction : UnitAction
  {
    [Export] private PackedScene bulletScene;
    [Export] private int damage;

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
        target = Unit.LevelGrid.GetUnitAtPosition(
          Unit.LevelGrid.GetGridPosition(Unit.TargetPosition));
        canShoot = true;
      };
    }

    public override void Execute(float delta)
    {
      Shoot(delta);
    }

    public override IEnumerable<GridPosition> ValidRangePositions => GetActionRangeGridPositions();

    private IEnumerable<GridPosition> GetActionRangeGridPositions()
    {
      var actionPositions = GetValidActionGridPositions().ToArray();
      for (var x = -Unit.MaxShootDistance; x <= Unit.MaxShootDistance; ++x)
      {
        for (var z = -Unit.MaxShootDistance; z <= Unit.MaxShootDistance; ++z)
        {
          var offset = new GridPosition(x, z);
          var testPosition = Unit.GridPosition + offset;
          
          var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
          if (testDistance > Unit.MaxShootDistance) continue;

          if (actionPositions.Contains(testPosition)) continue;
          if (!Unit.LevelGrid.IsValidPosition(testPosition)) continue;
          if (Unit.GridPosition == testPosition) continue;
          if (Unit.LevelGrid.IsOccupied(testPosition)) continue;

          yield return testPosition;
        }
      }
    }

    protected override IEnumerable<GridPosition> GetValidActionGridPositions()
    {
      for (var x = -Unit.MaxShootDistance; x <= Unit.MaxShootDistance; ++x)
      {
        for (var z = -Unit.MaxShootDistance; z <= Unit.MaxShootDistance; ++z)
        {
          var offset = new GridPosition(x, z);
          var testPosition = Unit.GridPosition + offset;

          var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
          if (testDistance > Unit.MaxShootDistance) continue;

          if (!Unit.LevelGrid.IsValidPosition(testPosition)) continue;
          if (!Unit.LevelGrid.IsOccupied(testPosition)) continue;

          var targetUnit = Unit.LevelGrid.GetUnitAtPosition(testPosition);
          if (targetUnit.IsEnemy == Unit.IsEnemy) continue; // both Units are on the same 'team'

          yield return testPosition;
        }
      }
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
}