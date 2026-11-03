using System;
using UnityEngine;

namespace GrayBoxMansion
{
    /// <summary>
    /// The old lock on the front door. Four slots, one for each relic.
    /// The selected hotbar item is what gets placed. Any order works.
    /// </summary>
    public class RelicLock : MonoBehaviour
    {
        [Serializable]
        public class Slot
        {
            public ItemId relic = ItemId.None;
            public GameObject filledVisual;
        }

        [SerializeField] Slot[] slots = new Slot[4];
        [SerializeField] MansionDoor frontDoor;
        [SerializeField] EscapeSequence ending;
        [SerializeField] Light glow;

        int filled;
        bool opened;

        public void TryInsert()
        {
            if (opened)
            {
                Say("The door is open.");
                return;
            }

            ItemId held = Inventory.Instance != null ? Inventory.Instance.Selected : ItemId.None;
            Slot match = FindEmptySlot(held);
            if (match != null && Inventory.Instance != null && Inventory.Instance.Remove(held))
            {
                if (match.filledVisual != null)
                    match.filledVisual.SetActive(true);
                filled++;
                RefreshGlow();
                if (GameAudio.Instance != null)
                    GameAudio.Instance.PlaySlotFill(transform.position, filled);

                if (filled >= RequiredCount())
                {
                    opened = true;
                    Say("The last piece sinks into the lock. The door opens.");
                    if (GameAudio.Instance != null)
                        GameAudio.Instance.PlayLockUnlock(transform.position);
                    if (frontDoor != null)
                        frontDoor.ForceOpen();
                    if (ending != null)
                        ending.Begin();
                    return;
                }

                int left = RequiredCount() - filled;
                string noun = left == 1 ? "slot remains." : "slots remain.";
                Say(ItemNames.Label(held) + " settles into the lock. " + left + " empty " + noun);
                return;
            }

            if (held != ItemId.None && !ItemNames.IsRelic(held) && GameAudio.Instance != null)
                GameAudio.Instance.PlayLockRattle(transform.position);

            if (filled <= 0)
            {
                Say("Four empty slots... something belongs here.");
                return;
            }

            int remaining = RequiredCount() - filled;
            Say(remaining + " empty " + (remaining == 1 ? "slot remains." : "slots remain."));
        }

        int RequiredCount()
        {
            if (slots == null || slots.Length == 0)
                return 4;
            return slots.Length;
        }

        Slot FindEmptySlot(ItemId relic)
        {
            if (!ItemNames.IsRelic(relic) || slots == null)
                return null;

            for (int i = 0; i < slots.Length; i++)
            {
                Slot slot = slots[i];
                if (slot == null || slot.relic != relic)
                    continue;
                if (slot.filledVisual != null && slot.filledVisual.activeSelf)
                    return null;
                return slot;
            }

            return null;
        }

        void RefreshGlow()
        {
            if (glow == null)
                return;
            glow.intensity = 0.15f + filled * 0.35f;
        }

        static void Say(string line)
        {
            if (PlayerInteractor.Instance != null)
                PlayerInteractor.Instance.ShowMessage(line);
        }
    }
}
