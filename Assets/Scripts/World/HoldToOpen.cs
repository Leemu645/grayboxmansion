using UnityEngine;

namespace GrayBoxMansion
{
    /// <summary>
    /// A drawer or cupboard that stays shut until the player holds E through one full bar.
    /// Then it slides or swings and reveals whatever was hidden inside.
    /// </summary>
    public class HoldToOpen : MonoBehaviour
    {
        [SerializeField] Transform movingPart;
        [SerializeField] Vector3 localSlide;
        [SerializeField] float yawSwing;
        [SerializeField] GameObject reveal;
        [SerializeField] string openedMessage = "It gives.";
        [SerializeField] float moveSpeed = 1.6f;

        Vector3 closedPosition;
        Quaternion closedRotation;
        float shown;
        bool opened;

        void Awake()
        {
            if (movingPart != null)
            {
                closedPosition = movingPart.localPosition;
                closedRotation = movingPart.localRotation;
            }

            if (reveal != null)
                reveal.SetActive(false);

            HoldInteractable hold = GetComponent<HoldInteractable>();
            if (hold != null)
                hold.HoldCompleted += OnHoldCompleted;
        }

        void OnDestroy()
        {
            HoldInteractable hold = GetComponent<HoldInteractable>();
            if (hold != null)
                hold.HoldCompleted -= OnHoldCompleted;
        }

        void OnHoldCompleted(int count)
        {
            if (opened || count < 1)
                return;

            opened = true;
            HoldInteractable hold = GetComponent<HoldInteractable>();
            if (hold != null)
                hold.AcceptingHolds = false;

            if (reveal != null)
                reveal.SetActive(true);

            if (PlayerInteractor.Instance != null && !string.IsNullOrEmpty(openedMessage))
                PlayerInteractor.Instance.ShowMessage(openedMessage);
            if (GameAudio.Instance != null)
                GameAudio.Instance.PlayDoorCreak(transform.position);
        }

        void Update()
        {
            if (!opened || movingPart == null)
                return;

            shown = Mathf.MoveTowards(shown, 1f, moveSpeed * Time.deltaTime);
            movingPart.localPosition = closedPosition + localSlide * shown;
            if (Mathf.Abs(yawSwing) > 0.01f)
                movingPart.localRotation = closedRotation * Quaternion.Euler(0f, yawSwing * shown, 0f);
        }
    }
}
