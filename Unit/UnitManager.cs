using Godot;
using TurnBasedStrategyCourse_godot.Level;
using TurnBasedStrategyCourse_godot.Unit.Actions;

namespace TurnBasedStrategyCourse_godot.Unit
{
  public class UnitManager : Node
  {
    [Signal]
    public delegate void UnitSelected(Unit unit);

    [Export] private NodePath levelGridNodePath;

    private Unit selectedUnit;
    private LevelGrid levelGrid;
    private UnitAction selectedAction;

    public override void _Ready()
    {
      levelGrid = GetNode<LevelGrid>(levelGridNodePath);

      foreach (Unit unit in GetTree().GetNodesInGroup("Units"))
      {
        unit.Connect(nameof(Unit.UnitSelected), this, nameof(OnUnitSelected));
        unit.Initialise(levelGrid);
      }
    }

    private void OnUnitSelected(Unit unit)
    {
      if (selectedUnit == unit) return;
      if (selectedUnit != null) selectedUnit.Selected = false;
      selectedUnit = unit;
      selectedUnit.Selected = true;

      if (selectedUnit != null) EmitSignal(nameof(UnitSelected), selectedUnit);
    }

    // ReSharper disable once UnusedMember.Local
    private void _on_Ground_input_event(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
    {
      if (!(@event is InputEventMouseButton eventMouseButton) || !eventMouseButton.Pressed) return;
      if (selectedUnit == null) return;
      
      selectedUnit.TargetPosition = position;

      if (selectedAction == null) return;
      
      selectedUnit.DoAction(selectedAction.ActionName);
    }

    private void _on_UI_ActionSelected(UnitAction action)
    {
      selectedAction = action;
    }
  }
}