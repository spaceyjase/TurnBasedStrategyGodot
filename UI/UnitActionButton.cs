using Godot;

namespace TurnBasedStrategyCourse_godot.UI
{
  public class UnitActionButton : Control
  {
    [Signal]
    public delegate void ActionSelected(string actionName);
    
    private Label label;

    public override void _Ready()
    {
      label = GetNode<Label>("Button/Label");
    }

    public void SetText(string text)
    {
      label.Text = text;
    }

    private void _on_Button_pressed()
    {
      EmitSignal(nameof(ActionSelected), label.Text);
    }
  }
}