using System.Collections.Generic;
using System.Linq;
using Godot;
using TurnBasedStrategyCourse_godot.Events;
using TurnBasedStrategyCourse_godot.Grid;
using TurnBasedStrategyCourse_godot.Level;
using TurnBasedStrategyCourse_godot.Unit.Actions;
using TurnBasedStrategyCourse_godot.Unit.Stats;

namespace TurnBasedStrategyCourse_godot.Unit
{
  public class Unit : Spatial
  {
    [Signal]
    public delegate void UnitSelected(Unit selectedUnit);

    [Signal]
    public delegate void OnUnitMoving(Unit selectedUnit, GridPosition oldPosition, GridPosition newPosition);

    [Export] private UnitStats unitStats;
    [Export] private bool isEnemy;

    private AnimationTree animationTree;
    private AnimationNodeStateMachinePlayback animationStateMachine;
    private Spatial selectedVisual;
    private bool selected;

    private UnitAction CurrentAction { get; set; }
    public UnitAction IdleAction { get; private set; }
    private LevelGrid LevelGrid { get; set; }
    private GridPosition GridPosition { get; set; }

    public bool Selected
    {
      get => selected;
      set
      {
        selected = value;
        if (selected)
        {
          selectedVisual.Show();
        }
        else
        {
          selectedVisual.Hide();
        }
      }
    }

    public float MovementSpeed => unitStats.MovementSpeed;
    public double StoppingDistance => unitStats.StoppingDistance;
    public float RotateSpeed => unitStats.RotateSpeed;
    private int MaxMoveDistance => unitStats.MaxMoveDistance;
    private int TotalActionPoints => unitStats.TotalActionPoints;

    private readonly Dictionary<string, UnitAction> actions = new Dictionary<string, UnitAction>();

    public void SetAnimation(string animationName)
    {
      animationStateMachine.Travel(animationName);
    }

    public override void _Ready()
    {
      animationTree = GetNode<AnimationTree>("AnimationTree");
      animationStateMachine = animationTree.Get("parameters/playback") as AnimationNodeStateMachinePlayback;
      selectedVisual = GetNode<Spatial>("SelectedVisual");
      selectedVisual.Hide();

      if (isEnemy)
      {
        GetNode<Area>("SelectorArea").QueueFree();
      }

      foreach (var child in GetChildren())
      {
        if (!(child is UnitAction action)) continue;

        if (actions.Count == 0)
        {
          IdleAction = action;
        }

        actions[action.Name] = action;
      }
      
      CurrentAction = IdleAction;
      TargetPosition = Translation;
      
      EventBus.Instance.Connect(nameof(EventBus.TurnChanged), this, nameof(OnTurnChanged));
    }

    private void OnTurnChanged(int turn, bool isPlayerTurn)
    {
      if (!isEnemy) GetNode<CollisionShape>("SelectorArea/CollisionShape").Disabled = !isPlayerTurn;
      if (isEnemy == isPlayerTurn) return;

      ResetActionPoints();
    }

    public void Initialise(LevelGrid levelGrid)
    {
      LevelGrid = levelGrid;
      GridPosition = LevelGrid.GetGridPosition(Translation);
      
      unitStats.Initialise();

      ActionPoints = TotalActionPoints;
    }

    public override void _Process(float delta)
    {
      base._Process(delta);
      
      ProcessAction(delta);
    }

    private void ProcessAction(float delta)
    {
      CurrentAction.Execute(delta);

      var newGridPosition = LevelGrid.GetGridPosition(Translation);
      if (newGridPosition == GridPosition) return;

      var oldPosition = GridPosition;
      GridPosition = newGridPosition;
      EmitSignal(nameof(OnUnitMoving), this, oldPosition, GridPosition);
    }

    // ReSharper disable once UnusedMember.Local
    private void _on_SelectorArea_input_event(Node camera, InputEvent @event, Vector3 position, Vector3 normal,
      int shape_idx)
    {
      if (!(@event is InputEventMouseButton eventMouseButton) || !eventMouseButton.Pressed ||
          eventMouseButton.ButtonIndex != 1) return;
      
      EmitSignal(nameof(UnitSelected), this);
    }
    
    public IEnumerable<GridPosition> ValidPositions => GetValidGridPosition();
    private Vector3 targetPosition = Vector3.Zero;

    public Vector3 TargetPosition
    { 
      get => targetPosition;
      set
      {
        var gridPosition = LevelGrid.GetGridPosition(value);
        if (!IsValidGridPosition(gridPosition)) return;

        targetPosition = LevelGrid.GetWorldPosition(gridPosition);
      }
    }
    
    public IEnumerable<UnitAction> Actions => actions.Values.Where(x => x != IdleAction);
    public int ActionPoints { get; private set; }
    public bool Busy => CurrentAction != IdleAction;

    private void TryChangeAction(string name)
    {
      if (actions.ContainsKey(name) == false)
      {
        GD.PrintErr($"State {name} does not exist!");
        return;
      }

      var action = actions[name];
      if (!CanTakeAction(action)) return;

      SpendActionPoints(action.ActionPointCost);
      
      ChangeAction(actions[name]);
    }

    public void ChangeAction(string name)
    {
      if (actions.ContainsKey(name) == false)
      {
        GD.PrintErr($"State {name} does not exist!");
        return;
      }
      
      ChangeAction(actions[name]);
    }

    private void ChangeAction(UnitAction newAction)
    {
      if (newAction == null)
      {
        GD.PrintErr("Cannot transition to a null state!");
        return;
      }

      CurrentAction?.OnExit?.Invoke();

      CurrentAction = newAction;
      
      EventBus.Instance.EmitSignal(CurrentAction == IdleAction ? nameof(EventBus.UnitIdle) : nameof(EventBus.UnitBusy), this);
      
      newAction.OnEnter?.Invoke();
    }
    
    private bool IsValidGridPosition(GridPosition position)
    {
      return GetValidGridPosition().Contains(position);
    }

    private IEnumerable<GridPosition> GetValidGridPosition()
    {
      for (var x = -MaxMoveDistance; x <= MaxMoveDistance; ++x)
      {
        for (var z = -MaxMoveDistance; z <= MaxMoveDistance; ++z)
        {
          var offset = new GridPosition(x, z);
          var testPosition = GridPosition + offset;

          if (!LevelGrid.IsValidPosition(testPosition)) continue;
          if (GridPosition == testPosition) continue;
          if (LevelGrid.IsOccupied(testPosition)) continue;

          yield return testPosition;
        }
      }
    }

    public void DoAction(string actionName)
    {
      if (CurrentAction.Name != IdleAction.ActionName) return;
      
      TryChangeAction(actionName);
    }

    private bool CanTakeAction(UnitAction action)
    {
      return ActionPoints >= action.ActionPointCost;
    }

    private void SpendActionPoints(int points)
    {
      ActionPoints -= points;
    }

    private void ResetActionPoints()
    {
      ActionPoints = TotalActionPoints;
    }
  }
}