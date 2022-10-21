using System.Collections.Generic;
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
    [Signal]
    public delegate void UnitActionSelected(UnitAction action);

    [Export] private NodePath levelGridNodePath;

    private Unit selectedUnit;
    private UnitAction selectedAction;
    private LevelGrid levelGrid;
    private bool playerTurn = true;
    
    private readonly List<Unit> playerUnits = new List<Unit>();
    private readonly List<Unit> enemyUnits = new List<Unit>();

    public override void _Ready()
    {
      levelGrid = GetNode<LevelGrid>(levelGridNodePath);
      
      levelGrid.Connect(nameof(LevelGrid.GroundClicked), this, nameof(OnGroundClicked));

      foreach (Unit unit in GetTree().GetNodesInGroup("Units"))
      {
        unit.Connect(nameof(Unit.Selected), this, nameof(OnUnitSelected));
        unit.Connect(nameof(Unit.Dead), this, nameof(OnUnitDead));
        unit.Initialise(levelGrid);
        
        if (unit.IsEnemy)
        {
          enemyUnits.Add(unit);
        }
        else
        {
          playerUnits.Add(unit);
        }
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
        
        selectedUnit.IsSelected = false;
      }

      selectedUnit = unit;
      selectedUnit.IsSelected = true;

      EmitSignal(nameof(UnitSelected), selectedUnit);
      
      selectedAction = null;
    }
    
    private void OnUnitDead(Unit unit)
    {
      var list = unit.IsEnemy ? enemyUnits : playerUnits;
      list.Remove(unit);
      
      // TODO: end game conditions
    }

    private void OnGroundClicked(Node camera, InputEvent @event, Vector3 position, Vector3 normal, int shape_idx)
    {
      if (!(@event is InputEventMouseButton eventMouseButton) || !eventMouseButton.Pressed) return;
      if (!playerTurn) return;
      if (selectedUnit == null) return;
      if (selectedUnit.Busy) return;

      if (selectedAction == null) return;
      if (!selectedUnit.TrySetTargetPositionForAction(selectedAction, position)) return;
      
      selectedUnit.DoAction(selectedAction.ActionName);
    }

    // ReSharper disable once UnusedMember.Local
    private void _on_UI_ActionSelected(UnitAction action)
    {
      selectedAction = action;
      
      EmitSignal(nameof(UnitActionSelected), selectedAction);
    }
    
    private async void OnTurnChanged(int turn, bool isPlayerTurn)
    {
      playerTurn = isPlayerTurn;
      if (playerTurn) return;
      
      selectedAction = null;
      selectedUnit = null;
      
      if (enemyUnits.Count == 0 || playerUnits.Count == 0) return;
      
      foreach (var unit in enemyUnits)
      {
        await unit.TakeAiTurn();
      }
      
      EventBus.Instance.EmitSignal(nameof(EventBus.FinishedAiTurn));
    }
  }
}