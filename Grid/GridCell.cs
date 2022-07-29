using Godot;

namespace TurnBasedStrategyCourse_godot.Grid
{
  public class GridCell : Spatial
  {
    private MeshInstance meshInstance;
    
    public override void _Ready()
    {
      base._Ready();
      
      meshInstance = GetNode<MeshInstance>(nameof(MeshInstance));
    }

    public void Hide()
    {
      meshInstance.Visible = false;
    }
    
    public void Show()
    {
      meshInstance.Visible = true;
    }
  }
}
