using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "New Input Event", menuName = "Events/Input Event")]
public class InputEventsSO : ScriptableObject
{
    [Header("Input Asset")]
    public InputActionAsset inputAsset;

    [Header("Input Action Names")]
    public string moveActionName = "Move";
    public string jumpActionName = "Jump";
    public string interactActionName = "Interact";
    public string mouseLookName = "Look";
    public string sprintActionName = "Sprint";
    public string CrouchActionName = "Crouch";
    public string CrouchCancelName = "CrouchCancel";
    public string DialgoueActionName = "DialogueNext";

    private InputAction moveAction, jumpAction, interactAction, mouseLookAction, sprintAction , crouchAction,DialogueAction;

    // Public events
    public event Action<Vector2> OnMoveEvent;
    public event Action<Vector2> OnMouseLookEvent;
    public event Action OnJumpEvent;
    public event Action OnInteractEvent;
    public event Action OnCoruch;
    public event Action OnCrouchCancel;
    public event Action OnDialoguePressed;
    public event Action<bool> OnSprintEvent;

    private void OnEnable()
    {
        // Find & enable actions
        moveAction = EnableAction(moveActionName, 
            ctx => OnMoveEvent?.Invoke(ctx.ReadValue<Vector2>()),
            ctx => OnMoveEvent?.Invoke(Vector2.zero));

        mouseLookAction = EnableAction(mouseLookName, 
            ctx => OnMouseLookEvent?.Invoke(ctx.ReadValue<Vector2>()),
            ctx => OnMouseLookEvent?.Invoke(Vector2.zero));

        jumpAction = EnableAction(jumpActionName, ctx => OnJumpEvent?.Invoke());
        interactAction = EnableAction(interactActionName, ctx => OnInteractEvent?.Invoke());

        sprintAction = EnableAction(sprintActionName,
            ctx => OnSprintEvent?.Invoke(true),
            ctx => OnSprintEvent?.Invoke(false));

        crouchAction = EnableAction(CrouchActionName, (ctx) => OnCoruch?.Invoke(), (ctx) => OnCrouchCancel?.Invoke());

        DialogueAction = EnableAction(DialgoueActionName, (ctx) => OnDialoguePressed?.Invoke());
             
    }

    private void OnDisable()
    {
        DisableAction(moveAction);
        DisableAction(mouseLookAction);
        DisableAction(jumpAction);
        DisableAction(interactAction);
        DisableAction(sprintAction);
        DisableAction(crouchAction);
        DisableAction(DialogueAction);
    }

private InputAction EnableAction(string name, Action<InputAction.CallbackContext> performed, Action<InputAction.CallbackContext> canceled = null)
{
    InputAction action = inputAsset.FindAction(name);
    if (action == null)
    {
        Debug.LogError($"InputEventsSO: Action '{name}' not found in InputActionAsset!");
        return null;
    }

    action.Enable();
    if (performed != null) action.performed += performed;
    if (canceled != null) action.canceled += canceled;

    return action;
}

    private void DisableAction(InputAction action)
    {
        if (action == null) return;
        action.Disable();
    }
}