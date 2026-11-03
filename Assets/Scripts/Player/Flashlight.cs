using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace GrayBoxMansion
{
    /// <summary>
    /// A carried flashlight. It does nothing until Give is called.
    /// F (or the gamepad right bumper) toggles the beam. The beam stays on
    /// when another hotbar item is selected. Only the little model hides.
    /// </summary>
    public class Flashlight : MonoBehaviour
    {
        [Header("Beam")]
        [Min(1f)]
        [SerializeField] float range = 18f;

        [Range(10f, 120f)]
        [SerializeField] float spotAngle = 52f;

        [Range(0f, 120f)]
        [SerializeField] float innerSpotAngle = 28f;

        [SerializeField] Color beamColor = new Color(1f, 0.93f, 0.8f);

        [Min(0f)]
        [SerializeField] float intensity = 14f;

        [SerializeField] bool castShadows = true;

        [Header("Look")]
        [SerializeField] Material bodyMaterial;
        [SerializeField] Material lensMaterial;

        Transform viewCamera;
        Light spot;
        GameObject heldModel;
        Material runtimeBodyMaterial;
        Material runtimeLensMaterial;
        bool owned;
        bool turnedOn;

        public bool HasFlashlight => owned;

        public bool IsOn => owned && turnedOn;

        bool IsSelected()
        {
            return owned && Inventory.Instance != null && Inventory.Instance.Selected == ItemId.Flashlight;
        }

        public void Give()
        {
            owned = true;
            turnedOn = false;
            ApplyBeamSettings();
            RefreshVisuals();
        }

        void Awake()
        {
            EnsureRig();
            ApplyBeamSettings();
            RefreshVisuals();
        }

        void Update()
        {
            if (PlayerControl.Suspended || !owned || spot == null)
                return;

            if (WasTogglePressed())
            {
                turnedOn = !turnedOn;
                if (GameAudio.Instance != null)
                    GameAudio.Instance.PlayFlashlight(turnedOn);
            }

            RefreshVisuals();
        }

        void OnValidate()
        {
            range = Mathf.Max(1f, range);
            spotAngle = Mathf.Clamp(spotAngle, 10f, 120f);
            innerSpotAngle = Mathf.Clamp(innerSpotAngle, 0f, spotAngle);
            intensity = Mathf.Max(0f, intensity);

            if (Application.isPlaying && spot != null)
                ApplyBeamSettings();
        }

        bool WasTogglePressed()
        {
            if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                return true;

            return Gamepad.current != null && Gamepad.current.rightShoulder.wasPressedThisFrame;
        }

        void EnsureRig()
        {
            if (viewCamera == null)
            {
                Camera childCamera = GetComponentInChildren<Camera>();
                if (childCamera != null)
                    viewCamera = childCamera.transform;
            }

            if (viewCamera == null)
            {
                Debug.LogError("Flashlight on " + name + " could not find a child camera.", this);
                return;
            }

            if (spot == null)
            {
                GameObject beamObject = new GameObject("FlashlightBeam");
                beamObject.transform.SetParent(viewCamera, false);
                beamObject.transform.localPosition = new Vector3(0.16f, -0.12f, 0.55f);
                beamObject.transform.localRotation = Quaternion.identity;

                spot = beamObject.AddComponent<Light>();
                spot.enabled = false;
                beamObject.AddComponent<UniversalAdditionalLightData>();
            }

            if (heldModel == null)
            {
                heldModel = new GameObject("HeldFlashlight");
                heldModel.transform.SetParent(viewCamera, false);
                heldModel.transform.localPosition = new Vector3(0.22f, -0.2f, 0.42f);
                heldModel.transform.localRotation = Quaternion.identity;
                heldModel.SetActive(false);

                CreatePiece(heldModel.transform, "Body", new Vector3(0f, 0f, -0.01f), new Vector3(0.045f, 0.08f, 0.045f));
                CreatePiece(heldModel.transform, "Lens", new Vector3(0f, 0f, 0.08f), new Vector3(0.055f, 0.014f, 0.055f));
            }

            ApplyLookMaterials();
        }

        static void CreatePiece(Transform parent, string pieceName, Vector3 localPosition, Vector3 localScale)
        {
            GameObject piece = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            piece.name = pieceName;
            piece.transform.SetParent(parent, false);
            piece.transform.localPosition = localPosition;
            piece.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            piece.transform.localScale = localScale;

            Collider pieceCollider = piece.GetComponent<Collider>();
            if (pieceCollider != null)
                Destroy(pieceCollider);

            piece.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        void ApplyLookMaterials()
        {
            if (heldModel == null)
                return;

            Transform bodyPiece = heldModel.transform.Find("Body");
            Transform lens = heldModel.transform.Find("Lens");
            Material bodyMat = BodyMaterial();
            Material lensMat = LensMaterial();
            if (bodyPiece != null && bodyMat != null)
                bodyPiece.GetComponent<Renderer>().sharedMaterial = bodyMat;
            if (lens != null && lensMat != null)
                lens.GetComponent<Renderer>().sharedMaterial = lensMat;
        }

        Material BodyMaterial()
        {
            if (bodyMaterial != null)
                return bodyMaterial;

            if (runtimeBodyMaterial == null)
                runtimeBodyMaterial = FallbackMaterial(new Color(0.12f, 0.12f, 0.13f), 0f);
            return runtimeBodyMaterial;
        }

        Material LensMaterial()
        {
            if (lensMaterial != null)
                return lensMaterial;

            if (runtimeLensMaterial == null)
                runtimeLensMaterial = FallbackMaterial(new Color(1f, 0.75f, 0.4f), 0.6f);
            return runtimeLensMaterial;
        }

        static Material FallbackMaterial(Color color, float emission)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
                return null;

            Material material = new Material(shader);
            material.color = color;
            material.SetColor("_BaseColor", color);
            if (emission > 0f)
            {
                material.EnableKeyword("_EMISSION");
                material.SetColor("_EmissionColor", color * emission);
            }

            return material;
        }

        void ApplyBeamSettings()
        {
            if (spot == null)
                return;

            spot.type = LightType.Spot;
            spot.color = beamColor;
            spot.useColorTemperature = false;
            spot.range = range;
            spot.spotAngle = spotAngle;
            spot.innerSpotAngle = Mathf.Min(innerSpotAngle, spotAngle);
            spot.intensity = intensity;
            spot.shadows = castShadows ? LightShadows.Soft : LightShadows.None;
            spot.shadowStrength = 0.85f;
        }

        void RefreshVisuals()
        {
            if (heldModel != null)
                heldModel.SetActive(IsSelected());

            if (spot != null)
            {
                spot.enabled = turnedOn;
                if (spot.enabled)
                    spot.intensity = intensity;
            }
        }
    }
}
