using System.Collections.Generic;
using System.IO;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GrayBoxMansion.Editor
{
    /// <summary>
    /// Builds the grey-box mansion from scratch.
    /// Menu: Tools > Gray Box Mansion > Build Mansion Scene.
    /// Running it again overwrites Assets/Scenes/Mansion.unity.
    /// </summary>
    public static partial class MansionSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/Mansion.unity";
        const string NavPath = "Assets/Scenes/MansionNavMesh.asset";
        const string MaterialFolder = "Assets/Materials";
        const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        const string VolumeProfilePath = "Assets/Settings/SampleSceneProfile.asset";
        const string AudioFolder = "Assets/Audio";

        const float T = 0.24f;
        const float West = -10f;
        const float East = 10f;
        const float South = -9f;
        const float North = 9f;
        const float XW = -4f;
        const float XE = 4f;
        const float ZFoyer = 2.6f;
        const float ZKitchen = -3.2f;
        const float FloorY = 3.6f;
        const float RoofY = 6.9f;
        const float Slab = 0.12f;
        const float DoorH = 2.4f;
        const float ArchH = 2.7f;
        const float HallZ = -1.5f;
        const float XBath = -7.2f;
        const float ZBathN = 4f;
        const float XStore = 7.2f;
        const float ZMaster = 2.6f;

        // Ramp climbs toward -Z. The top sits on the upstairs hall floor.
        const float StairBottomZ = 2.1f;
        const float StairTopZ = -2.85f;
        const float HoleSouth = -2.7f;
        const float HoleNorth = 2.25f;
        const float HoleX = 1.28f;

        static readonly List<Collider> DoorColliders = new List<Collider>();
        static MansionDoor FrontDoor;
        static Transform Structure;
        static Transform Doors;
        static Transform Props;
        static Transform Lights;

        [MenuItem("Tools/Gray Box Mansion/Build Mansion Scene")]
        public static void Build()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("Exit Play mode, then run Tools > Gray Box Mansion > Build Mansion Scene again.");
                return;
            }

            Shader lit = Shader.Find("Universal Render Pipeline/Lit");
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (lit == null || actions == null)
            {
                Debug.LogError("Could not build the mansion. Need the URP Lit shader and " + InputActionsPath + ".");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            DoorColliders.Clear();
            FrontDoor = null;

            EnsureFolder("Assets/Scenes");
            EnsureFolder(MaterialFolder);
            Palette palette = CreateMaterials(lit);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            ApplyDarkMood();

            Structure = Group("Structure");
            Doors = Group("Doors");
            Props = Group("Props");
            Lights = Group("Lights");

            BuildShell(palette);
            BuildStairs(palette);
            BuildDoors(palette);
            BuildDressing(palette);
            BuildLighting(palette);
            GameObject player = BuildPlayer(actions, palette);
            BuildInteractionUi(player, actions);
            BuildAudio();
            BakeNavMesh(player);

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            if (!saved)
            {
                Debug.LogError("The mansion was built in memory but could not be saved to " + ScenePath + ".");
                return;
            }

            EnsureSceneInBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            Selection.activeObject = sceneAsset;
            EditorGUIUtility.PingObject(sceneAsset);

            Debug.Log("Built " + ScenePath + " and added it to Build Settings. Press Play. You start in the foyer.");
            EditorUtility.DisplayDialog(
                "Mansion built",
                "Assets/Scenes/Mansion is open and listed in Build Settings.\n\nPress Play. You start in the foyer.\nWASD to walk, mouse to look, E to use, F for the flashlight.\n\nThe solution is in SETUP.md, under the spoiler heading.",
                "OK");
        }

        static void ApplyDarkMood()
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.015f, 0.014f, 0.016f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.subtractiveShadowColor = Color.black;
            RenderSettings.reflectionIntensity = 0.02f;
            RenderSettings.skybox = null;
            RenderSettings.sun = null;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(0.012f, 0.011f, 0.013f);
            RenderSettings.fogDensity = 0.035f;
        }

        static void BuildLighting(Palette palette)
        {
            // Short ranges so a room mesh is only touched by the lamps near it.
            // URP still caps how many extra lights one renderer can use.
            Lamp("FoyerLamp", new Vector3(0f, 2.7f, 7.4f), 1.7f, 7.5f, palette, true);
            Lamp("LivingLamp", new Vector3(-7.2f, 2.35f, 6.4f), 1.25f, 5.5f, palette, true);
            Lamp("DiningLamp", new Vector3(-7f, 2.2f, -0.2f), 0.85f, 4.8f, palette, false);
            Lamp("KitchenLamp", new Vector3(-7.4f, 2.45f, -6.4f), 1.55f, 5.2f, palette, false);
            Lamp("GalleryLamp", new Vector3(7.2f, 2.3f, -2f), 0.7f, 4.6f, palette, true);
            Lamp("BackHallLamp", new Vector3(0.2f, 2.2f, -6.6f), 0.4f, 4f, palette, true);
            Lamp("StairBottom", new Vector3(0f, 2.5f, 1.2f), 1.15f, 5.5f, palette, false);
            Lamp("StairTop", new Vector3(0f, FloorY + 1.6f, -3.6f), 1.25f, 6f, palette, false);
            Lamp("StudyLamp", new Vector3(7.4f, 2.15f, 7.2f), 1.15f, 4.8f, palette, true);
            Lamp("NurseryLamp", new Vector3(-8.6f, FloorY + 1.7f, 6.6f), 0.95f, 5.5f, palette, false);
            Lamp("BathLamp", new Vector3(-5.6f, FloorY + 2f, 1.2f), 0.75f, 4.2f, palette, false);
            Lamp("MasterLamp", new Vector3(7f, FloorY + 1.7f, 6.4f), 0.8f, 5.2f, palette, true);
            Lamp("HallWest", new Vector3(-6.5f, FloorY + 1.55f, -5.2f), 0.7f, 5f, palette, false);
            Lamp("HallEast", new Vector3(5.5f, FloorY + 1.55f, -5.2f), 0.5f, 4.4f, palette, true);
            Lamp("Balcony", new Vector3(-2.6f, FloorY + 1.5f, 0.6f), 0.35f, 3.2f, palette, true);
            Lamp("StorageGlimmer", new Vector3(8.7f, FloorY + 1.5f, 0.8f), 0.22f, 2.6f, palette, false);

            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(VolumeProfilePath);
            if (profile == null)
                return;

            GameObject volumeObject = new GameObject("Global Volume");
            volumeObject.transform.SetParent(Lights, false);
            Volume volume = volumeObject.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 0f;
            volume.sharedProfile = profile;
        }

        static Light Lamp(string name, Vector3 position, float intensity, float range, Palette palette, bool flicker)
        {
            Light light = CreateLight(Lights, name, position, new Color(1f, 0.78f, 0.48f), intensity, range, palette.bulb);
            if (flicker)
            {
                FlickerLamp wobble = light.gameObject.AddComponent<FlickerLamp>();
                SerializedObject serialized = new SerializedObject(wobble);
                serialized.FindProperty("strength").floatValue = 0.38f;
                serialized.FindProperty("speed").floatValue = Random.Range(6f, 10f);
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }

            return light;
        }

        static Light CreateLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range, Material bulbMaterial)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            Light light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.useColorTemperature = false;
            light.intensity = intensity;
            light.range = range;
            light.shadows = LightShadows.None;
            go.AddComponent<UniversalAdditionalLightData>();

            if (bulbMaterial != null)
            {
                GameObject glass = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                glass.name = name + "Bulb";
                glass.transform.SetParent(go.transform, false);
                glass.transform.localScale = Vector3.one * 0.14f;
                Object.DestroyImmediate(glass.GetComponent<Collider>());
                Renderer renderer = glass.GetComponent<Renderer>();
                renderer.sharedMaterial = bulbMaterial;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }

            return light;
        }

        static GameObject BuildPlayer(InputActionAsset actions, Palette palette)
        {
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(0f, 0.08f, 5.45f);
            player.transform.rotation = Quaternion.identity;

            CharacterController capsule = player.AddComponent<CharacterController>();
            FirstPersonController controller = player.AddComponent<FirstPersonController>();
            // Reset() on the controller overwrites the capsule when the component is added.
            capsule.height = 1.75f;
            capsule.radius = 0.28f;
            capsule.center = new Vector3(0f, 0.875f, 0f);
            capsule.stepOffset = 0.4f;
            capsule.slopeLimit = 55f;
            capsule.skinWidth = 0.04f;
            capsule.minMoveDistance = 0.001f;

            GameObject bodyMesh = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            bodyMesh.name = "Body";
            bodyMesh.transform.SetParent(player.transform, false);
            bodyMesh.transform.localPosition = new Vector3(0f, 0.875f, 0f);
            bodyMesh.transform.localScale = new Vector3(0.56f, 0.875f, 0.56f);
            Object.DestroyImmediate(bodyMesh.GetComponent<Collider>());
            bodyMesh.GetComponent<Renderer>().sharedMaterial = palette.fabric;
            bodyMesh.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(player.transform, false);
            cameraObject.transform.localPosition = new Vector3(0f, 1.55f, 0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.01f, 0.009f, 0.011f);
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 60f;
            camera.fieldOfView = 68f;
            cameraObject.AddComponent<AudioListener>();
            UniversalAdditionalCameraData cameraData = cameraObject.AddComponent<UniversalAdditionalCameraData>();
            cameraData.renderPostProcessing = true;

            SerializedObject controllerObject = new SerializedObject(controller);
            controllerObject.FindProperty("inputActions").objectReferenceValue = actions;
            controllerObject.FindProperty("viewCamera").objectReferenceValue = camera.transform;
            controllerObject.ApplyModifiedPropertiesWithoutUndo();

            player.AddComponent<Inventory>();
            player.AddComponent<InventoryHud>();

            Flashlight flashlight = player.AddComponent<Flashlight>();
            SerializedObject flashObject = new SerializedObject(flashlight);
            flashObject.FindProperty("bodyMaterial").objectReferenceValue = palette.metal;
            flashObject.FindProperty("lensMaterial").objectReferenceValue = palette.glow;
            flashObject.ApplyModifiedPropertiesWithoutUndo();

            return player;
        }

        static void BuildInteractionUi(GameObject player, InputActionAsset actions)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                Debug.LogError("LegacyRuntime.ttf was not found. On-screen prompts will have no letters.");

            GameObject canvasObject = new GameObject("InteractionUI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            GameObject crosshair = CreateCrosshair(canvasObject.transform);
            Text prompt = CreateLabel(canvasObject.transform, "Prompt", font, 22, new Vector2(0.5f, 0.5f), new Vector2(0f, -36f), new Vector2(640f, 40f));
            Text message = CreateLabel(canvasObject.transform, "Message", font, 26, new Vector2(0.5f, 0.2f), Vector2.zero, new Vector2(980f, 120f));
            prompt.gameObject.SetActive(false);
            message.gameObject.SetActive(false);

            PlayerInteractor interactor = player.AddComponent<PlayerInteractor>();
            Camera camera = player.GetComponentInChildren<Camera>();
            SerializedObject serialized = new SerializedObject(interactor);
            serialized.FindProperty("inputActions").objectReferenceValue = actions;
            serialized.FindProperty("viewCamera").objectReferenceValue = camera != null ? camera.transform : null;
            serialized.FindProperty("crosshair").objectReferenceValue = crosshair;
            serialized.FindProperty("promptLabel").objectReferenceValue = prompt;
            serialized.FindProperty("messageLabel").objectReferenceValue = message;
            serialized.FindProperty("interactDistance").floatValue = 3.5f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject CreateCrosshair(Transform parent)
        {
            GameObject go = new GameObject("Crosshair");
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(8f, 8f);
            rect.anchoredPosition = Vector2.zero;

            Image image = go.AddComponent<Image>();
            image.raycastTarget = false;
            image.color = new Color(1f, 0.95f, 0.85f, 0.85f);
            Sprite knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            if (knob != null)
                image.sprite = knob;
            return go;
        }

        static Text CreateLabel(Transform parent, string name, Font font, int size, Vector2 anchor, Vector2 offset, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = offset;

            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(0.95f, 0.9f, 0.78f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(1.4f, -1.4f);
            return text;
        }

        static void BuildAudio()
        {
            GameObject root = new GameObject("Audio");
            GameAudio audio = root.AddComponent<GameAudio>();
            SerializedObject serialized = new SerializedObject(audio);
            AssignClips(serialized, "footstepWood", "footstep_wood_01.ogg", "footstep_wood_02.ogg", "footstep_wood_03.ogg", "footstep_wood_04.ogg");
            AssignClip(serialized, "jump", "jump.ogg");
            AssignClip(serialized, "land", "land.ogg");
            AssignClip(serialized, "pickup", "pickup.ogg");
            AssignClip(serialized, "keyJingle", "key_jingle.ogg");
            AssignClip(serialized, "flashlightOn", "flashlight_on.ogg");
            AssignClip(serialized, "flashlightOff", "flashlight_off.ogg");
            AssignClips(serialized, "doorCreak", "closet_open_01.ogg", "closet_open_02.ogg", "creak_01.ogg", "creak_02.ogg");
            AssignClips(serialized, "lockUnlock", "lock_unlock_01.ogg", "lock_unlock_02.ogg");
            AssignClip(serialized, "lockRattle", "lock_rattle.wav");
            AssignClip(serialized, "slotFill", "slot_fill.wav");
            SerializedProperty master = serialized.FindProperty("masterVolume");
            if (master != null)
                master.floatValue = 0.85f;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            AudioSource bed = root.AddComponent<AudioSource>();
            bed.clip = Clip("ambient_mansion.wav");
            bed.loop = true;
            bed.playOnAwake = true;
            bed.volume = 0.14f;
            bed.spatialBlend = 0f;
            bed.dopplerLevel = 0f;
            bed.priority = 64;
        }

        static void BakeNavMesh(GameObject player)
        {
            GameObject navObject = new GameObject("NavMesh");
            NavMeshSurface surface = navObject.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.All;
            surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surface.overrideVoxelSize = true;
            surface.voxelSize = 0.14f;
            surface.minRegionArea = 0.25f;

            // The bake should describe the house, not the player capsule, and doorways
            // should stay connected. Closed doors carve the mesh at runtime instead.
            bool playerWasActive = player.activeSelf;
            player.SetActive(false);
            foreach (Collider doorCollider in DoorColliders)
            {
                if (doorCollider != null)
                    doorCollider.enabled = false;
            }

            surface.BuildNavMesh();

            foreach (Collider doorCollider in DoorColliders)
            {
                if (doorCollider != null)
                    doorCollider.enabled = true;
            }

            player.SetActive(playerWasActive);

            NavMeshData data = surface.navMeshData;
            if (data == null)
            {
                Debug.LogError("NavMesh bake produced no data. Floors are still walkable for the player.");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<Object>(NavPath) != null)
                AssetDatabase.DeleteAsset(NavPath);

            data.hideFlags = HideFlags.None;
            if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(data)))
                AssetDatabase.CreateAsset(data, NavPath);

            surface.navMeshData = data;
            EditorUtility.SetDirty(surface);
            Debug.Log("Baked NavMesh to " + NavPath + ".");
        }

        static MansionDoor BuildDoor(
            string name,
            Vector3 hinge,
            Vector3 closedDirection,
            float openSignedAngle,
            float width,
            float height,
            float floorY,
            ItemId key,
            bool sealedDoor,
            string prompt,
            string lockedMessage,
            string unlockMessage,
            Material panelMaterial,
            Material knobMaterial)
        {
            GameObject hingeObject = new GameObject(name);
            hingeObject.transform.SetParent(Doors, false);
            hinge.y = floorY + 0.04f;
            hingeObject.transform.position = hinge;

            // Local +X runs along the closed door. Local +Z is one swing direction.
            // A negative open angle swings the other way. Flipping the basis would
            // also flip which way the panel extends, so the sign lives on the angle.
            Vector3 right = closedDirection.normalized;
            Vector3 forward = Vector3.Cross(right, Vector3.up);
            if (forward.sqrMagnitude < 0.001f)
                forward = Vector3.forward;
            forward.Normalize();
            hingeObject.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            if (Vector3.Dot(hingeObject.transform.right, right) < 0.95f)
                Debug.LogError("Door hinge axis is wrong on " + name + ".");

            GameObject panel = Box(
                hingeObject.transform,
                "Panel",
                new Vector3(width * 0.5f, height * 0.5f, 0f),
                new Vector3(width, height, 0.07f),
                panelMaterial);
            Box(
                hingeObject.transform,
                "Knob",
                new Vector3(width - 0.1f, height * 0.48f, 0.07f),
                new Vector3(0.07f, 0.07f, 0.07f),
                knobMaterial,
                false);

            Collider panelCollider = panel.GetComponent<Collider>();
            DoorColliders.Add(panelCollider);

            NavMeshObstacle obstacle = panel.AddComponent<NavMeshObstacle>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = Vector3.zero;
            obstacle.size = Vector3.one;
            obstacle.carving = true;
            obstacle.carveOnlyStationary = true;

            MansionDoor door = hingeObject.AddComponent<MansionDoor>();
            SerializedObject serialized = new SerializedObject(door);
            serialized.FindProperty("requiredKey").intValue = (int)key;
            serialized.FindProperty("sealedUntilForced").boolValue = sealedDoor;
            serialized.FindProperty("openAngle").floatValue = openSignedAngle;
            serialized.FindProperty("lockedMessage").stringValue = lockedMessage;
            serialized.FindProperty("unlockMessage").stringValue = unlockMessage;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (!sealedDoor)
            {
                Interactable interactable = MakeInteractable(hingeObject, prompt, "");
                UnityEventTools.AddPersistentListener(interactable.OnInteract, door.TryToggle);
            }

            return door;
        }

        static void Listen(Interactable interactable, UnityAction call)
        {
            UnityEventTools.AddPersistentListener(interactable.OnInteract, call);
        }

        static Interactable MakeInteractable(GameObject target, string prompt, string message)
        {
            Interactable interactable = target.GetComponent<Interactable>();
            if (interactable == null)
                interactable = target.AddComponent<Interactable>();
            if (target.GetComponent<LookHighlight>() == null)
                target.AddComponent<LookHighlight>();

            SerializedObject serialized = new SerializedObject(interactable);
            serialized.FindProperty("prompt").stringValue = prompt;
            serialized.FindProperty("message").stringValue = message;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return interactable;
        }

        static void MakePickup(GameObject target, ItemId item, string prompt, string message)
        {
            Interactable interactable = MakeInteractable(target, prompt, message);
            ItemPickup pickup = target.AddComponent<ItemPickup>();
            SerializedObject serialized = new SerializedObject(pickup);
            serialized.FindProperty("item").intValue = (int)item;
            serialized.FindProperty("alreadyCarryingMessage").stringValue = "I already have one of these.";
            serialized.ApplyModifiedPropertiesWithoutUndo();
            Listen(interactable, pickup.Collect);
        }

        static void WireHold(GameObject target, Transform movingPart, Vector3 slide, GameObject reveal, string prompt, string stuckMessage, string openedMessage)
        {
            MakeInteractable(target, prompt, stuckMessage);
            HoldInteractable hold = target.AddComponent<HoldInteractable>();
            SerializedObject holdObject = new SerializedObject(hold);
            holdObject.FindProperty("holdSeconds").floatValue = 0.85f;
            holdObject.ApplyModifiedPropertiesWithoutUndo();

            HoldToOpen opener = target.AddComponent<HoldToOpen>();
            SerializedObject serialized = new SerializedObject(opener);
            serialized.FindProperty("movingPart").objectReferenceValue = movingPart;
            serialized.FindProperty("localSlide").vector3Value = slide;
            serialized.FindProperty("yawSwing").floatValue = 0f;
            serialized.FindProperty("reveal").objectReferenceValue = reveal;
            serialized.FindProperty("openedMessage").stringValue = openedMessage;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject Solid(Transform parent, string name, float x0, float x1, float y0, float y1, float z0, float z1, Material material, bool withCollider = true)
        {
            float xa = Mathf.Min(x0, x1);
            float xb = Mathf.Max(x0, x1);
            float ya = Mathf.Min(y0, y1);
            float yb = Mathf.Max(y0, y1);
            float za = Mathf.Min(z0, z1);
            float zb = Mathf.Max(z0, z1);
            return Box(
                parent,
                name,
                new Vector3((xa + xb) * 0.5f, (ya + yb) * 0.5f, (za + zb) * 0.5f),
                new Vector3(Mathf.Max(0.02f, xb - xa), Mathf.Max(0.02f, yb - ya), Mathf.Max(0.02f, zb - za)),
                material,
                withCollider);
        }

        static void WallAlongX(Transform parent, string name, float z, float x0, float x1, float y0, float y1, Material material, float gapA = float.NaN, float gapB = float.NaN, float gapTop = 0f)
        {
            Wall(parent, name, true, z, x0, x1, y0, y1, material, gapA, gapB, gapTop);
        }

        static void WallAlongZ(Transform parent, string name, float x, float z0, float z1, float y0, float y1, Material material, float gapA = float.NaN, float gapB = float.NaN, float gapTop = 0f)
        {
            Wall(parent, name, false, x, z0, z1, y0, y1, material, gapA, gapB, gapTop);
        }

        static void Wall(Transform parent, string name, bool alongX, float fixedAxis, float a0, float a1, float y0, float y1, Material material, float gapA, float gapB, float gapTop)
        {
            float half = T * 0.5f;
            bool gap = !float.IsNaN(gapA);
            if (!gap)
            {
                if (alongX)
                    Solid(parent, name, a0, a1, y0, y1, fixedAxis - half, fixedAxis + half, material);
                else
                    Solid(parent, name, fixedAxis - half, fixedAxis + half, y0, y1, a0, a1, material);
                return;
            }

            float gapMin = Mathf.Min(gapA, gapB);
            float gapMax = Mathf.Max(gapA, gapB);
            float start = Mathf.Min(a0, a1);
            float end = Mathf.Max(a0, a1);
            if (gapMin - start > 0.05f)
                PlaceSpan(parent, name + "A", alongX, fixedAxis, start, gapMin, y0, y1, material);
            if (end - gapMax > 0.05f)
                PlaceSpan(parent, name + "B", alongX, fixedAxis, gapMax, end, y0, y1, material);

            float top = Mathf.Max(y0, y1);
            float header = Mathf.Clamp(gapTop, Mathf.Min(y0, y1), top);
            if (top - header > 0.05f)
                PlaceSpan(parent, name + "Header", alongX, fixedAxis, gapMin, gapMax, header, top, material);
        }

        static void PlaceSpan(Transform parent, string name, bool alongX, float fixedAxis, float a, float b, float y0, float y1, Material material)
        {
            float half = T * 0.5f;
            if (alongX)
                Solid(parent, name, a, b, y0, y1, fixedAxis - half, fixedAxis + half, material);
            else
                Solid(parent, name, fixedAxis - half, fixedAxis + half, y0, y1, a, b, material);
        }

        static void Floor(Transform parent, string name, float x0, float x1, float z0, float z1, float top, Material material)
        {
            Solid(parent, name, x0, x1, top - Slab, top, z0, z1, material);
        }

        static Transform Group(string name)
        {
            return new GameObject(name).transform;
        }

        static GameObject Box(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool withCollider = true,
            Quaternion rotation = default)
        {
            return Primitive(PrimitiveType.Cube, parent, name, position, scale, material, withCollider, ValidRotation(rotation));
        }

        static GameObject Cylinder(
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            Quaternion rotation = default,
            bool withCollider = true)
        {
            return Primitive(PrimitiveType.Cylinder, parent, name, position, scale, material, withCollider, ValidRotation(rotation));
        }

        static Quaternion ValidRotation(Quaternion rotation)
        {
            if (rotation.x == 0f && rotation.y == 0f && rotation.z == 0f && rotation.w == 0f)
                return Quaternion.identity;
            return rotation;
        }

        static GameObject Primitive(
            PrimitiveType type,
            Transform parent,
            string name,
            Vector3 position,
            Vector3 scale,
            Material material,
            bool withCollider,
            Quaternion rotation)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!withCollider)
                Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string folderName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        static void EnsureSceneInBuildSettings(string scenePath)
        {
            EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(current.Length + 1);
            bool found = false;
            foreach (EditorBuildSettingsScene entry in current)
            {
                if (entry.path == scenePath)
                {
                    if (!found)
                        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                    found = true;
                    continue;
                }

                scenes.Add(entry);
            }

            if (!found)
                scenes.Insert(0, new EditorBuildSettingsScene(scenePath, true));

            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static void AssignClip(SerializedObject serialized, string propertyName, string fileName)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogError("GameAudio has no field named " + propertyName + ".");
                return;
            }

            property.objectReferenceValue = Clip(fileName);
        }

        static void AssignClips(SerializedObject serialized, string propertyName, params string[] fileNames)
        {
            SerializedProperty property = serialized.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                Debug.LogError("GameAudio has no clip array named " + propertyName + ".");
                return;
            }

            property.arraySize = fileNames.Length;
            for (int i = 0; i < fileNames.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = Clip(fileNames[i]);
        }

        static AudioClip Clip(string fileName)
        {
            string path = AudioFolder + "/" + fileName;
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (clip == null)
                Debug.LogError("Missing audio clip at " + path + ".");
            return clip;
        }

        static Palette CreateMaterials(Shader lit)
        {
            return new Palette
            {
                floorWood = SaveLit(lit, "MansionFloorWood.mat", new Color(0.16f, 0.11f, 0.08f), 0.18f, 0f),
                floorStone = SaveLit(lit, "MansionFloorStone.mat", new Color(0.14f, 0.13f, 0.12f), 0.08f, 0f),
                plaster = SaveLit(lit, "MansionPlaster.mat", new Color(0.22f, 0.2f, 0.18f), 0.04f, 0f),
                warm = SaveLit(lit, "MansionWarmWall.mat", new Color(0.26f, 0.16f, 0.13f), 0.04f, 0f),
                cool = SaveLit(lit, "MansionCoolWall.mat", new Color(0.16f, 0.18f, 0.2f), 0.05f, 0f),
                study = SaveLit(lit, "MansionStudyWall.mat", new Color(0.14f, 0.16f, 0.18f), 0.05f, 0f),
                dining = SaveLit(lit, "MansionDiningWall.mat", new Color(0.15f, 0.18f, 0.14f), 0.04f, 0f),
                nursery = SaveLit(lit, "MansionNurseryWall.mat", new Color(0.24f, 0.2f, 0.18f), 0.04f, 0f),
                master = SaveLit(lit, "MansionMasterWall.mat", new Color(0.22f, 0.12f, 0.13f), 0.05f, 0f),
                ceiling = SaveLit(lit, "MansionCeiling.mat", new Color(0.07f, 0.065f, 0.06f), 0.03f, 0f),
                wood = SaveLit(lit, "MansionWood.mat", new Color(0.28f, 0.17f, 0.09f), 0.16f, 0f),
                woodDark = SaveLit(lit, "MansionWoodDark.mat", new Color(0.14f, 0.09f, 0.06f), 0.14f, 0f),
                metal = SaveLit(lit, "MansionMetal.mat", new Color(0.12f, 0.12f, 0.13f), 0.55f, 0.8f),
                brass = SaveLit(lit, "MansionBrass.mat", new Color(0.55f, 0.4f, 0.16f), 0.55f, 0.85f),
                silver = SaveLit(lit, "MansionSilver.mat", new Color(0.72f, 0.73f, 0.75f), 0.6f, 0.9f),
                fabric = SaveLit(lit, "MansionFabric.mat", new Color(0.18f, 0.14f, 0.13f), 0.05f, 0f),
                fabricRed = SaveLit(lit, "MansionFabricRed.mat", new Color(0.28f, 0.08f, 0.08f), 0.05f, 0f),
                stone = SaveLit(lit, "MansionStone.mat", new Color(0.18f, 0.17f, 0.16f), 0.1f, 0f),
                bulb = SaveEmissive(lit, "MansionBulb.mat", new Color(1f, 0.82f, 0.5f), 3.5f),
                glow = SaveEmissive(lit, "MansionGlow.mat", new Color(1f, 0.7f, 0.32f), 0.8f),
                glass = SaveEmissive(lit, "MansionWindow.mat", new Color(0.05f, 0.07f, 0.1f), 0.15f),
                canvas = SaveLit(lit, "MansionCanvas.mat", new Color(0.1f, 0.09f, 0.08f), 0.04f, 0f),
                candle = SaveEmissive(lit, "MansionCandle.mat", new Color(1f, 0.6f, 0.2f), 1.4f),
                iron = SaveLit(lit, "MansionIron.mat", new Color(0.2f, 0.2f, 0.22f), 0.45f, 0.7f),
            };
        }

        static Material SaveLit(Shader shader, string fileName, Color color, float smoothness, float metallic)
        {
            string path = MaterialFolder + "/" + fileName;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            material.color = color;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            material.DisableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
            EditorUtility.SetDirty(material);
            return material;
        }

        static Material SaveEmissive(Shader shader, string fileName, Color color, float intensity)
        {
            Material material = SaveLit(shader, fileName, color, 0.35f, 0f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * intensity);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
            EditorUtility.SetDirty(material);
            return material;
        }

        internal struct Palette
        {
            public Material floorWood;
            public Material floorStone;
            public Material plaster;
            public Material warm;
            public Material cool;
            public Material study;
            public Material dining;
            public Material nursery;
            public Material master;
            public Material ceiling;
            public Material wood;
            public Material woodDark;
            public Material metal;
            public Material brass;
            public Material silver;
            public Material fabric;
            public Material fabricRed;
            public Material stone;
            public Material bulb;
            public Material glow;
            public Material glass;
            public Material canvas;
            public Material candle;
            public Material iron;
        }
    }
}
