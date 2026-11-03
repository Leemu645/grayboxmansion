using UnityEngine;

namespace GrayBoxMansion
{
    /// <summary>
    /// A hinged door. E opens and closes it. A required key must be the selected
    /// hotbar item the first time. After that it stays unlocked.
    /// The front door stays shut until ForceOpen, which the relic lock calls.
    /// </summary>
    public class MansionDoor : MonoBehaviour
    {
        [SerializeField] ItemId requiredKey = ItemId.None;
        [SerializeField] bool sealedUntilForced;
        [SerializeField] float openAngle = 100f;
        [SerializeField] float swingSpeed = 140f;
        [SerializeField] string lockedMessage = "It's locked.";
        [SerializeField] string unlockMessage = "The key turns.";
        [SerializeField] string openedMessage = "";
        [SerializeField] string closedMessage = "";

        Quaternion closedRotation;
        float angle;
        float targetAngle;
        bool unlocked;
        bool released;

        public bool IsOpen => targetAngle != 0f;

        void Awake()
        {
            closedRotation = transform.localRotation;
            unlocked = requiredKey == ItemId.None && !sealedUntilForced;
        }

        void Update()
        {
            if (Mathf.Abs(angle - targetAngle) < 0.05f)
            {
                angle = targetAngle;
            }
            else
            {
                angle = Mathf.MoveTowards(angle, targetAngle, swingSpeed * Time.deltaTime);
            }

            transform.localRotation = closedRotation * Quaternion.Euler(0f, angle, 0f);
        }

        public void TryToggle()
        {
            if (sealedUntilForced && !released)
            {
                Say("Four empty slots... something belongs here.");
                if (GameAudio.Instance != null)
                    GameAudio.Instance.PlayLockRattle(transform.position);
                return;
            }

            if (!unlocked)
            {
                bool keyMatches = requiredKey != ItemId.None
                    && Inventory.Instance != null
                    && Inventory.Instance.Selected == requiredKey;

                if (!keyMatches)
                {
                    Say(string.IsNullOrEmpty(lockedMessage) ? "It's locked." : lockedMessage);
                    if (GameAudio.Instance != null)
                        GameAudio.Instance.PlayLockRattle(transform.position);
                    return;
                }

                unlocked = true;
                Say(string.IsNullOrEmpty(unlockMessage) ? "The key turns." : unlockMessage);
                if (GameAudio.Instance != null)
                    GameAudio.Instance.PlayLockUnlock(transform.position);
                SwingOpen(playCreak: true);
                return;
            }

            if (targetAngle == 0f)
            {
                SwingOpen(playCreak: true);
                if (!string.IsNullOrEmpty(openedMessage))
                    Say(openedMessage);
            }
            else
            {
                targetAngle = 0f;
                if (GameAudio.Instance != null)
                    GameAudio.Instance.PlayDoorCreak(transform.position);
                if (!string.IsNullOrEmpty(closedMessage))
                    Say(closedMessage);
            }
        }

        public void ForceOpen()
        {
            released = true;
            unlocked = true;
            SwingOpen(playCreak: true);
        }

        void SwingOpen(bool playCreak)
        {
            targetAngle = openAngle;
            if (playCreak && GameAudio.Instance != null)
                GameAudio.Instance.PlayDoorCreak(transform.position);
        }

        static void Say(string line)
        {
            if (PlayerInteractor.Instance != null && !string.IsNullOrEmpty(line))
                PlayerInteractor.Instance.ShowMessage(line);
        }
    }
}
