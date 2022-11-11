using Godot;
using TurnBasedStrategyCourse_godot.Destroyable;
using TurnBasedStrategyCourse_godot.Events;

namespace TurnBasedStrategyCourse_godot.Crate;

public class Crate : Spatial, IDestroyable
{
  public void Damage(int damage)
  {
    EventBus.Instance.EmitSignal(nameof(EventBus.ObstacleDestroyed), this);
    QueueFree();
  }
}