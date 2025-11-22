using UnityEngine;

public class IdleState : State
{
    public IdleState(StateMachine m, State parent, PlayerContext ctx)
        : base(m, parent, ctx) { }

    public override void OnEnter()
    {
        ctx.currentMovementSpeed = 0f;
        Debug.Log("Entered Idle");
    }

    public override State GetTransition()
    {
        // Enter crouch if crouch button toggled
        if (ctx.stateInputManager.crouchPressed)
        {
            return ((GroundedState)parent).crouchState;
        }
        
        // Transition to walking if movement input
        float mag = ctx.stateInputManager.moveInput.magnitude;
        if (mag > 0.01f)
        {
            return ((GroundedState)parent).walkingState;
        }
        
        return null;
    }
}
