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

    public void HideCell()
    {
      meshInstance.Visible = false;
    }
    
    public void ShowCell()
    {
      meshInstance.Visible = true;
    }
  }
}
