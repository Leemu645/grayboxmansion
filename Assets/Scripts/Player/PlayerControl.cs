using UnityEngine;

namespace GrayBoxMansion
{
    /// <summary>
    /// Freezes look and walking while the escape screen needs the mouse.
    /// Static flags survive a scene reload, so the player resets them on load.
    /// </summary>
    public static class PlayerControl
    {
        public static bool Suspended { get; private set; }

        public static void Suspend(bool freeCursor = true)
        {
            Suspended = true;
            if (!freeCursor)
                return;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public static void ResetForSceneLoad()
        {
            Suspended = false;
        }

        public static void Resume()
        {
            Suspended = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
