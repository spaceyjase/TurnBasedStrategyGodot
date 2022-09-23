using Godot;

namespace TurnBasedStrategyCourse_godot.Unit
{
  public class UnitRagdoll : Spatial
  {
    [Export] private float explosiveForce = 2f;
    
    private Skeleton skeleton;
    private MeshInstance mesh;

    public override void _Ready()
    {
      base._Ready();
      
      skeleton = GetNode<Skeleton>("Armature/Skeleton");
      mesh = GetNode<MeshInstance>("Armature/Skeleton/characterMedium001");
    }

    public void Init(Material material)
    {
      mesh.SetSurfaceMaterial(0, material);
    }

    public void StartRagdoll()
    {
      skeleton.PhysicalBonesStartSimulation();

      var direction = GlobalTransform.origin.DirectionTo(GlobalTransform.origin + Vector3.Forward + -Vector3.Up);
      foreach (var collider in GetNode<Skeleton>("Armature/Skeleton").GetChildren())
      {
        if (!(collider is PhysicalBone bone)) continue;

        bone.ApplyCentralImpulse(explosiveForce * -direction);
      }
      
      GetNode<RigidBody>("Gun").ApplyCentralImpulse(explosiveForce * -direction);
    }

    private void _on_DeathTimer_timeout()
    {
      QueueFree();
    }
  }
}
