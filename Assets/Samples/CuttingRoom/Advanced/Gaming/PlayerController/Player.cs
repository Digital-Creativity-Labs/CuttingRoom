using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    /// <summary>
    /// Movement speed of the character controller.
    /// </summary>
    [SerializeField]
    private Vector3 movementSpeed = Vector3.one;

    /// <summary>
    /// Rotation speed of the player.
    /// </summary>
    [SerializeField]
    private Vector3 rotationSpeed = Vector3.one;

    /// <summary>
    /// Character controller.
    /// </summary>
    [SerializeField]
    private CharacterController characterController = null;

    /// <summary>
    /// Angle which the x look axis cannot exceed in a positive or negative direction.
    /// </summary>
    [SerializeField]
    private float xLookAxisRange = 30.0f;

    [SerializeField]
    private bool invertVerticalLook = false;

    /// <summary>
    /// Stores the move input value.
    /// </summary>
    private Vector2 moveInputValue = default;

    private void Update()
    {
        Vector3 forwardMovement = transform.forward * moveInputValue.y * movementSpeed.x;

        Vector3 lateralMovement = transform.right * moveInputValue.x * movementSpeed.z;

        Vector3 movementDelta = (forwardMovement + lateralMovement) * Time.deltaTime;

        movementDelta.y = movementSpeed.y * Time.deltaTime;

        characterController.Move(movementDelta);
    }

    #region Input callbacks

    public void OnMove(InputValue inputValue)
    {
        moveInputValue = inputValue.Get<Vector2>();
    }

    public void OnLook(InputValue inputValue)
    {
        Vector2 inputVector = inputValue.Get<Vector2>();

        // The y axis of the input method (up down on mouse or stick) multiplied by rotation speed on the x axis.
        float verticalLookDelta = inputVector.y * rotationSpeed.x;

        if (invertVerticalLook)
        {
            verticalLookDelta = -verticalLookDelta;
        }

        Vector3 eulerAngles = transform.rotation.eulerAngles + new Vector3(verticalLookDelta, inputVector.x * rotationSpeed.y, 0.0f);

        float clampedXLook = eulerAngles.x;

        // Apply clamp to up down look direction (prevents camera flipping upside down).
        // If less than clamped rotation and on the top side of the look "sphere" or greater than the look range and on the bottom side of the "look" sphere.
        if (clampedXLook < 360.0f - xLookAxisRange && clampedXLook > 180.0f || clampedXLook > xLookAxisRange && clampedXLook < 180.0f)
        {
            // Clamp to 360 - look range.
            if (clampedXLook > 180.0f)
            {
                clampedXLook = 360.0f - xLookAxisRange;
            }
            // Clamp to 0 + look range.
            else
            {
                clampedXLook = xLookAxisRange;
            }
        }

        transform.rotation = Quaternion.Euler(new Vector3(clampedXLook, eulerAngles.y, eulerAngles.z));
    }

    #endregion
}
