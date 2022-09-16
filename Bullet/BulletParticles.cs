using Godot;

namespace TurnBasedStrategyCourse_godot.Bullet
{
  public class BulletParticles : CPUParticles
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
}