using Godot;
using TurnBasedStrategyCourse_godot.Destroyable;
using TurnBasedStrategyCourse_godot.Events;

namespace TurnBasedStrategyCourse_godot.Crate;

public class Crate : Spatial, IDestroyable
{
  [Export] private PackedScene crateDestroyedScene;
  
  public void Damage(int damage)
  {
    var crateDestroyed = crateDestroyedScene.Instance<Spatial>();
    GetTree().Root.AddChild(crateDestroyed);
    crateDestroyed.GlobalTranslation = GlobalTranslation;
    
    EventBus.Instance.EmitSignal(nameof(EventBus.NavigationChange));
    QueueFree();
  }
}