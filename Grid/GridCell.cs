using Godot;

namespace TurnBasedStrategyCourse_godot.Grid
{
  public class GridCell : Spatial
  {
    private MeshInstance mesh;

    public override void _Ready()
    {
      base._Ready();
      mesh = GetNode<MeshInstance>("MeshInstance");
    }

    public void SetMaterial(SpatialMaterial newMaterial)
    {
      mesh.SetSurfaceMaterial(0, newMaterial);
    }
  }
}
