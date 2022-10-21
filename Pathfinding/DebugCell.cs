using Godot;

namespace TurnBasedStrategyCourse_godot.Pathfinding
{
  public class DebugCell : Spatial
  {
    private Label3D label;

    public override void _Ready()
    {
      base._Ready();

      label = GetNode<Label3D>(nameof(Label3D));
    }

    public void SetCost(int hCost, int gCost, int fCost)
    {
      label.Text =
        $"H: {(hCost == 0 ? "-" : hCost.ToString())}\n" +
        $"G: {(gCost == int.MaxValue ? "-" : gCost.ToString())}\n" +
        $"F: {(fCost == int.MaxValue ? "-" : fCost.ToString())}";
    }
  }
}