using UnityEngine;

namespace GrayBoxMansion
{
    /// <summary>
    /// Brightens this object's renderers while the player is looking at it.
    /// </summary>
    public class LookHighlight : MonoBehaviour
    {
        static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        static readonly int ColorId = Shader.PropertyToID("_Color");
        static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        Renderer[] renderers;
        Material[] sharedMaterials;
        Material[] instances;
        bool highlighted;

        void Awake()
        {
            CacheRenderers();
        }

        void CacheRenderers()
        {
            renderers = GetComponentsInChildren<Renderer>(true);
            sharedMaterials = new Material[renderers.Length];
            instances = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
                sharedMaterials[i] = renderers[i] != null ? renderers[i].sharedMaterial : null;
        }

        public void SetHighlighted(bool on)
        {
            if (renderers == null)
                CacheRenderers();

            if (on == highlighted || renderers == null)
                return;

            highlighted = on;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                    continue;

                if (!on)
                {
                    renderer.sharedMaterial = sharedMaterials[i];
                    if (instances[i] != null)
                    {
                        Destroy(instances[i]);
                        instances[i] = null;
                    }

                    continue;
                }

                Material copy = new Material(renderer.sharedMaterial);
                instances[i] = copy;
                Color warm = new Color(0.72f, 0.48f, 0.24f);
                if (copy.HasProperty(BaseColorId))
                    copy.SetColor(BaseColorId, warm);
                if (copy.HasProperty(ColorId))
                    copy.SetColor(ColorId, warm);
                copy.EnableKeyword("_EMISSION");
                if (copy.HasProperty(EmissionId))
                    copy.SetColor(EmissionId, new Color(0.45f, 0.24f, 0.08f));
                renderer.sharedMaterial = copy;
            }
        }

        void OnDestroy()
        {
            if (instances == null)
                return;

            for (int i = 0; i < instances.Length; i++)
            {
                if (instances[i] != null)
                    Destroy(instances[i]);
            }
        }
    }
}
