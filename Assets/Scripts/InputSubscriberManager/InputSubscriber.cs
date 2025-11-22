using System;
using System.Collections.Generic;
using UnityEngine;

public class InputSubscriber : MonoBehaviour
{
    public static InputSubscriber Instance;

    [SerializeField] private InputEventsSO InputSo;

    private Dictionary<InputType, (Action<Action> sub, Action<Action> unSub)> VoidEvents;
    private Dictionary<InputType, (Action<Action<Vector2>> sub, Action<Action<Vector2>> unSub)> Vector2Events;
    private Dictionary<InputType, (Action<Action<bool>> sub, Action<Action<bool>> unSub)> BoolEvents;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        VoidEvents = new Dictionary<InputType, (Action<Action>, Action<Action>)>
        {
            { InputType.Jump, (h => InputSo.OnJumpEvent += h, h => InputSo.OnJumpEvent -= h) },
            { InputType.Interact, (h => InputSo.OnInteractEvent += h, h => InputSo.OnInteractEvent -= h) },
            { InputType.Crouch, (h => InputSo.OnCoruch += h, h => InputSo.OnCoruch -= h) },
            { InputType.CrouchCancel, (h => InputSo.OnCrouchCancel += h, h => InputSo.OnCrouchCancel -= h) },
            {InputType.DialogueNext,(h => InputSo.OnDialoguePressed += h ,h => InputSo.OnDialoguePressed -= h)}
             
        };

        Vector2Events = new Dictionary<InputType, (Action<Action<Vector2>>, Action<Action<Vector2>>)>
        {
            { InputType.Move, (h => InputSo.OnMoveEvent += h, h => InputSo.OnMoveEvent -= h) },
            { InputType.MouseLook, (h => InputSo.OnMouseLookEvent += h, h => InputSo.OnMouseLookEvent -= h) }
        };

        BoolEvents = new Dictionary<InputType, (Action<Action<bool>>, Action<Action<bool>>)>
        {
            { InputType.Sprint, (h => InputSo.OnSprintEvent += h, h => InputSo.OnSprintEvent -= h) }
        };
    }

    // --- Void Events ---
    public void SubscribeToVoidInputEvent(InputType inputType, Action callback)
    {
        if (VoidEvents.ContainsKey(inputType))
            VoidEvents[inputType].sub(callback);
    }

    public void UnSubscribeToVoidInputEvent(InputType inputType, Action callback)
    {
        if (VoidEvents.ContainsKey(inputType))
            VoidEvents[inputType].unSub(callback);
    }

    // --- Vector2 Events ---
    public void SubscribeToVector2InputEvent(InputType inputType, Action<Vector2> callback)
    {
        if (Vector2Events.ContainsKey(inputType))
            Vector2Events[inputType].sub(callback);
    }

    public void UnSubscribeToVector2InputEvent(InputType inputType, Action<Vector2> callback)
    {
        if (Vector2Events.ContainsKey(inputType))
            Vector2Events[inputType].unSub(callback);
    }

    // --- Bool Events ---
    public void SubscribeToBoolInputEvent(InputType inputType, Action<bool> callback)
    {
        if (BoolEvents.ContainsKey(inputType))
            BoolEvents[inputType].sub(callback);
    }

    public void UnSubscribeToBoolInputEvent(InputType inputType, Action<bool> callback)
    {
        if (BoolEvents.ContainsKey(inputType))
            BoolEvents[inputType].unSub(callback);
    }
}