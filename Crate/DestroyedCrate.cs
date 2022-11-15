using System;
using Godot;

namespace TurnBasedStrategyCourse_godot.Crate;

public class DestroyedCrate : Spatial
{
  [Export] private float explosiveForce = 8f;
  [Export] private float timeToLive = 3f;

  public override void _Ready()
  {
    base._Ready();
    
    GetTree().CreateTimer(timeToLive).Connect("timeout", this, nameof(OnTimeout));
    
    foreach (var child in GetChildren())
    {
      if (child is not RigidBody body) continue;
      
      body.ApplyImpulse(new Vector3(GD.Randf(), GD.Randf(), GD.Randf()), Vector3.Up * explosiveForce);
    }
  }
  
  private void OnTimeout()
  {
    QueueFree();
  }
}