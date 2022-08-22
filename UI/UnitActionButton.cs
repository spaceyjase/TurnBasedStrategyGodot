using Godot;
using TurnBasedStrategyCourse_godot.Unit.Actions;

namespace TurnBasedStrategyCourse_godot.UI
{
  public class UnitActionButton : Control
  {
    [Signal]
    public delegate void ActionSelected(string actionName);
    
    private Label label;
    private UnitAction action;

    public override void _Ready()
    {
      label = GetNode<Label>("Button/Label");
    }

    private void _on_Button_pressed()
    {
      EmitSignal(nameof(ActionSelected), action);
    }

    public void SetAction(UnitAction unitAction)
    {
      label.Text = unitAction.ActionName;
      action = unitAction;
    }
  }
}