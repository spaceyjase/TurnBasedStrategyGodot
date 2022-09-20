using System;
using System.Collections.Generic;
using Godot;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions
{
  public class ShootAction : UnitAction
  {
    [Export] private PackedScene bulletScene;
    [Export] private int damage;
    
    private readonly float aimTime = 1f;
    private readonly float shootTime = 0.5f;
    private readonly float coolOffTime = 0.1f;
    
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
        target = unit.LevelGrid.GetUnitAtPosition(
          unit.LevelGrid.GetGridPosition(unit.TargetPosition));
        canShoot = true;
      };
    }

    public override void Execute(float delta)
    {
      Shoot(delta);
    }

    protected override IEnumerable<GridPosition> GetValidActionGridPositions()
    {
      for (var x = -unit.MaxShootDistance; x <= unit.MaxShootDistance; ++x)
      {
        for (var z = -unit.MaxShootDistance; z <= unit.MaxShootDistance; ++z)
        {
          var offset = new GridPosition(x, z);
          var testPosition = unit.GridPosition + offset;

          var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
          if (testDistance > unit.MaxShootDistance) continue;

          if (!unit.LevelGrid.IsValidPosition(testPosition)) continue;
          if (!unit.LevelGrid.IsOccupied(testPosition)) continue;

          var targetUnit = unit.LevelGrid.GetUnitAtPosition(testPosition);
          if (targetUnit.IsEnemy == unit.IsEnemy) continue; // both units are on the same 'team'

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
          var aimDirection = unit.Translation.DirectionTo(unit.TargetPosition);
          var newTransform = unit.Transform.LookingAt(unit.GlobalTransform.origin - aimDirection, Vector3.Up);
          unit.Transform = unit.Transform.InterpolateWith(newTransform, unit.RotateSpeed * delta);
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

      bullet.Init(unit.BulletSpawnPosition, target.GlobalTranslation);
      
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
          unit.SetAnimation(UnitAnimations.Shooting); 
          state = State.CoolOff;
          timer = coolOffTime;
          break;
        case State.CoolOff:
          unit.ChangeAction(unit.DefaultAction.ActionName);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}