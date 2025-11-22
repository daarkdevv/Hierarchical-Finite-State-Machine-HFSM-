using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class StateMachine
{
    PlayerContext context;
    public State root;
    bool started;

    bool isTransitioning;

    (State from, State to)? pendingTransition;


    public StateMachine(PlayerContext context, State root)
    {
        this.context = context;
        this.root = root;
        if (root != null) root.machine = this;
    }
    public string CurrentStateLog()
    {
        var leaf = root.LeafNodeOfThis();
        return string.Join(" -> ", leaf.PathToRoot().Select(s => s.GetType().Name));
    }
    public void Start()
    {
        if(started)
        return;

        root.Enter();
        started = true;
    }

    public void Tick(float deltaTime)
    {
         if (isTransitioning) return;

        root.Update(deltaTime);
    }

    public IEnumerable<IActivity> GetExitingStateActivities(State from, State lca)
    {
        for (State s = from; s != null && s != lca; s = s.parent)
        {
            foreach (var activity in s.ExitActivities)
                yield return activity;
        }
    }

    public IEnumerable<IActivity> GetEnteringStateActivities(State to, State lca)
    {
        Stack<State> entering = new();
        for (State s = to; s != null && s != lca; s = s.parent)
            entering.Push(s);

        while (entering.Count > 0)
        {
            var s = entering.Pop();
            foreach (var activity in s.EnterActivities)
                yield return activity;
        }
    }

    public async Task ChangeState(State from, State to)
    {
          if (isTransitioning)
        {
            pendingTransition = (from, to);
            return;
        }

        isTransitioning = true;

            State lca = GetLca(from, to);

        var exitActs = GetExitingStateActivities(from, lca).ToList();

        Debug.Log(exitActs.Count);

        if (exitActs.Count > 0)
        {
            await new Sequential(exitActs).RunAsync();
        }

        for (State s = from; s != lca; s = s.parent)
            s.Exit();

        Stack<State> states = new();
        for (State s = to; s != lca; s = s.parent)
            states.Push(s);

        var enterActs = GetEnteringStateActivities(to, lca).ToList();
        if (enterActs.Count > 0)
            await new Sequential(enterActs).RunAsync();

        while (states.Count > 0)
            states.Pop().Enter();

        isTransitioning = false;


        if (pendingTransition.HasValue)
        {
            var next = pendingTransition.Value;
            pendingTransition = null;
            await ChangeState(next.from, next.to);
         }

    }
    
    

    public static State GetLca(State from, State to)
    {
        HashSet<State> fromSet = new(from.PathToRoot());
        foreach (var s in to.PathToRoot())
            if (fromSet.Contains(s)) return s;
        return null;
    }
}
