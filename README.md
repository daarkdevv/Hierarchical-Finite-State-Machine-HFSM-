# Hierarchical Finite State Machine (HFSM)

A custom, lightweight Hierarchical Finite State Machine implementation in Unity with support for asynchronous state transitions, activities (enter/exit animations), and event-driven input handling.

## Overview

This HFSM system is designed for complex character behavior management, combining the simplicity of FSMs with the organizational power of hierarchical state nesting. It powers player movement states (Idle, Walking, Running, Crouching, Airborne) and integrates seamlessly with Unity's input system and character controller.

### Key Features

- **Hierarchical States** — Nest states within parent states for cleaner organization (e.g., `GroundedState` contains `IdleState`, `WalkingState`, `RunningState`)
- **Async Transitions** — Smooth state changes with asynchronous activities (camera height transitions, animations)
- **Activity System** — Attach enter/exit activities to states for side effects and animations
- **Event-Driven Input** — Decouple input handling from state logic via subscriptions
- **Type-Safe Input** — Use an enum (`InputType`) instead of strings for input events
- **Automatic Traversal** — Parent states delegate to active children; transitions bubble up correctly
- **Clean Architecture** — Single-class-per-file, each state in its own file for easy maintenance

## Architecture

### Core Components

#### `State` (base class)
Abstract base for all states. Provides lifecycle hooks and transition logic.

```csharp
public abstract class State
{
    public StateMachine machine;
    public State parent;
    public State ActiveChild;
    public PlayerContext ctx;
    public List<IActivity> EnterActivities { get; set; }
    public List<IActivity> ExitActivities { get; set; }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnUpdate(float deltaTime) { }
    public virtual State GetTransition() => null;
    public virtual State InitialState() => null;
    
    public void Enter() { /* enter this state and initial child */ }
    public void Exit() { /* exit and cleanup */ }
    public void Update(float deltaTime) { /* descend into children, then evaluate transitions */ }
}
```

**Key methods:**
- `OnEnter()` — called when state becomes active
- `OnExit()` — called when leaving the state
- `GetTransition()` — return the next state (or null to stay)
- `InitialState()` — return the default child state (for parent states)
- `AddEnterActivity()` / `AddExitActivity()` — attach async side effects

#### `StateMachine` (orchestrator)
Manages the root state, ticking, and async state changes.

```csharp
public class StateMachine
{
    public State root;
    public void Start();
    public void Tick(float deltaTime);
    public async Task ChangeState(State from, State to);
}
```

**Key methods:**
- `Start()` — enter the root and initial hierarchy
- `Tick(deltaTime)` — update the current state
- `ChangeState(from, to)` — async transition with exit/enter activities

#### `PlayerContext` (shared data)
Runtime state shared across all states. Holds components, speeds, and input manager.

```csharp
public class PlayerContext
{
    public CharacterController characterController;
    public Transform cameraTransform;
    public Transform Player;
    public float currentMovementSpeed;
    public float walkSpeed, sprintSpeed, airborneSpeed, crouchSpeed;
    public float crouchHeight, standingHeight;
    public bool IsGrounded;
    public StateInputManager stateInputManager;
}
```

#### `StateInputManager` (input aggregator)
Subscribes to input events and exposes current input state to states.

```csharp
public class StateInputManager
{
    public Vector2 moveInput;
    public bool sprintPressed;
    public bool crouchPressed;
    public void Initialize();
}
```

### State Hierarchy

```
PlayerRootState
├── GroundedState
│   ├── IdleState
│   ├── WalkingState
│   ├── RunningState
│   └── CrouchState
└── AirBorneState
```

**How it works:**
- `PlayerRootState` is the root; it always updates and applies movement
- `GroundedState` is active when grounded; manages walking/idle/crouch
- `IdleState`, `WalkingState`, etc. are leaf states; handle speed and transition logic
- When grounded == false, `AirBorneState` takes over

### Input System Integration

Input flows through a chain:

1. **Unity Input System** → InputAction callbacks
2. **InputEventsSO** (ScriptableObject) → C# events
3. **InputSubscriber** → subscribes by `InputType` enum and exposes handler methods
4. **StateInputManager** → subscribes and stores current input state
5. **States** → poll `ctx.stateInputManager` in `GetTransition()` and `OnUpdate()`

