using TMPro;
using UnityEngine;
//#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
//#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header ("Debugging")]
        public bool m_showDebugText = false;
        public TMP_Text m_dpadText;
        public TMP_Text m_leftStickText;

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

        [Header("BTS Android TV")]
		public bool m_invertDpadATVY = true;
		public Vector2 m_dapdATVScale = new Vector2(1.0f, 30.0f);
		public Vector2 m_nonDpadScale = new Vector2(1.0f, 2.0f);
		public InputAction m_dpadATV;
		public InputAction m_dpadJumpButtonATV;

        public Vector2 m_moveDeadZone = new Vector2(0.25f, 0.25f);

        // Store separate inputs for left stick and D-pad
        private Vector2 leftStickInput;
        private Vector2 dPadInput;

        // Float to hold the axis value
        private float turretRotate;

        private bool m_moveYIsLook = false;

        public float TurretRotate
        {
            get
            {
                return turretRotate;
            }
        }

//#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
        public void OnMove(InputValue value)
        {
            leftStickInput = value.Get<Vector2>();

            if (m_showDebugText && null != m_leftStickText)
            {
                m_leftStickText.text = "" + leftStickInput;
            }

            UpdateMoveInput();
        }

        public void OnDpad(InputValue value)
        {
            dPadInput = value.Get<Vector2>();

            if (m_showDebugText && null != m_dpadText)
            {
                m_dpadText.text = "" + dPadInput;
            }

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
//#endif

        private void Awake()
        {
            // Ensure initial values are zeroed out
            leftStickInput = Vector2.zero;
            dPadInput = Vector2.zero;
            move = Vector2.zero;
        }

        public void OnEnable()
		{

			if (BTS_TV_Type.IsAndroidTV || RuntimePlatform.tvOS == Application.platform)
			{
				m_dpadATV.Enable();
				m_dpadATV.performed += OnDpadPerformed;
				m_dpadATV.canceled += OnDpadCanceled;

				m_dpadJumpButtonATV.Enable();
				m_dpadJumpButtonATV.performed += OnDpadJumpPerformed;
				m_dpadJumpButtonATV.canceled += OnDpadJumpCanceled;
			}
		}

        public void OnDisable()
		{
			if (BTS_TV_Type.IsAndroidTV || RuntimePlatform.tvOS == Application.platform)
			{
				m_dpadATV.Disable();
				m_dpadATV.performed -= OnDpadPerformed;
				m_dpadATV.canceled -= OnDpadCanceled;

				m_dpadJumpButtonATV.Disable();
				m_dpadJumpButtonATV.performed -= OnDpadJumpPerformed;
				m_dpadJumpButtonATV.canceled -= OnDpadJumpCanceled;
			}
		}

        void OnDpadCanceled(InputAction.CallbackContext context)
		{
			OnDpadPerformed(context);
		}

        void OnDpadPerformed(InputAction.CallbackContext context)
		{
			Vector2 l_value = context.ReadValue<Vector2>();

			l_value.x = Mathf.Abs(l_value.x) < m_moveDeadZone.x ? 0 : l_value.x;
			l_value.y = Mathf.Abs(l_value.y) < m_moveDeadZone.y ? 0 : l_value.y;

			if (m_moveYIsLook)
			{
				l_value.x *= m_dapdATVScale.x;
				l_value.y *= m_dapdATVScale.y;
			}

			HandleMovement(l_value);
		}

        private void HandleMovement(Vector2 l_value)
		{
			if (m_moveYIsLook)
			{
				if (m_invertDpadATVY)
				{
					l_value.y = -l_value.y;
				}

				Vector2 l_YIsLook = new Vector2(l_value.y /* * 1.5f*/, 0);
				LookInput(l_YIsLook);
				l_value.y = 0;
			}

			MoveInput(l_value);
		}

        void OnDpadJumpPerformed(InputAction.CallbackContext context)
		{
			JumpInput(true);
		}

		void OnDpadJumpCanceled(InputAction.CallbackContext context)
		{
			JumpInput(false);
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