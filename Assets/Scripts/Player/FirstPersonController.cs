using UnityEngine;
using UnityEngine.InputSystem;

namespace GrayBoxMansion
{
    /// <summary>
    /// First-person movement for the mansion.
    /// Horizontal look turns the body. Vertical look only tilts the camera.
    /// A CharacterController moves the body; gravity and jumping live here.
    /// Input comes from Assets/InputSystem_Actions. The old Input Manager is off,
    /// so Input.GetAxis would throw.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [DisallowMultipleComponent]
    public class FirstPersonController : MonoBehaviour
    {
        const float JumpBufferSeconds = 0.12f;

        [Header("References")]
        [Tooltip("Assets/InputSystem_Actions. Reads Move, Look, Jump, and Sprint from the Player map.")]
        [SerializeField] InputActionAsset inputActions;

        [Tooltip("Camera to tilt up and down. Leave empty to use a child Camera.")]
        [SerializeField] Transform viewCamera;

        [Header("Movement")]
        [Min(0f)]
        [SerializeField] float moveSpeed = 4.4f;

        [Min(0f)]
        [SerializeField] float sprintSpeed = 7f;

        [Min(0f)]
        [SerializeField] float jumpHeight = 0.85f;

        [Min(0f)]
        [SerializeField] float gravity = 20f;

        [Header("Look")]
        [Min(0f)]
        [SerializeField] float mouseSensitivity = 0.1f;

        [Min(0f)]
        [SerializeField] float stickLookSpeed = 200f;

        [SerializeField] float minPitch = -85f;
        [SerializeField] float maxPitch = 85f;

        [Header("Cursor")]
        [SerializeField] bool lockCursorOnPlay = true;

        CharacterController body;
        InputActionAsset runtimeActions;
        InputActionMap playerMap;
        InputAction moveAction;
        InputAction lookAction;
        InputAction jumpAction;
        InputAction sprintAction;

        float verticalVelocity;
        float yaw;
        float pitch;
        float jumpBuffer;
        float stepTimer;
        float airborneTime;
        float timeAlive;
        bool movementLocked;
        bool inputReady;
        bool cursorLocked;
        bool suppressPointerLook;

        void Reset()
        {
            CharacterController capsule = GetComponent<CharacterController>();
            if (capsule == null)
                return;

            capsule.height = 1.75f;
            capsule.radius = 0.28f;
            capsule.center = new Vector3(0f, 0.875f, 0f);
            capsule.stepOffset = 0.4f;
            capsule.slopeLimit = 55f;
            capsule.skinWidth = 0.04f;
        }

        void Awake()
        {
            // Static flags survive a reload. Clear them before the first Update.
            PlayerControl.ResetForSceneLoad();

            body = GetComponent<CharacterController>();

            if (viewCamera == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>();
                if (childCamera != null)
                    viewCamera = childCamera.transform;
            }

            if (viewCamera == null)
            {
                Debug.LogError(
                    "FirstPersonController on " + name + " has no camera. Add a Camera as a child and assign View Camera.",
                    this);
            }

            InputActionAsset source = inputActions != null ? inputActions : InputSystem.actions;
            if (source != null)
                runtimeActions = Instantiate(source);

            yaw = transform.eulerAngles.y;
            if (viewCamera != null)
            {
                pitch = viewCamera.localEulerAngles.x;
                if (pitch > 180f)
                    pitch -= 360f;
            }
        }

        void OnEnable()
        {
            EnableInput();
            if (lockCursorOnPlay)
                SetCursorLocked(true);
        }

        void OnDisable()
        {
            if (playerMap != null)
                playerMap.Disable();

            inputReady = false;

            if (cursorLocked)
                SetCursorLocked(false);
        }

        void OnDestroy()
        {
            if (runtimeActions != null)
                Destroy(runtimeActions);
        }

        void Update()
        {
            if (PlayerControl.Suspended)
                return;

            timeAlive += Time.deltaTime;
            UpdateCursor();
            Look();
            Move();
        }

