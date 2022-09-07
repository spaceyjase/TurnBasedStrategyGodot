using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Unit.Actions;
using Timer = System.Timers.Timer;

namespace TurnBasedStrategyCourse_godot.UI
{
  public class UnitActionSystemUI : Node
  {
    [Signal]
    private delegate void ActionSelected(string actionName);
    [Signal]
    private delegate void EndTurnPressed(string actionName);

    [Export] private PackedScene unitActionButtonScene;

    private GridContainer gridContainer;
    private Label busyLabel;
    private Label actionPointLabel;
    private Label turnLabel;
    private Control playerControls;
    private Control enemyControls;

    private Unit.Unit currentUnit;
    private Button endTurnButton;
    
    private Timer enemyTimer = new Timer(2000);

    private Unit.Unit CurrentUnit
    { 
      get => currentUnit;
      set
      {
        currentUnit = value;
        ClearActionButtons();
        
        if (currentUnit == null) return;

        UpdateActionButtons(currentUnit);
        UpdateActionPoints(currentUnit);
      }
    }

    private void UpdateActionButtons(Unit.Unit unit)
    {
      foreach (var action in unit.Actions)
      {
        var button = (UnitActionButton)unitActionButtonScene.Instance();
        gridContainer.AddChild(button);

        button.SetAction(action);
        button.Connect(nameof(UnitActionButton.ActionSelected), this, nameof(_on_UnitActionButton_ActionSelected));
      }

      gridContainer.Columns = unit.Actions.Count();
    }

    private void ClearActionButtons()
    {
      // Remove existing (could pool these given the same action)
      foreach (Node child in gridContainer.GetChildren())
      {
        gridContainer.RemoveChild(child);
        child.QueueFree();
      }
    }

    public override void _Ready()
    {
      base._Ready();

      
      EventBus.Instance.Connect(nameof(EventBus.UnitBusy), this, nameof(OnUnitBusy));
      EventBus.Instance.Connect(nameof(EventBus.UnitIdle), this, nameof(OnUnitIdle));
      EventBus.Instance.Connect(nameof(EventBus.TurnChanged), this, nameof(OnTurnChanged));

      playerControls = GetNode<Control>("PlayerControls");
      gridContainer = GetNode<GridContainer>("PlayerControls/GridContainer");
      
      enemyControls = GetNode<Control>("EnemyControls");

      busyLabel = GetNode<Label>("PlayerControls/BusyLabel");
      busyLabel.Visible = false;
      
      actionPointLabel = GetNode<Label>("PlayerControls/ActionPointLabel");
      actionPointLabel.Visible = false;

      turnLabel = GetNode<Label>("TurnLabel");

      endTurnButton = GetNode<Button>("EndTurnButton");
      
      enemyTimer.Elapsed += (sender, args) =>
      {
        enemyTimer.Stop();
        _on_EndTurnButton_pressed();
      };
    }

    private void OnUnitBusy(Unit.Unit unit)
    {
      UpdateActionPoints(unit);
      busyLabel.Text = $"{unit.Name} busy";
      busyLabel.Visible = true;
      endTurnButton.Disabled = true;
    }

    private void UpdateActionPoints(Unit.Unit unit)
    {
      actionPointLabel.Text = $"Action Points: {unit.ActionPoints}";
      actionPointLabel.Visible = true;
    }

    private void OnUnitIdle(Unit.Unit unit)
    {
      UpdateActionPoints(unit);
      busyLabel.Text = $"{unit.Name} idle";
      busyLabel.Visible = false;
      endTurnButton.Disabled = false;
    }

    // ReSharper disable once UnusedMember.Local
    private void _on_UnitManager_UnitSelected(Unit.Unit unit)
    {
      CurrentUnit = unit;
    }

    private void _on_UnitActionButton_ActionSelected(UnitAction action)
    {
      EmitSignal(nameof(ActionSelected), action);
    }

    private void _on_EndTurnButton_pressed()
    {
      EmitSignal(nameof(EndTurnPressed));
    }
    
    private void OnTurnChanged(int turn, bool isPlayerTurn)
    {
      UpdateControls(isPlayerTurn);
      if (isPlayerTurn)
      {
        if (CurrentUnit != null)
        {
          UpdateActionPoints(CurrentUnit);
        }
      }
      else if (CurrentUnit != null)
      {
        CurrentUnit.Selected = false;
        CurrentUnit = null;
      }

      turnLabel.Text = $"Turn: {turn}";
    }

    private void UpdateControls(bool isPlayerTurn)
    {
      endTurnButton.Visible = isPlayerTurn;
      playerControls.Visible = isPlayerTurn;
      enemyControls.Visible = !isPlayerTurn;
      actionPointLabel.Visible = false;

      // TODO: Dummy AI
      if (isPlayerTurn) return;
      
      enemyTimer.Start();
    }
  }
}