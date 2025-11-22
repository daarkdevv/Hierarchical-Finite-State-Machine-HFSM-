using UnityEngine;

public class CrouchState : State
{
    public CrouchState(StateMachine m, State parent, PlayerContext ctx)
        : base(m, parent, ctx) { }

    public override void OnEnter()
    {
        Debug.Log("Entered Crouch");
        ctx.currentMovementSpeed = ctx.walkSpeed * 0.5f;
    }

    public override void OnExit()
    {
        ctx.currentMovementSpeed = ctx.walkSpeed;
    }

    public override State GetTransition()
    {
        // Exit crouch when the crouch toggle is released
        if (!ctx.stateInputManager.crouchPressed)
        {
            if (parent is GroundedState gs) 
                return gs.idleState;
        }

        return null;
    }

    // Attach crouch enter/exit activities at construction time
    public static CrouchState Create(StateMachine m, State parent, PlayerContext ctx)
    {
        var s = new CrouchState(m, parent, ctx);
        s.AddEnterActivity(new CrouchActiveActivity(ctx, true));
        s.AddExitActivity(new CrouchActiveActivity(ctx, false));
        return s;
    }
}
