using Godot;

namespace TurnBasedStrategyCourse_godot.Unit
{
  public class UnitManager : Node
  {
    private Unit selectedUnit;

    public override void _Ready()
    {
      foreach (Unit unit in GetTree().GetNodesInGroup("Units"))
      {
        unit.Connect(nameof(Unit.OnUnitSelected), this, nameof(OnUnitSelected));
      }
    }

    private void OnUnitSelected(Unit unit)
    {
      this.selectedUnit?.Deselect();
      this.selectedUnit = unit;
      this.selectedUnit.Select();
    }
    
    // ReSharper disable once UnusedMember.Local
    private void _on_Ground_input_event(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
    {
      if (!(@event is InputEventMouseButton eventMouseButton) || !eventMouseButton.Pressed ||
          eventMouseButton.ButtonIndex != 1) return;

      selectedUnit?.SetMovementDirection(position);
    }
  }
}