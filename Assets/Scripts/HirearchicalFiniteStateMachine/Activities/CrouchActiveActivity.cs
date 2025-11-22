using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class CrouchActiveActivity : IActivity
{
    private readonly Transform cameraPos;
    private readonly bool isCrouchingDown;
    private readonly PlayerContext playerContext;

    public CrouchActiveActivity(
        PlayerContext playerContext,
        bool isCrouchingDown
    )
    {
        this.isCrouchingDown = isCrouchingDown;
        this.playerContext = playerContext;
        this.cameraPos = playerContext.cameraTransform;
    }

    public override async Task PerformActivity()
    {
        if (isCrouchingDown)
        {
            Debug.Log("CrouchingStart");
            playerContext.currentMovementSpeed = playerContext.crouchSpeed;
            await CrouchDownAsync();
            Debug.Log("CrouchingEnd");
        }
        else
        {
            Debug.Log("StandingUpStart");
            await StandUpAsync();
            Debug.Log("StandingUpEnd");
        }
    }

    private async Task CrouchDownAsync()
    {
        await Tween.LocalPositionY(
            cameraPos,
            playerContext.crouchHeight,
            playerContext.crouchTransitionDuration,
             playerContext.crouchEasing
        );
    
    }

    private async Task StandUpAsync()
    {
        await Tween.LocalPositionY(
            cameraPos,
            playerContext.standingHeight,
            playerContext.crouchTransitionDuration,
              playerContext.crouchEasing
        );
        
    }
}