Example:
```csharp
// In StateInputManager.Initialize():
InputSubscriber.Instance.SubscribeToVector2InputEvent(
    InputType.Move, 
    (v) => moveInput = v
);

// In IdleState.GetTransition():
float mag = ctx.stateInputManager.moveInput.magnitude;
if (mag > 0.01f) return parentGroundedState.walkingState;
```

### Activity System

Activities are async side effects that run during state enter/exit. Example: camera height transition during crouch.

```csharp
public abstract class IActivity
{
    public virtual async Task PerformActivity() { }
}

public class CrouchActiveActivity : IActivity
{
    public override async Task PerformActivity()
    {
        if (isCrouchingDown)
            await CrouchDownAsync(); // move camera down
        else
            await StandUpAsync();     // move camera up
    }
}
```

**Usage in states:**
```csharp
public static CrouchState Create(StateMachine m, State parent, PlayerContext ctx)
{
    var s = new CrouchState(m, parent, ctx);
    s.AddEnterActivity(new CrouchActiveActivity(ctx, true));  // enter = crouch down
    s.AddExitActivity(new CrouchActiveActivity(ctx, false));  // exit = stand up
    return s;
}
```

During `StateMachine.ChangeState()`, all exit activities from exiting states run in sequence, then states transition, then enter activities run.

## Runtime Flow

### Frame-by-frame execution

1. **Update()** calls `machine.Tick(deltaTime)`
2. `StateMachine.Tick()` calls `root.Update(deltaTime)`
3. `State.Update()` descends into `ActiveChild.Update()` if it exists
4. Once at a leaf, `GetTransition()` is evaluated
5. If a transition is returned, `StateMachine.ChangeState()` is called asynchronously
6. `ChangeState()`:
   - Runs exit activities from exiting states
   - Calls Exit() on exiting states
   - Runs enter activities on entering states
   - Calls Enter() on entering states
7. After transition completes, normal update resumes

### Example: Idle → Walking

```
Frame N:
- IdleState.Update() → GetTransition()
- moveInput.magnitude > 0.01 → returns WalkingState
- ChangeState(IdleState, WalkingState) queued

Frame N+1:
- IdleState.Exit() called
- WalkingState.Enter() called → sets currentMovementSpeed = walkSpeed
- PlayerRootState applies CharacterController.Move(forward * walkSpeed * dt)
```

## Usage

### Creating a Custom State

```csharp
using UnityEngine;

public class CustomState : State
{
    public CustomState(StateMachine m, State parent, PlayerContext ctx)
        : base(m, parent, ctx) { }

    public override void OnEnter()
    {
        Debug.Log("Entered CustomState");
        ctx.currentMovementSpeed = 2f;
    }

    public override void OnExit()
    {
        Debug.Log("Exiting CustomState");
    }

    public override State GetTransition()
    {
        // Check input or state conditions
        if (some_condition)
            return ((ParentState)parent).otherChildState;
        
        return null; // stay in this state
    }
}
```

### Creating a Custom Activity

```csharp
using System.Threading.Tasks;
using UnityEngine;

public class FadeOutActivity : IActivity
{
    private CanvasGroup canvasGroup;
    private float duration;

    public FadeOutActivity(CanvasGroup cg, float dur)
    {
        canvasGroup = cg;
        duration = dur;
    }

    public override async Task PerformActivity()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            await Awaitable.EndOfFrameAsync();
        }
        canvasGroup.alpha = 0f;
    }
}
```

### Setting Up in PlayerStateDriver

```csharp
void Start()
{
    characterController = GetComponent<CharacterController>();
    cameraTransform = Camera.main.transform;
    playerTransform = transform;
    checkIsGrounded = GetComponent<CheckIsGrounded>();

    // Create context
    ctx = new PlayerContext(this, checkIsGrounded);
    
    // Create state machine
    State root = new PlayerRootState(null, null, ctx);
    machine = new StateMachine(ctx, root);
    ((PlayerRootState)root).SetStateMachine(machine);
    machine.Start();
}

void Update()
{
    machine.Tick(Time.deltaTime);
}
```

## Project Structure

