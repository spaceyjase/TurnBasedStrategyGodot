using Godot;

namespace TurnBasedStrategyCourse_godot.Bullet;

public class Bullet : Spatial
{
  [Export] private float movementSpeed = 200f;
  [Export] private PackedScene bulletParticlesScene;
    
  private Vector3 destination;
  private Vector3 startPosition;
  private const float baseMovementSpeed = 1f;
  private bool firstMove = true;

  public void Init(Vector3 position, Vector3 target)
  {
    destination = target;
    startPosition = position;
  }

  public override void _Ready()
  {
    base._Ready();

    GlobalTranslation = startPosition;
    Transform = Transform.LookingAt(destination, Vector3.Up);
    destination.y = GlobalTranslation.y;
  }

  public override void _Process(float delta)
  {
    base._Process(delta);

    var beforeMove = Translation.DistanceTo(destination);
      
    var moveDirection = Translation.DirectionTo(destination);
    GlobalTranslation += moveDirection * ((firstMove ? baseMovementSpeed : movementSpeed) * delta);
    firstMove = false; // hack for the trail to show (almost) immediately
      
    var afterMove = Translation.DistanceTo(destination);

    if (beforeMove > afterMove) return;
      
    GlobalTranslation = destination;
      
    var particles = bulletParticlesScene.Instance<CPUParticles>();
    GetTree().Root.AddChild(particles);
    particles.GlobalTranslation = destination;

    QueueFree();
  }
}