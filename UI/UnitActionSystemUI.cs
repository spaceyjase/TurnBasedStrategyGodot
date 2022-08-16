using System.Linq;
using Godot;

namespace TurnBasedStrategyCourse_godot.UI
{
  public class UnitActionSystemUI : Node
  {
    [Signal]
    private delegate void ActionSelected(string actionName);

    [Export] private PackedScene unitActionButtonScene = null;

    private GridContainer gridContainer;

    public override void _Ready()
    {
      base._Ready();

      gridContainer = GetNode<GridContainer>(nameof(GridContainer));
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

        button.SetText(action.ActionName);
        button.Connect(nameof(UnitActionButton.ActionSelected), this, nameof(_on_UnitActionButton_ActionSelected));
      }

      gridContainer.Columns = unit.Actions.Count();
    }

    private void _on_UnitActionButton_ActionSelected(string actionName)
    {
      EmitSignal(nameof(ActionSelected), actionName);
    }
  }
}