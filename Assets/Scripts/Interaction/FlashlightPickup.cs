using UnityEngine;

namespace GrayBoxMansion
{
    /// <summary>
    /// A flashlight lying in the world. Collect gives it to the player and hides this object.
    /// </summary>
    public class FlashlightPickup : MonoBehaviour
    {
        public void Collect()
        {
            Flashlight flashlight = FindAnyObjectByType<Flashlight>();
            if (flashlight == null)
            {
                Debug.LogError("Nothing in the scene has a Flashlight component.", this);
                return;
            }

            flashlight.Give();
            if (Inventory.Instance != null)
                Inventory.Instance.Add(ItemId.Flashlight);
            if (GameAudio.Instance != null)
                GameAudio.Instance.PlayPickup();
            gameObject.SetActive(false);
        }
    }
}
