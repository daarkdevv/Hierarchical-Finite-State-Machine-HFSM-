using UnityEngine;

public class RunningState : State
{
    public RunningState(StateMachine m, State parent, PlayerContext ctx)
        : base(m, parent, ctx) { }

    public override void OnEnter()
    {
        ctx.currentMovementSpeed = ctx.sprintSpeed;
        Debug.Log("Entered Running");
    }

    public override State GetTransition()
    {
        // Enter crouch if crouch button toggled
        if (ctx.stateInputManager.crouchPressed)
        {
            if (parent is GroundedState gs) 
                return gs.crouchState;
        }
        
        float mag = ctx.stateInputManager.moveInput.magnitude;
        if (mag < 0.01f)
        {
            if (parent is GroundedState gs) 
                return gs.idleState;
        }
        
        if (!ctx.stateInputManager.sprintPressed)
        {
            if (parent is GroundedState gs) 
                return gs.walkingState;
        }
        
        return null;
    }
}
