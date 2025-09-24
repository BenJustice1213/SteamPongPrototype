using UnityEngine;

public class CursorToggle : MonoBehaviour
{
    private bool isCursorLocked = true;

    void Start()
    {
        LockCursor();
    }

    void Update()
    {
        // Unlock with Esc
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        // Lock again on left click if currently unlocked
        if (!isCursorLocked && Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }
    }

    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }
}
