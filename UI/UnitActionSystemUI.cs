using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Unit.Actions;

namespace TurnBasedStrategyCourse_godot.UI
{
  public class UnitActionSystemUI : Node
  {
    [Signal]
    private delegate void ActionSelected(string actionName);

    [Export] private PackedScene unitActionButtonScene;

    private GridContainer gridContainer;
    private Label busyLabel;

    public override void _Ready()
    {
      base._Ready();

      gridContainer = GetNode<GridContainer>(nameof(GridContainer));
      
      EventBus.Instance.Connect(nameof(EventBus.UnitBusy), this, nameof(OnUnitBusy));
      EventBus.Instance.Connect(nameof(EventBus.UnitIdle), this, nameof(OnUnitIdle));

      busyLabel = GetNode<Label>("BusyLabel");
      busyLabel.Visible = false;
    }

    private void OnUnitBusy(Unit.Unit unit)
    {
      busyLabel.Text = $"{unit.Name} busy";
      busyLabel.Visible = true;
    }
    
    private void OnUnitIdle(Unit.Unit unit)
    {
      busyLabel.Text = $"{unit.Name} idle";
      busyLabel.Visible = false;
    }

    private void _on_UnitManager_UnitSelected(Unit.Unit unit)
    {
      // Remove existing (could pool these given the same action)
      foreach (Node child in gridContainer.GetChildren())
      {
        gridContainer.RemoveChild(child);
        child.QueueFree();
      }

      foreach (var action in unit.Actions)
      {
        var button = (UnitActionButton)unitActionButtonScene.Instance();
        gridContainer.AddChild(button);

        button.SetAction(action);
        button.Connect(nameof(UnitActionButton.ActionSelected), this, nameof(_on_UnitActionButton_ActionSelected));
      }

      gridContainer.Columns = unit.Actions.Count();
    }

    private void _on_UnitActionButton_ActionSelected(UnitAction action)
    {
      EmitSignal(nameof(ActionSelected), action);
    }
  }
}