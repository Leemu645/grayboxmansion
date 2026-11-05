using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GrayBoxMansion
{
    /// <summary>
    /// Brief shake, then Game Over. Try Again reloads the mansion, which resets
    /// the relic puzzle, the inventory, and the creature.
    /// </summary>
    public class GameOverSequence : MonoBehaviour
    {
        bool showing;
        bool menuReady;

        public void Begin()
        {
            if (showing)
                return;

            showing = true;
            PlayerControl.Suspend(false);
            if (GameAudio.Instance != null)
                GameAudio.Instance.PlayScare();
            StartCoroutine(ScareThenMenu());
        }

        IEnumerator ScareThenMenu()
        {
            Camera view = Camera.main;
            Transform cam = view != null ? view.transform : null;
            Vector3 rest = cam != null ? cam.localPosition : Vector3.zero;
            float elapsed = 0f;
            const float scareSeconds = 0.7f;
            while (elapsed < scareSeconds)
            {
                elapsed += Time.deltaTime;
                if (cam != null)
                {
                    float fade = 1f - (elapsed / scareSeconds);
                    cam.localPosition = rest + Random.insideUnitSphere * 0.07f * fade;
                }

                yield return null;
            }

            if (cam != null)
                cam.localPosition = rest;

            PlayerControl.Suspend(true);
            BuildOverlay();
            menuReady = true;
        }

        void Update()
        {
            if (!menuReady)
                return;

            bool again = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            if (again)
                TryAgain();
        }

        public void TryAgain()
        {
            PlayerControl.ResetForSceneLoad();
            AudioListener.volume = 1f;
            SceneManager.LoadScene("Mansion");
        }

        void BuildOverlay()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject systemObject = new GameObject("EventSystem");
                systemObject.AddComponent<EventSystem>();
                systemObject.AddComponent<InputSystemUIInputModule>();
            }

            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Sprite sprite = WhiteSprite();

            GameObject canvasObject = new GameObject("GameOverUI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 60;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject shadeObject = new GameObject("Shade");
            shadeObject.transform.SetParent(canvasObject.transform, false);
            Image shade = shadeObject.AddComponent<Image>();
            shade.sprite = sprite;
            shade.color = new Color(0.18f, 0.02f, 0.02f, 0.55f);
            shade.raycastTarget = true;
            RectTransform shadeRect = shade.rectTransform;
            shadeRect.anchorMin = Vector2.zero;
            shadeRect.anchorMax = Vector2.one;
            shadeRect.offsetMin = Vector2.zero;
            shadeRect.offsetMax = Vector2.zero;

            Text title = MakeText(canvasObject.transform, font, "Game Over", 72, new Vector2(0.5f, 0.58f), new Vector2(900f, 120f));
            title.color = new Color(0.95f, 0.82f, 0.78f, 1f);

            GameObject buttonObject = new GameObject("TryAgain");
            buttonObject.transform.SetParent(canvasObject.transform, false);
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.sprite = sprite;
            buttonImage.color = new Color(0.38f, 0.12f, 0.1f, 0.95f);
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(TryAgain);
            RectTransform buttonRect = buttonImage.rectTransform;
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
            buttonRect.sizeDelta = new Vector2(280f, 64f);

            Text buttonLabel = MakeText(buttonObject.transform, font, "Try Again", 28, new Vector2(0.5f, 0.5f), new Vector2(260f, 56f));
            buttonLabel.rectTransform.anchorMin = Vector2.zero;
            buttonLabel.rectTransform.anchorMax = Vector2.one;
            buttonLabel.rectTransform.offsetMin = Vector2.zero;
            buttonLabel.rectTransform.offsetMax = Vector2.zero;
            buttonLabel.color = new Color(1f, 0.94f, 0.9f, 1f);
        }

        static Text MakeText(Transform parent, Font font, string value, int size, Vector2 anchor, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(value);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = Vector2.zero;

            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.95f, 0.9f, 0.78f, 1f);
            text.raycastTarget = false;
            text.text = value;
            return text;
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
