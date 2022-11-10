using Godot;

namespace TurnBasedStrategyCourse_godot.Utils;

public class LifetimeParticlesHandler : CPUParticles
{
  private const float lifetimeExtension = 0.1f;

  public override void _Ready()
  {
    GetTree().CreateTimer(Lifetime + lifetimeExtension).Connect("timeout", this, nameof(OnTimerTimeout));
    Emitting = true;
  }

  private void OnTimerTimeout()
  {
    QueueFree();
  }
}