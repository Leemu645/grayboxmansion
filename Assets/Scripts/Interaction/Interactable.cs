using UnityEngine;
using UnityEngine.Events;

namespace GrayBoxMansion
{
    /// <summary>
    /// Something the player can look at and use.
    /// Put this on the object that has the collider, or on a parent of that collider.
    /// </summary>
    public class Interactable : MonoBehaviour
    {
        [Tooltip("Short name shown in the prompt, after \"E:\".")]
        [SerializeField] string prompt = "Interact";

        [Tooltip("Sentence shown on screen when the player interacts. Leave empty if another script writes the line.")]
        [TextArea(2, 4)]
        [SerializeField] string message = "";

        [SerializeField] UnityEvent onInteract;

        [SerializeField] bool showPrompt = true;

        public string Prompt => string.IsNullOrEmpty(prompt) ? "Interact" : prompt;

        public bool ShowsPrompt => showPrompt && !string.IsNullOrEmpty(prompt);

        public string Message => message;

        public UnityEvent OnInteract => onInteract;

        public void Interact()
        {
            onInteract?.Invoke();
        }
    }
}
