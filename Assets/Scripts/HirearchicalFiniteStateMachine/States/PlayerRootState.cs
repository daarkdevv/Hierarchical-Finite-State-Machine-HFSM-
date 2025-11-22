using UnityEngine;

public class PlayerRootState : State
{
    public GroundedState Grounded;
    public AirBorneState airBorneState;

    public PlayerRootState(StateMachine m, State parent, PlayerContext ctx)
        : base(m, parent, ctx)
    {
        
    }

    public void SetStateMachine(StateMachine m)
    {
        this.machine = m;
        Grounded = new GroundedState(m, this, ctx); // parent must be this
        airBorneState = new(m, this, ctx);
    }

    public override void OnEnter()
    {
        Debug.Log("Entered Root");
    }

    public override void OnUpdate(float deltaTime)
    {
        
    }



    public override State GetTransition()
    {
        // Decide when to move between grounded and airborne subtrees.
        // If player is not grounded and we're currently in the grounded subtree, switch to airborne.
        return null;
    }

    public override State InitialState() => Grounded;
}
