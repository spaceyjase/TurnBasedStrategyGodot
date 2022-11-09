using Godot;
using Godot.Collections;

namespace TurnBasedStrategyCourse_godot.Grenade;

public class GrenadeProjectile : Area
{
  [Signal] public delegate void Hit();
  
  [Export] private int damage = 30; // TODO: game resources
  [Export] private float moveSpeed = 15f;
  [Export] private float stoppingDistance = 0.2f;

  private Vector3 targetPosition;
  private CollisionShape collisionShape;

  public override void _Ready()
  {
    base._Ready();
    
    collisionShape = GetNode<CollisionShape>("CollisionShape");
  }

  public override void _Process(float delta)
  {
    base._Process(delta);

    GlobalTranslation = GlobalTranslation.MoveToward(targetPosition, moveSpeed * delta);
    if (!(GlobalTranslation.DistanceTo(targetPosition) < stoppingDistance)) return;
    
    var spaceState = GetWorld().DirectSpaceState;
    var query = new PhysicsShapeQueryParameters
    {
      Transform = GlobalTransform,
      CollideWithAreas = true,
      CollideWithBodies = false,
    };
    query.SetShape(collisionShape.Shape);
    var results = spaceState.IntersectShape(query);

    foreach (Dictionary result in results)
    {
      if (result["collider"] is not Spatial { Owner: Unit.Unit unit }) continue;
      
      // Note any unit here; players can damage themselves!
      unit.Damage(damage);
    }

    EmitSignal(nameof(Hit));

    QueueFree();
  }

  public void Init(Vector3 position)
  {
    targetPosition = position;
  }
}