```
Assets/Scripts/HirearchicalFiniteStateMachine/
├── Activities/
│   ├── IActivity.cs              (base interface)
│   ├── DelayActivity.cs          (simple delay)
│   ├── CrouchActiveActivity.cs   (camera height transition)
│   └── Sequential.cs             (run multiple activities)
├── States/
│   ├── State.cs                  (abstract base)
│   ├── PlayerRootState.cs        (root; applies movement)
│   ├── GroundedState.cs          (parent state for grounded behavior)
│   ├── IdleState.cs              (leaf; stationary)
│   ├── WalkingState.cs           (leaf; normal movement)
│   ├── RunningState.cs           (leaf; sprint movement)
│   ├── CrouchState.cs            (leaf; crouch with activity)
│   └── AirBorneState.cs          (leaf; falling/jumping)
├── StateMachine/
│   ├── StateMachine.cs           (main orchestrator)
│   └── PlayerStateDriver.cs      (MonoBehaviour; entry point)
├── StateInput/
│   └── StateInputManager.cs      (input aggregator)
└── PlayerProperties/
    ├── PlayerContext.cs          (shared runtime data)
    └── PlayerConfig.cs           (serialized config)
```

## Performance Considerations

- **No GC allocations in steady state** — input polling and state updates create no garbage
- **Activities are async** — they don't block frame logic; transitions happen smoothly
- **Hierarchical traversal is O(depth)** — typically 3–4 levels deep
- **Input subscription is one-time** — no per-frame dictionary lookups

## Extending the System

### Add a New State

1. Create a new file (e.g., `JumpState.cs`)
2. Inherit from `State`
3. Implement `OnEnter()`, `OnExit()`, `GetTransition()`
4. Add as a child in parent state's constructor

### Add a New Input Type

1. Add to `InputType` enum:
   ```csharp
   public enum InputType { Move, Sprint, Crouch, Jump, Interact, NewAction }
   ```
2. Register in `InputSubscriber.InitializeDictionaries()`:
   ```csharp
   { InputType.NewAction, (h => InputSo.OnNewActionEvent += h, ...) }
   ```
3. Subscribe in `StateInputManager.Initialize()`
4. Use in states via `ctx.stateInputManager`

### Add a New Activity

1. Implement `IActivity.PerformActivity()` with async/await
2. Attach to states via `state.AddEnterActivity()` or `AddExitActivity()`
3. `StateMachine.ChangeState()` automatically awaits all activities

## Common Patterns

### Conditional Transitions

```csharp
public override State GetTransition()
{
    if (ctx.IsGrounded && ctx.stateInputManager.jumpPressed)
        return jumpState;
    
    if (!ctx.IsGrounded)
        return airborneState;
    
    return null;
}
```

### Speed Modulation

Set speed in `OnEnter()` and apply in `PlayerRootState.OnUpdate()`:

```csharp
// In IdleState
public override void OnEnter() => ctx.currentMovementSpeed = 0f;

// In PlayerRootState
public override void OnUpdate(float deltaTime)
{
    Vector3 move = forward * raw.y + right * raw.x;
    ctx.characterController.Move(move * ctx.currentMovementSpeed * deltaTime);
}
```

### State-Specific Behavior

Override `OnUpdate()` for per-frame logic (e.g., rotation, animation):

```csharp
public override void OnUpdate(float deltaTime)
{
    // Smooth rotation toward input direction
    if (ctx.stateInputManager.moveInput.magnitude > 0.01f)
    {
        // apply rotation logic
    }
}
```

## Debugging

### Enable Debug Logs

Most classes include `Debug.Log()` statements. Check the console for:
- "Entered [StateName]"
- "StateInputManager: Move event received"
- "[CHOICE] Confirmed choice X" (dialogue system)

### Inspect State Machine State

```csharp
string stateLog = machine.CurrentStateLog();
// Output: "PlayerRootState -> GroundedState -> WalkingState"
Debug.Log(stateLog);
```

### Verify Input Subscription

1. Ensure `InputSubscriber` prefab is in the scene
2. Ensure `InputEventsSO` is assigned in the Inspector
3. Check that `InputActions.inputactions` has the required actions
4. Verify `StateInputManager.Initialize()` is called after scene loads

## Known Limitations & Future Improvements

- **No nested child state navigation UI** — currently manual state references
- **Single root only** — design supports one HFSM per entity

## License

This HFSM system is part of a learning/portfolio project. Feel free to adapt for your own use.

## Contributing

Suggestions and improvements welcome! Some areas for enhancement:

- [ ] Parameterized state transitions (e.g., transition speeds, conditions)
- [ ] State machine visualization tool
- [ ] Automated state scaffolding
- [ ] Performance profiling for large hierarchies
- [ ] Unit tests for state transitions

---

**Last Updated:** November 22, 2025

For questions or issues, refer to the inline code comments or check the project structure above.
