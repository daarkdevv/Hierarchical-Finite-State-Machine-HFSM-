using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PrimeTween;
using UnityEngine;

public class PlayerStateDriver : MonoBehaviour
{
    StateMachine machine;
    PlayerContext ctx;
    public PlayerConfig playerConfig;
    string LastStates;

    // Components moved from PlayerConfig
    public CharacterController characterController;
    public Transform cameraTransform;
    public Transform playerTransform;
    public CheckIsGrounded checkIsGrounded;

    void Start()
    {
        // Get required components
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
            Debug.LogError("PlayerStateDriver: Missing CharacterController component!");

        cameraTransform = Camera.main?.transform;
        if (cameraTransform == null)
            Debug.LogError("PlayerStateDriver: No main camera found!");

        playerTransform = GetComponent<Transform>();

        // Create context with this driver's components
        ctx = new PlayerContext(this,checkIsGrounded);
        
        // Initialize state machine
        State playerroot = new PlayerRootState(null, null, ctx);
        machine = new StateMachine(ctx, playerroot);
        ((PlayerRootState)playerroot).SetStateMachine(machine);
        machine.Start();
    }

    void Update()
    {
        var stateslog = machine.CurrentStateLog();

        if (stateslog != LastStates)
        {
            Debug.Log(stateslog);
            LastStates = stateslog;
        }

        machine.Tick(Time.deltaTime);

        if (ctx.characterController != null)
        {
            // moveInput: Vector2 (x = strafe, y = forward)
            Vector2 raw = ctx.stateInputManager.moveInput;
            Vector3 moveInput = ctx.Player.forward * raw.y + ctx.Player.right * raw.x; 
            Vector3 movement = moveInput * ctx.currentMovementSpeed * Time.deltaTime;
            ctx.characterController.Move(movement);
        }
    }
}






