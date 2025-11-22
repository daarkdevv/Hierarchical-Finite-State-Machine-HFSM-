using UnityEngine;

public class AirBorneState : State
{
    public AirBorneState(StateMachine m, State parent, PlayerContext ctx)
        : base(m, parent, ctx) { }

    public override void OnEnter()
    {
        Debug.Log("Entered Airborne");
        ctx.currentMovementSpeed = ctx.airborneSpeed;
    }

    public override State GetTransition()
    {
        if (ctx.IsGrounded)
        {
            if (parent is PlayerRootState prs)
                return prs.Grounded;
        }
        return null;
    }
}
