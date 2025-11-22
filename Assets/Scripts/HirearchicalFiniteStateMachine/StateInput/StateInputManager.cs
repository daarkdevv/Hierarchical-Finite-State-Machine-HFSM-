using UnityEngine;

public class StateInputManager
{
    public Vector2 moveInput;
    public bool sprintPressed;
    public bool crouchPressed;
    public bool dialogueInteractPressed;
    Transform player;
    
    public StateInputManager(Transform player)
    {
        this.player = player;
        Initialize();
    }

    public void Initialize()
    {
        InputSubscriber.Instance.SubscribeToVector2InputEvent(InputType.Move, (v) => moveInput = v);
        InputSubscriber.Instance.SubscribeToBoolInputEvent(InputType.Sprint, (b) => sprintPressed = b);   
        InputSubscriber.Instance.SubscribeToVoidInputEvent(InputType.Crouch, () => crouchPressed = !crouchPressed);
    }

}



