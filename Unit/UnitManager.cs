using Godot;
using TurnBasedStrategyCourse_godot.Level;

namespace TurnBasedStrategyCourse_godot.Unit
{
  public class UnitManager : Node
  {
    [Export] private NodePath levelGridNodePath;
    
    private Unit selectedUnit;
    private LevelGrid levelGrid;

    public override void _Ready()
    {
      levelGrid = GetNode<LevelGrid>(levelGridNodePath);
      
      foreach (Unit unit in GetTree().GetNodesInGroup("Units"))
      {
        unit.Connect(nameof(Unit.OnUnitSelected), this, nameof(OnUnitSelected));
        unit.Initialise(levelGrid);
      }
    }

    private void OnUnitSelected(Unit unit)
    {
      if (selectedUnit != null) selectedUnit.Selected = false;
      selectedUnit = unit;
      selectedUnit.Selected = true;
    }
    
    // ReSharper disable once UnusedMember.Local
    private void _on_Ground_input_event(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
    {
      if (!(@event is InputEventMouseButton eventMouseButton) || !eventMouseButton.Pressed) return;

      switch (eventMouseButton.ButtonIndex)
      {
        case 1:
          selectedUnit?.MoveTo(position);
          break;
        case 2:
          selectedUnit?.Spin();
          break;
      }
    }
  }
}