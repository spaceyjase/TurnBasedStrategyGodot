using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Grenade;
using TurnBasedStrategyCourse_godot.Grid;

namespace TurnBasedStrategyCourse_godot.Unit.Actions;

public class GrenadeAction : UnitAction
{
  [Export] private PackedScene grenadeProjectileScene;
  [Export] private float shakeIntensity = 1f;

  private enum State
  {
    Idle,
    Aiming,
    Throwing
  }

  public override void _Ready()
  {
    base._Ready();

    OnEnter += () => { state = State.Aiming; };
  }

  private State state;

  public override void Execute(float delta)
  {
    switch (state)
    {
      case State.Idle:
        break;
      case State.Throwing:
        break;
      case State.Aiming:
        var projectile = grenadeProjectileScene.Instance<GrenadeProjectile>();
        projectile.Init(Unit.TargetPosition);
        projectile.Connect(nameof(GrenadeProjectile.Hit), this, nameof(OnGrenadeHit));
        AddChild(projectile);
        state = State.Throwing;
        break;
      default:
        throw new ArgumentOutOfRangeException();
    }
  }

  private void OnGrenadeHit()
  {
    state = State.Idle;
    EventBus.Instance.EmitSignal(nameof(EventBus.CameraShake), shakeIntensity);
    Unit.ChangeAction(Unit.DefaultAction.ActionName);
  }

  private IEnumerable<GridPosition> GetAllGridPositions()
  {
    for (var x = -Unit.MaxThrowDistance; x <= Unit.MaxThrowDistance; ++x)
    {
      for (var z = -Unit.MaxThrowDistance; z <= Unit.MaxThrowDistance; ++z)
      {
        var offset = new GridPosition(x, z);

        var testDistance = Mathf.Abs(x) + Mathf.Abs(z);
        if (testDistance > Unit.MaxShootDistance) continue;

        yield return Unit.GridPosition + offset;
      }
    }
  }

  protected override IEnumerable<GridPosition> GetValidActionGridPositions()
  {
    return from testPosition in GetAllGridPositions()
      where Unit.LevelManager.IsValidPosition(testPosition)
      select testPosition;
  }
}