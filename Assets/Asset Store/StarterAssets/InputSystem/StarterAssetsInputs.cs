using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        // Store separate inputs for left stick and D-pad
        private Vector2 leftStickInput;
        private Vector2 dPadInput;

        // Float to hold the axis value
        public float turretRotate;


#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        public void OnMove(InputValue value)
        {
            leftStickInput = value.Get<Vector2>();
            UpdateMoveInput();
        }

        public void OnDpad(InputValue value)
        {
            dPadInput = value.Get<Vector2>();
            UpdateMoveInput();
        }

        public void OnTurretRotate(InputValue value)
        {
            turretRotate = value.Get<float>(); // Axis outputs a float (-1 to 1)
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }
#endif

        private void Awake()
        {
            // Ensure initial values are zeroed out
            leftStickInput = Vector2.zero;
            dPadInput = Vector2.zero;
            move = Vector2.zero;
        }

        private void UpdateMoveInput()
        {
            // On tvOS, ignore D-pad due to bug; use only left stick
            if (Application.platform == RuntimePlatform.tvOS)
            {
                move = leftStickInput;
            }
            // On other platforms, combine left stick and D-pad (D-pad takes precedence if active)
            else
            {
                move = dPadInput != Vector2.zero ? dPadInput : leftStickInput;
            }
        }

        public void MoveInput(Vector2 newMoveDirection)
        {
            leftStickInput = newMoveDirection;
            UpdateMoveInput();
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}