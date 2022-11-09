using Godot;

namespace TurnBasedStrategyCourse_godot.Unit.UI;

public class UnitWorldUI : Spatial
{
  private Label label;
  private ProgressBar healthProgressBar;

  public override void _Ready()
  {
    label = GetNode<Label>("Viewport/PanelContainer/VBoxContainer/Label");
    healthProgressBar = GetNode<ProgressBar>("Viewport/PanelContainer/VBoxContainer/HealthProgressBar");
  }

  public void UpdateUI(Unit unit)
  {
    label.Text = $"AP: {unit.ActionPoints}/{unit.TotalActionPoints}";
    healthProgressBar.MaxValue = unit.MaxHealth;
    healthProgressBar.Value = unit.CurrentHealth;
  }
}