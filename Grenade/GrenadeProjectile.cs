using Godot;
using Godot.Collections;

namespace TurnBasedStrategyCourse_godot.Grenade;

public class GrenadeProjectile : Area
{
  [Signal] public delegate void Hit(Vector3 position);
  
  [Export] private int damage = 30; // TODO: game resources
  [Export] private float moveSpeed = 15f;
  [Export] private float stoppingDistance = 0.2f;
  [Export] private Curve curve;

  private Vector3 startPosition;
  private Vector3 endPosition;
  private CollisionShape collisionShape;
  private float totalDistance;

  public override void _Ready()
  {
    base._Ready();

    GlobalTranslation = startPosition;
    totalDistance = GlobalTranslation.DistanceTo(endPosition);
    collisionShape = GetNode<CollisionShape>("CollisionShape");
  }

  public override void _Process(float delta)
  {
    base._Process(delta);

    var translation = GlobalTranslation;
    translation.y = 0f;
    var distance = translation.DistanceTo(endPosition);
    var distanceNormalized = 1 - distance / totalDistance;
    
    translation = GlobalTranslation.MoveToward(endPosition, moveSpeed * delta);
    translation.y = curve.Interpolate(distanceNormalized);
    GlobalTranslation = translation;
    
    if (distance > stoppingDistance) return;
    
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

    EmitSignal(nameof(Hit), GlobalTranslation);

    QueueFree();
  }

  public void Init(Vector3 unitPosition, Vector3 targetPosition)
  {
    startPosition = unitPosition;
    endPosition = targetPosition;
  }
}