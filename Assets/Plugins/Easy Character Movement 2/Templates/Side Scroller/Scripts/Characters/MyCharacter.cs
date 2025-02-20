﻿using UnityEngine;

namespace EasyCharacterMovement.Templates.SideScrollerTemplate
{
    public class MyCharacter : Character
    {
        // TODO Add your game custom code here...

        protected override void HandleInput()
        {
            // Add horizontal input movement (in world space)

            float movementDirection1D = Input.GetAxisRaw("Horizontal");
            
            SetMovementDirection(Vector3.right * movementDirection1D);

            // Jump

            if (Input.GetButton("Jump"))
            {
                Jump();
            }
            else if (Input.GetButtonUp("Jump"))
            {
                StopJumping();
            }

            // Snap side to side rotation

            if (movementDirection1D != 0.0f)
                SetYaw(movementDirection1D * 90.0f);
        }

        protected override void OnOnEnable()
        {
            // Call base method implementation

            base.OnOnEnable();

            // Disable character rotation

            SetRotationMode(RotationMode.None);
        }
    }
}
