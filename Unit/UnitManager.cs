using Godot;
using TurnBasedStrategyCourse_godot.Events;
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
    private UnitAction selectedAction;
    private LevelGrid levelGrid;
    private bool playerTurn = true;

    public override void _Ready()
    {
      levelGrid = GetNode<LevelGrid>(levelGridNodePath);

      foreach (Unit unit in GetTree().GetNodesInGroup("Units"))
      {
        unit.Connect(nameof(Unit.UnitSelected), this, nameof(OnUnitSelected));
        unit.Initialise(levelGrid);
      }
      
      EventBus.Instance.Connect(nameof(EventBus.TurnChanged), this, nameof(OnTurnChanged));
    }

    private void OnUnitSelected(Unit unit)
    {
      if (!playerTurn) return;
      if (selectedUnit == unit) return;
      if (selectedUnit != null)
      {
        if (selectedUnit.Busy) return;
        
        selectedUnit.Selected = false;
      }

      selectedUnit = unit;
      selectedUnit.Selected = true;

      if (selectedUnit != null) EmitSignal(nameof(UnitSelected), selectedUnit);
      
      selectedAction = null;
    }

    // ReSharper disable once UnusedMember.Local
    private void _on_Ground_input_event(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
    {
      if (!(@event is InputEventMouseButton eventMouseButton) || !eventMouseButton.Pressed) return;
      if (!playerTurn) return;
      if (selectedUnit == null) return;
      if (selectedUnit.Busy) return;
      
      selectedUnit.TargetPosition = position;

      if (selectedAction == null) return;
      
      selectedUnit.DoAction(selectedAction.ActionName);
    }

    // ReSharper disable once UnusedMember.Local
    private void _on_UI_ActionSelected(UnitAction action)
    {
      selectedAction = action;
    }
    
    private void OnTurnChanged(int turn, bool isPlayerTurn)
    {
      playerTurn = isPlayerTurn;
      if (playerTurn) return;
      
      selectedAction = null;
      selectedUnit = null;
    }
  }
}