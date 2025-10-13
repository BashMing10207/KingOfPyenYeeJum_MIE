using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputManagerCompo : GetCompoableBase, InputSystem_Actions.IUIActions
{
    private InputSystem_Actions _inputActions;

    public UnityEvent OnTouchDown;
    public UnityEvent OnTouchup;
    //public UnityEvent OnTouchDown;

    private void Awake()
    {
        ActiveInput();
    }
    public void ActiveInput()
    {
        if (_inputActions == null)
        {
            _inputActions = new InputSystem_Actions();
            //_inputActions.Player.SetCallbacks(this);
            _inputActions.UI.SetCallbacks(this);
        }
        _inputActions.Player.Enable();
        _inputActions.UI.Enable();
    }
    public void DisActivePlayerInput()
    {
        _inputActions.Player.Disable();
    }
    public void OnCancel(InputAction.CallbackContext context)
    {
        
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if(context.action.WasPressedThisFrame())
        {
            OnTouchDown?.Invoke();
        }
        else if(context.action.WasReleasedThisFrame())
        {
            Debug.Log("mouseUp");
            OnTouchup?.Invoke();
        }
        Debug.Log("mous");
    }

    public void OnMiddleClick(InputAction.CallbackContext context)
    {
        
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        
    }

    public void OnPoint(InputAction.CallbackContext context)
    {
         
    }

    public void OnRightClick(InputAction.CallbackContext context)
    {
         
    }

    public void OnScrollWheel(InputAction.CallbackContext context)
    {
         
    }

    public void OnSubmit(InputAction.CallbackContext context)
    {
         
    }

    public void OnTrackedDeviceOrientation(InputAction.CallbackContext context)
    {
         
    }

    public void OnTrackedDevicePosition(InputAction.CallbackContext context)
    {
         
    }
}
