using UnityEngine;
using UnityEngine.InputSystem;

public class CursorUnlocker : MonoBehaviour
{
    private PlayerInputActions inputActions;

    void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
        inputActions.Player.Pause.performed += ctx => UnlockCursor();
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Optional: switch to UI input map if you have one
        // GetComponent<PlayerInput>().SwitchCurrentActionMap("UI");
    }

    void OnDestroy()
    {
        inputActions.Player.Pause.performed -= ctx => UnlockCursor();
        inputActions.Dispose();
    }
}
