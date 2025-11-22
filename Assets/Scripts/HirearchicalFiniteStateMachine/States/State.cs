using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public StateMachine machine;
    public State parent;
    public State ActiveChild;
    public PlayerContext ctx;

    public List<IActivity> EnterActivities { get; set; } = new();
    public List<IActivity> ExitActivities { get; set; } = new();

    protected State(StateMachine m, State parent, PlayerContext ctx)
    {
        machine = m;
        this.parent = parent;
        this.ctx = ctx;
    }

    public void AddEnterActivity(IActivity activity)
    {
        if (activity != null) EnterActivities.Add(activity);
    }

    public void AddExitActivity(IActivity activity)
    {
        if (activity != null) ExitActivities.Add(activity);
    }

    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void OnUpdate(float deltaTime) { }

    public void Enter()
    {
        if (parent != null) parent.ActiveChild = this;
        OnEnter();
        

        State child = InitialState();
        if (child != null) child.Enter();

    }

    public void Exit()
    {
        ActiveChild?.Exit();
        ActiveChild = null;
        OnExit();
    }

    public void Update(float deltaTime)
    {
          if (ActiveChild != null)
        {
            ActiveChild.Update(deltaTime);
        }


        State next = GetTransition();

        if (next != null)
        {
            machine.ChangeState(this.LeafNodeOfThis(), next);
            return;
        }

        OnUpdate(deltaTime);
    }

    public virtual State GetTransition() => null;
    public virtual State InitialState() => null;

    public State LeafNodeOfThis()
    {
        State s = this;
        while (s.ActiveChild != null) s = s.ActiveChild;
        return s;
    }

    public IEnumerable<State> PathToRoot()
    {
        for (State s = this; s != null; s = s.parent)
            yield return s;
    }
}
