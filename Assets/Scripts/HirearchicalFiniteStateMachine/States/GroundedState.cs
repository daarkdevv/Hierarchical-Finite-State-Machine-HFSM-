using UnityEngine;

public class GroundedState : State
{
    public IdleState idleState;
    public WalkingState walkingState;
    public RunningState runningState;
    public CrouchState crouchState;

    public GroundedState(StateMachine m, State parent, PlayerContext ctx) 
        : base(m, parent, ctx) 
    {
        idleState = new IdleState(m, this, ctx);
        walkingState = new WalkingState(m, this, ctx);
        runningState = new RunningState(m, this, ctx);
        crouchState = CrouchState.Create(m, this, ctx);
    }

    public override void OnEnter()
    {
        Debug.Log("Entered Grounded");
    }

    public override State InitialState() => idleState;

    public override State GetTransition()
    {
        // Prioritize airborne transition
        if (!ctx.IsGrounded)
        {
            if (parent is PlayerRootState prs)
                return prs.airBorneState;
        }
        // Let child states handle movement transitions
        return null;
    }
}


