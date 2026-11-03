using System;
using UnityEngine;

namespace GrayBoxMansion
{
    /// <summary>
    /// Used by holding E, not tapping it. PlayerInteractor fills a bar and calls
    /// RegisterHold once each time the hold finishes.
    /// </summary>
    public class HoldInteractable : MonoBehaviour
    {
        [Min(0.2f)]
        [SerializeField] float holdSeconds = 0.9f;

        public float HoldSeconds => holdSeconds;

        public int Completed { get; private set; }

        public bool AcceptingHolds { get; set; } = true;

        public event Action<int> HoldCompleted;

        public void RegisterHold()
        {
            if (!AcceptingHolds)
                return;

            Completed++;
            HoldCompleted?.Invoke(Completed);
        }
    }
}
