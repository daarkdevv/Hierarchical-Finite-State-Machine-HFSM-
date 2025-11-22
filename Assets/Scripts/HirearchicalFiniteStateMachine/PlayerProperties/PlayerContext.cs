using System;
using PrimeTween;
using UnityEngine;

public class PlayerContext
{
    public CharacterController characterController;
    public Transform cameraTransform;
    public Transform Player;
    public bool sprintPressedThisFrame;
    public bool crouchPressedThisFrame;
    public float currentMovementSpeed = 3f;
    public float walkSpeed = 3f;
    public float sprintSpeed = 7f;
    public float airborneSpeed = 2f;
    public float crouchSpeed = 1f;
    public float crouchHeight;
    public float crouchTransitionDuration = 1.5f;
    public float standingHeight;
    public bool IsGrounded;

    public Ease crouchEasing;

    public StateInputManager stateInputManager;

    public PlayerContext(PlayerStateDriver driver , CheckIsGrounded checkIsGrounded)
    {
        // Get components from driver
        characterController = driver.characterController;
        cameraTransform = driver.cameraTransform;
        Player = driver.playerTransform;
        crouchEasing = driver.playerConfig.crouchEasing;
        standingHeight = cameraTransform.localPosition.y;

        stateInputManager = new(Player);

        // Copy movement values from config
        if (driver.playerConfig != null)
        {
            currentMovementSpeed = driver.playerConfig.currentMovementSpeed;
            walkSpeed = driver.playerConfig.walkSpeed;
            sprintSpeed = driver.playerConfig.sprintSpeed;
            airborneSpeed = driver.playerConfig.airborneSpeed;
            crouchSpeed = driver.playerConfig.crouchSpeed;
            crouchTransitionDuration = driver.playerConfig.crouchHeightTransitionSpeed;
            crouchHeight = driver.playerConfig.crouchHeight;
        }

        checkIsGrounded.GroundedChanged += (b) => IsGrounded = b; 

    }

}

[CreateAssetMenu(fileName = "PlayerContext", menuName = "PlayerConfig", order = 0)]
public class PlayerConfig : ScriptableObject
{
    public float currentMovementSpeed = 3f;
    public float walkSpeed = 3f;
    public float sprintSpeed = 7f;
    public float airborneSpeed = 2f;
    public float crouchSpeed = 1f;
    public float crouchHeight;
    public float crouchHeightTransitionSpeed = 1.5f;
    public Ease crouchEasing;
}