        void EnableInput()
        {
            inputReady = false;
            playerMap = null;

            if (runtimeActions == null)
            {
                Debug.LogError(
                    "FirstPersonController on " + name + " has no Input Actions asset. Drag Assets/InputSystem_Actions into Input Actions.",
                    this);
                return;
            }

            playerMap = runtimeActions.FindActionMap("Player", throwIfNotFound: false);
            if (playerMap == null)
            {
                Debug.LogError("The Input Actions asset has no 'Player' action map.", this);
                return;
            }

            moveAction = playerMap.FindAction("Move", throwIfNotFound: false);
            lookAction = playerMap.FindAction("Look", throwIfNotFound: false);
            jumpAction = playerMap.FindAction("Jump", throwIfNotFound: false);
            sprintAction = playerMap.FindAction("Sprint", throwIfNotFound: false);

            if (moveAction == null || lookAction == null || jumpAction == null || sprintAction == null)
            {
                Debug.LogError("The Player action map is missing Move, Look, Jump, or Sprint.", this);
                return;
            }

            playerMap.Enable();
            inputReady = true;
        }

        void UpdateCursor()
        {
            bool releaseCursor = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
            if (!releaseCursor && Gamepad.current != null)
                releaseCursor = Gamepad.current.startButton.wasPressedThisFrame;

            if (cursorLocked && releaseCursor)
            {
                SetCursorLocked(false);
                return;
            }

            if (!cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                SetCursorLocked(true);
        }

        void SetCursorLocked(bool locked)
        {
            if (locked && !cursorLocked)
                suppressPointerLook = true;

            cursorLocked = locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        void Look()
        {
            if (viewCamera == null)
            {
                suppressPointerLook = false;
                return;
            }

            if (inputReady && !movementLocked)
            {
                Vector2 look = lookAction.ReadValue<Vector2>();
                bool fromPointer = lookAction.activeControl != null && lookAction.activeControl.device is Pointer;

                if (fromPointer && cursorLocked && !suppressPointerLook)
                {
                    yaw += look.x * mouseSensitivity;
                    pitch -= look.y * mouseSensitivity;
                }
                else if (!fromPointer)
                {
                    yaw += look.x * stickLookSpeed * Time.deltaTime;
                    pitch -= look.y * stickLookSpeed * Time.deltaTime;
                }

                float low = Mathf.Min(minPitch, maxPitch);
                float high = Mathf.Max(minPitch, maxPitch);
                pitch = Mathf.Clamp(pitch, low, high);
            }

            suppressPointerLook = false;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            viewCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        public Transform View => viewCamera;

        public void SetMovementLocked(bool locked)
        {
            movementLocked = locked;
        }

        void Move()
        {
            if (movementLocked || !body.enabled)
                return;

            bool grounded = body.isGrounded;
            float fallSpeed = verticalVelocity;
            if (grounded && verticalVelocity < 0f)
                verticalVelocity = -2f;

            float gravityAccel = -Mathf.Abs(gravity);

            if (inputReady && jumpAction.WasPressedThisFrame())
                jumpBuffer = JumpBufferSeconds;

            bool jumped = false;
            if (grounded && jumpBuffer > 0f)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravityAccel);
                jumpBuffer = 0f;
                jumped = true;
            }
            else if (jumpBuffer > 0f)
            {
                jumpBuffer -= Time.deltaTime;
            }

            verticalVelocity += gravityAccel * Time.deltaTime;

            Vector3 wish = Vector3.zero;
            if (inputReady)
            {
                Vector2 move = moveAction.ReadValue<Vector2>();
                wish = transform.right * move.x + transform.forward * move.y;
                if (wish.sqrMagnitude > 1f)
                    wish.Normalize();
            }

            bool sprinting = inputReady && sprintAction.IsPressed();
            float speed = sprinting ? sprintSpeed : moveSpeed;

            Vector3 velocity = wish * speed;
            velocity.y = verticalVelocity;
            body.Move(velocity * Time.deltaTime);

            bool landed = timeAlive > 0.4f && grounded && airborneTime > 0.12f && fallSpeed < -4f;
            if (landed && GameAudio.Instance != null)
                GameAudio.Instance.PlayLand(transform.position);

            if (jumped)
            {
                airborneTime = 0.2f;
                if (GameAudio.Instance != null)
                    GameAudio.Instance.PlayJump();
            }
            else if (grounded)
            {
                airborneTime = 0f;
            }
            else
            {
                airborneTime += Time.deltaTime;
            }

            Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
            if (grounded && !jumped && !landed && flat.sqrMagnitude > 0.4f)
            {
                stepTimer -= Time.deltaTime;
                if (stepTimer <= 0f)
                {
                    if (GameAudio.Instance != null)
                        GameAudio.Instance.PlayFootstep(transform.position, sprinting);
                    stepTimer = sprinting ? 0.32f : 0.48f;
                }
            }
            else
            {
                stepTimer = Mathf.Min(stepTimer, 0.12f);
            }
        }
    }
}
