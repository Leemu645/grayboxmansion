using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace GrayBoxMansion
{
    /// <summary>
    /// Looks out from the camera and uses whatever Interactable is under the crosshair.
    /// A tap of E uses a normal object. An object with HoldInteractable fills a bar while E is held.
    /// The Interact action in the input asset is marked Hold, so this also reads the E key directly.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        public static PlayerInteractor Instance { get; private set; }

        [SerializeField] InputActionAsset inputActions;
        [SerializeField] Transform viewCamera;
        [SerializeField] GameObject crosshair;
        [SerializeField] Text promptLabel;
        [SerializeField] Text messageLabel;

        [Min(0.5f)]
        [SerializeField] float interactDistance = 3.5f;

        [Min(0.5f)]
        [SerializeField] float messageSeconds = 4.2f;

        InputActionAsset runtimeActions;
        InputAction interactAction;
        bool inputReady;

        readonly RaycastHit[] hitBuffer = new RaycastHit[16];
        float messageTimeRemaining;

        GameObject holdBar;
        Image holdFill;
        float holdTime;
        bool holdBarReady;
        LookHighlight highlighted;

        void Awake()
        {
            if (Instance != null && Instance != this)
                Debug.LogWarning("More than one PlayerInteractor is in the scene.", this);
            else
                Instance = this;

            if (viewCamera == null)
            {
                Camera child = GetComponentInChildren<Camera>();
                if (child != null)
                    viewCamera = child.transform;
            }

            InputActionAsset source = inputActions != null ? inputActions : InputSystem.actions;
            if (source != null)
                runtimeActions = Instantiate(source);
        }

        void OnEnable()
        {
            inputReady = false;
            if (runtimeActions == null)
            {
                Debug.LogError(
                    "PlayerInteractor on " + name + " has no Input Actions asset.",
                    this);
                return;
            }

            InputActionMap playerMap = runtimeActions.FindActionMap("Player", throwIfNotFound: false);
            interactAction = playerMap != null
                ? playerMap.FindAction("Interact", throwIfNotFound: false)
                : null;

            if (interactAction == null)
            {
                Debug.LogError("The Input Actions asset has no Player/Interact action.", this);
                return;
            }

            interactAction.Enable();
            inputReady = true;
        }

        void OnDisable()
        {
            if (interactAction != null)
                interactAction.Disable();
            inputReady = false;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            if (runtimeActions != null)
                Destroy(runtimeActions);
        }

        void Update()
        {
            TickMessage();

            if (PlayerControl.Suspended)
            {
                ShowPrompt(null);
                UpdateHighlight(null);
                SetHoldVisible(false);
                holdTime = 0f;
                return;
            }

            bool cursorLocked = Cursor.lockState == CursorLockMode.Locked;
            if (crosshair != null && crosshair.activeSelf != cursorLocked)
                crosshair.SetActive(cursorLocked);

            Interactable lookedAt = cursorLocked ? FindLookTarget() : null;
            ShowPrompt(lookedAt);
            UpdateHighlight(lookedAt);

            HoldInteractable hold = lookedAt != null ? lookedAt.GetComponent<HoldInteractable>() : null;
            if (hold == null && lookedAt != null)
                hold = lookedAt.GetComponentInParent<HoldInteractable>();

            if (hold != null && hold.AcceptingHolds && hold.isActiveAndEnabled)
            {
                // A tap still says the line. Holding is what fills the bar.
                if (PressedInteract() && lookedAt != null && !string.IsNullOrEmpty(lookedAt.Message))
                    ShowMessage(lookedAt.Message);
                UpdateHold(hold);
            }
            else
            {
                holdTime = 0f;
                SetHoldVisible(false);
                bool click = cursorLocked && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
                bool useKey = PressedInteract();

                if (lookedAt != null && (useKey || click))
                    Use(lookedAt);
            }
        }

        bool PressedInteract()
        {
            // The Interact action is marked Hold in the input asset, so its performed
            // phase arrives late. Read the key itself so a tap is a tap, and a hold
            // does not fire a second time when that phase completes.
            if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
                return true;
            if (Gamepad.current != null && Gamepad.current.buttonNorth.wasPressedThisFrame)
                return true;
            return false;
        }

        bool HeldInteract()
        {
            if (Keyboard.current != null && Keyboard.current.eKey.isPressed)
                return true;
            if (Gamepad.current != null && Gamepad.current.buttonNorth.isPressed)
                return true;
            return inputReady && interactAction != null && interactAction.IsPressed();
        }

        void UpdateHold(HoldInteractable hold)
        {
            if (!HeldInteract())
            {
                holdTime = 0f;
                SetHoldVisible(false);
                return;
            }

            EnsureHoldBar();
            holdTime += Time.deltaTime;
            float duration = Mathf.Max(0.2f, hold.HoldSeconds);
            SetHoldVisible(true);
            if (holdFill != null)
                holdFill.fillAmount = Mathf.Clamp01(holdTime / duration);

            if (holdTime < duration)
                return;

            holdTime = 0f;
            if (holdFill != null)
                holdFill.fillAmount = 0f;
            hold.RegisterHold();
        }

        void Use(Interactable lookedAt)
        {
            if (!string.IsNullOrEmpty(lookedAt.Message))
                ShowMessage(lookedAt.Message);
            lookedAt.Interact();
        }

        Interactable FindLookTarget()
        {
            if (viewCamera == null)
                return null;

            Ray ray = new Ray(viewCamera.position, viewCamera.forward);
            int count = Physics.RaycastNonAlloc(
                ray,
                hitBuffer,
                interactDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            int closest = -1;
            float closestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                Transform hitTransform = hitBuffer[i].collider.transform;
                if (hitTransform == transform || hitTransform.IsChildOf(transform))
                    continue;

                if (hitBuffer[i].distance < closestDistance)
                {
                    closestDistance = hitBuffer[i].distance;
                    closest = i;
                }
            }

            if (closest < 0)
                return null;

            return hitBuffer[closest].collider.GetComponentInParent<Interactable>();
        }

        void ShowPrompt(Interactable target)
        {
            if (promptLabel == null)
                return;

            bool show = target != null && target.isActiveAndEnabled && target.ShowsPrompt;
            if (promptLabel.gameObject.activeSelf != show)
                promptLabel.gameObject.SetActive(show);

            if (show)
                promptLabel.text = "E: " + target.Prompt;
        }

        void UpdateHighlight(Interactable target)
        {
            LookHighlight next = null;
            if (target != null && target.isActiveAndEnabled)
            {
                next = target.GetComponent<LookHighlight>();
                if (next == null)
                    next = target.GetComponentInParent<LookHighlight>();
            }

            if (next == highlighted)
                return;

            if (highlighted != null)
                highlighted.SetHighlighted(false);
            highlighted = next;
            if (highlighted != null)
                highlighted.SetHighlighted(true);
        }

        public void ShowMessage(string message)
        {
            if (messageLabel == null || string.IsNullOrEmpty(message))
                return;

            messageLabel.text = message;
            messageLabel.gameObject.SetActive(true);
            messageTimeRemaining = messageSeconds;
        }

        void TickMessage()
        {
            if (messageTimeRemaining <= 0f)
                return;

            messageTimeRemaining -= Time.deltaTime;
            if (messageTimeRemaining <= 0f && messageLabel != null)
                messageLabel.gameObject.SetActive(false);
        }

        void EnsureHoldBar()
        {
            if (holdBarReady || promptLabel == null)
                return;

            Canvas canvas = promptLabel.canvas;
            if (canvas == null)
                canvas = promptLabel.GetComponentInParent<Canvas>(true);
            if (canvas == null)
                return;

            Sprite sprite = WhiteSprite();
            holdBar = new GameObject("HoldBar");
            holdBar.transform.SetParent(canvas.transform, false);
            Image background = holdBar.AddComponent<Image>();
            background.sprite = sprite;
            background.color = new Color(0f, 0f, 0f, 0.7f);
            background.raycastTarget = false;
            RectTransform barRect = background.rectTransform;
            barRect.anchorMin = barRect.anchorMax = new Vector2(0.5f, 0.5f);
            barRect.sizeDelta = new Vector2(240f, 16f);
            barRect.anchoredPosition = new Vector2(0f, -68f);

            GameObject fillObject = new GameObject("Fill");
            fillObject.transform.SetParent(holdBar.transform, false);
            holdFill = fillObject.AddComponent<Image>();
            holdFill.sprite = sprite;
            holdFill.color = new Color(0.86f, 0.68f, 0.36f, 1f);
            holdFill.type = Image.Type.Filled;
            holdFill.fillMethod = Image.FillMethod.Horizontal;
            holdFill.fillAmount = 0f;
            holdFill.raycastTarget = false;
            RectTransform fillRect = holdFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);

            holdBar.SetActive(false);
            holdBarReady = true;
        }

        void SetHoldVisible(bool visible)
        {
            if (holdBar != null && holdBar.activeSelf != visible)
                holdBar.SetActive(visible);
        }

        static Sprite WhiteSprite()
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
