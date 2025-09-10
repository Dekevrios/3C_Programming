using System;
using System.Runtime.CompilerServices;
using System.Collections;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Action<Vector2> OnMoveInput; 
    private void Update()
    {
        CheckJumpInput();
        CheckSprintInput();
        CheckCrouchInput();
        CheckChangePOVInput();
        CheckClimbInput();
        CheckGlideInput();
        CheckCancelInput();
        CheckPunchInput();
        CheckMainMenuInput();
        CheckMovementInput();
    }

    private void CheckMovementInput()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");
        Debug.Log($"Vertical Input: {verticalInput}, Horizontal Input: {horizontalInput}");

        Vector2 inputAxis = new Vector2(horizontalInput, verticalInput);
        if(OnMoveInput != null)
        {
            OnMoveInput(inputAxis);
        }

    }

    private void CheckJumpInput()
    {
        bool isPressJumpInput = Input.GetKeyDown(KeyCode.Space);
        if (isPressJumpInput)
        {
            Debug.Log("Jump input detected!");
            // Handle jump logic here
        }
    }

    private void CheckSprintInput()
    {
        bool isHoldSprintInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (isHoldSprintInput)
        {
            Debug.Log("Sprint input detected!");
            // Handle sprint logic here
        }
        else
        {
            Debug.Log("no sprint");
        }
    }

    private void CheckCrouchInput()
    {
        bool isPressCrouchInput = Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);

        if (isPressCrouchInput)
        {
            Debug.Log("Crouch ");
            // Handle crouch logic here
        }

    }

    private void CheckChangePOVInput()
    {
        bool isPressChangePOVInput = Input.GetKeyDown(KeyCode.Q);

        if (isPressChangePOVInput)
        {
            Debug.Log("Change POV ");
            // Handle change POV logic here
        }
    }


    private void CheckClimbInput()
    {
        bool isPressClimbInput = Input.GetKeyDown(KeyCode.E);

        if (isPressClimbInput)
        {
            Debug.Log("Climb ");
            // Handle climb logic here
        }
    }


    private void CheckGlideInput()
    {
        bool isPressGlideInput = Input.GetKeyDown(KeyCode.G);

        if (isPressGlideInput)
        {
            Debug.Log("Glide ");
            // Handle glide logic here
        }
    }

    private void CheckCancelInput()
    {
        bool isPressCancelInput = Input.GetKeyDown(KeyCode.C);

        if (isPressCancelInput)
        {
            Debug.Log("Cancel ");
            // Handle cancel logic here
        }

    }

    private void CheckPunchInput()
    {
        bool isPressPunchInput = Input.GetKeyDown(KeyCode.Mouse0);

        if (isPressPunchInput)
        {
            Debug.Log("Punch ");
            // Handle punch logic here
        }

    }

    private void CheckMainMenuInput()
    {
        bool isPressMainMenuInput = Input.GetKeyDown(KeyCode.Escape);
        if (isPressMainMenuInput)
        {
            Debug.Log("Main Menu ");
            // Handle main menu logic here
        }

    }
}

