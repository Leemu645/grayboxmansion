using UnityEditor;
using UnityEngine;

namespace GrayBoxMansion.Editor
{
    /// <summary>
    /// Rooms, stairs, doors, and the relic hunt.
    ///
    /// Ground, north at the top:
    ///   living | foyer (front door) | study (locked)
    ///   dining | stair hall         | gallery
    ///   kitchen | back hall         | gallery
    ///
    /// Upstairs:
    ///   nursery + bathroom | stair well and balcony | master (locked) + storage
    ///   a hallway runs the full width along the south.
    /// </summary>
    public static partial class MansionSceneBuilder
    {
        static void BuildShell(Palette palette)
        {
            // Ground floors. Split so each mesh only sits near a few lamps.
            Floor(Structure, "FloorLiving", West, XW, ZFoyer, North, 0f, palette.floorWood);
            Floor(Structure, "FloorFoyer", XW, XE, ZFoyer, North, 0f, palette.floorStone);
            Floor(Structure, "FloorStudy", XE, East, ZFoyer, North, 0f, palette.floorWood);
            Floor(Structure, "FloorDining", West, XW, ZKitchen, ZFoyer, 0f, palette.floorWood);
            Floor(Structure, "FloorKitchen", West, XW, South, ZKitchen, 0f, palette.floorStone);
            Floor(Structure, "FloorCenter", XW, XE, South, ZFoyer, 0f, palette.floorWood);
            Floor(Structure, "FloorGallery", XE, East, South, ZFoyer, 0f, palette.floorWood);

            // Upstairs. The foyer is open to the roof. The stair hole is open too.
            Floor(Structure, "UpperWest", West, XW, South, North, FloorY, palette.floorWood);
            Floor(Structure, "UpperEast", XE, East, South, North, FloorY, palette.floorWood);
            Floor(Structure, "UpperHall", XW, XE, South, HoleSouth, FloorY, palette.floorWood);
            Floor(Structure, "BalconyWest", XW, -HoleX, HoleSouth - 0.15f, 2.4f, FloorY, palette.floorWood);
            Floor(Structure, "BalconyEast", HoleX, XE, HoleSouth - 0.15f, 2.4f, FloorY, palette.floorWood);

            Solid(Structure, "RoofWest", West, XW, RoofY, RoofY + Slab, South, North, palette.ceiling);
            Solid(Structure, "RoofCenter", XW, XE, RoofY, RoofY + Slab, South, North, palette.ceiling);
            Solid(Structure, "RoofEast", XE, East, RoofY, RoofY + Slab, South, North, palette.ceiling);

            // Outer shell. The front door is the only hole in the north wall.
            WallAlongX(Structure, "WallNorth", North, West, East, 0f, RoofY, palette.plaster, -0.72f, 0.72f, DoorH);
            WallAlongX(Structure, "WallSouth", South, West, East, 0f, RoofY, palette.plaster);
            WallAlongZ(Structure, "WallWest", West, South, North, 0f, RoofY, palette.plaster);
            WallAlongZ(Structure, "WallEast", East, South, North, 0f, RoofY, palette.plaster);

            // Ground dividers.
            WallAlongZ(Structure, "LivingFoyer", XW, ZFoyer, North, 0f, FloorY, palette.warm, 5.05f, 7.25f, ArchH);
            WallAlongZ(Structure, "StudyFoyer", XE, ZFoyer, North, 0f, FloorY, palette.study, 5.2f, 6.6f, DoorH);
            WallAlongX(Structure, "LivingDining", ZFoyer, West, XW, 0f, FloorY, palette.warm, -7.9f, -5.7f, ArchH);
            WallAlongX(Structure, "DiningKitchen", ZKitchen, West, XW, 0f, FloorY, palette.dining, -8.05f, -6.6f, DoorH);
            WallAlongX(Structure, "StudySouth", ZFoyer, XE, East, 0f, FloorY, palette.study);

            // West wing against the stair hall: kitchen arch, then the dining door.
            WallAlongZ(Structure, "WestCenterA", XW, South, -7.7f, 0f, FloorY, palette.plaster);
            WallAlongZ(Structure, "WestCenterB", XW, -5.7f, -0.15f, 0f, FloorY, palette.plaster);
            WallAlongZ(Structure, "WestCenterC", XW, 1.35f, ZFoyer, 0f, FloorY, palette.plaster);

            // East wing against the center: two arches, solid up to the study.
            WallAlongZ(Structure, "EastCenterA", XE, South, -7.5f, 0f, FloorY, palette.plaster);
            WallAlongZ(Structure, "EastCenterB", XE, -5.4f, 0.05f, 0f, FloorY, palette.plaster);
            WallAlongZ(Structure, "EastCenterC", XE, 1.85f, ZFoyer, 0f, FloorY, palette.plaster);

            // Upstairs rooms.
            WallAlongZ(Structure, "UpperWestVoid", XW, HallZ, North, FloorY, RoofY, palette.nursery);
            WallAlongZ(Structure, "MasterVoid", XE, ZMaster, North, FloorY, RoofY, palette.master);
            WallAlongZ(Structure, "NurseryBath", XBath, HallZ, ZBathN, FloorY, RoofY, palette.nursery);
            WallAlongX(Structure, "BathNorth", ZBathN, XBath, XW, FloorY, RoofY, palette.cool);
            WallAlongZ(Structure, "StorageWest", XStore, HallZ, ZMaster, FloorY, RoofY, palette.woodDark);
            WallAlongX(Structure, "MasterSouth", ZMaster, XE, East, FloorY, RoofY, palette.master, 4.75f, 6.15f, FloorY + DoorH);

            WallAlongX(Structure, "NurseryDoorWall", HallZ, West, XBath, FloorY, RoofY, palette.nursery, -9.3f, -7.9f, FloorY + DoorH);
            WallAlongX(Structure, "BathDoorWall", HallZ, XBath, XW, FloorY, RoofY, palette.cool, -6.6f, -5.2f, FloorY + DoorH);
            WallAlongX(Structure, "VestibuleMouth", HallZ, XE, XStore, FloorY, RoofY, palette.plaster, 4.35f, 6.9f, FloorY + ArchH);
            WallAlongX(Structure, "StorageDoorWall", HallZ, XStore, East, FloorY, RoofY, palette.woodDark, 7.9f, 9.3f, FloorY + DoorH);

            // Night windows. No colliders, so they cannot snag the player.
            Solid(Structure, "WindowLiving", -8.2f, -6.4f, 1.4f, 2.5f, North - 0.08f, North - 0.02f, palette.glass, false);
            Solid(Structure, "WindowMaster", 6.2f, 8.2f, FloorY + 1.3f, FloorY + 2.5f, North - 0.08f, North - 0.02f, palette.glass, false);
            Solid(Structure, "WindowNursery", West + 0.02f, West + 0.08f, FloorY + 1.4f, FloorY + 2.4f, 6.2f, 7.6f, palette.glass, false);
        }

        static void BuildStairs(Palette palette)
        {
            float rise = FloorY + 0.02f - (-0.04f);
            float run = StairBottomZ - StairTopZ;
            float angle = Mathf.Atan2(rise, run) * Mathf.Rad2Deg;
            if (angle >= 44f)
                Debug.LogError("Stair slope is " + angle.ToString("0.0") + " degrees. The player will not climb it.");
            else
                Debug.Log("Stair slope is " + angle.ToString("0.0") + " degrees.");

            // Positive X rotation lifts the -Z end, which is the top of this flight.
            Quaternion rampRotation = Quaternion.Euler(angle, 0f, 0f);
            Vector3 low = new Vector3(0f, -0.04f, StairBottomZ);
            Vector3 high = new Vector3(0f, FloorY + 0.02f, StairTopZ);
            Vector3 mid = (low + high) * 0.5f;
            Vector3 normal = rampRotation * Vector3.up;
            const float thick = 0.18f;
            Vector3 center = mid - normal * (thick * 0.5f);
            float length = Mathf.Sqrt(rise * rise + run * run);
            GameObject ramp = Box(
                Structure,
                "StairRamp",
                center,
                new Vector3(1.9f, thick, length),
                palette.woodDark,
                true,
                rampRotation);
            ramp.GetComponent<MeshRenderer>().enabled = false;

            int steps = 14;
            for (int i = 0; i < steps; i++)
            {
                float t1 = (i + 1) / (float)steps;
                float midT = (i + 0.5f) / steps;
                float yTop = Mathf.Lerp(0.02f, FloorY, t1);
                float z = Mathf.Lerp(StairBottomZ, StairTopZ, midT);
                float depth = run / steps + 0.03f;
                Solid(
                    Structure,
                    "Step" + (i + 1),
                    -0.95f,
                    0.95f,
                    yTop - 0.07f,
                    yTop + 0.01f,
                    z - depth * 0.5f,
                    z + depth * 0.5f,
                    palette.wood,
                    false);
            }

            // Sides stop the player sliding off. They end at the upstairs floor.
            Solid(Structure, "StairWallWest", -1.52f, -1.28f, 0f, FloorY - 0.16f, StairTopZ, StairBottomZ, palette.woodDark);
            Solid(Structure, "StairWallEast", 1.28f, 1.52f, 0f, FloorY - 0.16f, StairTopZ, StairBottomZ, palette.woodDark);
            // Closes the wedge under the top of the flight so nobody gets stuck in it.
            Solid(Structure, "UnderStair", -1.15f, 1.15f, 0f, 3.2f, HoleSouth + 0.02f, HoleSouth + 0.2f, palette.woodDark);

            Box(Structure, "NewelBL", new Vector3(-1.15f, 0.45f, StairBottomZ + 0.05f), new Vector3(0.12f, 0.9f, 0.12f), palette.wood);
            Box(Structure, "NewelBR", new Vector3(1.15f, 0.45f, StairBottomZ + 0.05f), new Vector3(0.12f, 0.9f, 0.12f), palette.wood);
            Box(Structure, "NewelTL", new Vector3(-1.15f, FloorY + 0.55f, HoleSouth - 0.05f), new Vector3(0.12f, 1.1f, 0.12f), palette.wood);
            Box(Structure, "NewelTR", new Vector3(1.15f, FloorY + 0.55f, HoleSouth - 0.05f), new Vector3(0.12f, 1.1f, 0.12f), palette.wood);

            // Upstairs railings around the open well and the balcony edge.
            Solid(Structure, "RailWest", -1.5f, -1.26f, FloorY, FloorY + 1.15f, HoleSouth, HoleNorth, palette.wood);
            Solid(Structure, "RailEast", 1.26f, 1.5f, FloorY, FloorY + 1.15f, HoleSouth, HoleNorth, palette.wood);
            Solid(Structure, "RailNorthWest", XW, -HoleX, FloorY, FloorY + 1.15f, 2.28f, 2.48f, palette.wood);
            Solid(Structure, "RailNorthEast", HoleX, XE, FloorY, FloorY + 1.15f, 2.28f, 2.48f, palette.wood);

            GameObject banister = Solid(Structure, "Banister", -2.6f, -1.7f, FloorY + 0.9f, FloorY + 1.15f, 1.6f, 1.85f, palette.wood);
            MakeInteractable(banister, "Banister", "The wood is worn smooth, right where a hand would rest.");
        }

        static void BuildDoors(Palette palette)
        {
            // Front door. The relic lock is parented to the hinge so it swings clear.
            FrontDoor = BuildDoor(
                "FrontDoor",
                new Vector3(-0.68f, 0f, North),
                Vector3.right,
                -100f,
                1.28f,
                DoorH - 0.06f,
                0f,
                ItemId.None,
                true,
                "Door",
                "It's shut.",
                "",
                palette.woodDark,
                palette.brass);

            BuildRelicLock(palette);

            BuildDoor(
                "StudyDoor",
                new Vector3(XE, 0f, 5.24f),
                Vector3.forward,
                -100f,
                1.26f,
                DoorH - 0.06f,
                0f,
                ItemId.StudyKey,
                false,
                "Door",
                "It's locked.",
                "The key turns. The study is open.",
                palette.woodDark,
                palette.brass);

            BuildDoor(
                "KitchenDoor",
                new Vector3(-7.98f, 0f, ZKitchen),
                Vector3.right,
                -100f,
                1.28f,
                DoorH - 0.06f,
                0f,
                ItemId.None,
                false,
                "Door",
                "",
                "",
                palette.wood,
                palette.brass);

            BuildDoor(
                "DiningHallDoor",
                new Vector3(XW, 0f, -0.1f),
                Vector3.forward,
                100f,
                1.28f,
                DoorH - 0.06f,
                0f,
                ItemId.None,
                false,
                "Door",
                "",
                "",
                palette.wood,
                palette.brass);

            BuildDoor(
                "NurseryDoor",
                new Vector3(-9.24f, 0f, HallZ),
                Vector3.right,
                100f,
                1.26f,
                DoorH - 0.06f,
                FloorY,
                ItemId.None,
                false,
                "Door",
                "",
                "",
                palette.wood,
                palette.brass);

            BuildDoor(
                "BathroomDoor",
                new Vector3(-6.54f, 0f, HallZ),
                Vector3.right,
                100f,
                1.26f,
                DoorH - 0.06f,
                FloorY,
                ItemId.None,
                false,
                "Door",
                "",
                "",
                palette.wood,
                palette.brass);

            BuildDoor(
                "StorageDoor",
                new Vector3(7.96f, 0f, HallZ),
                Vector3.right,
                100f,
                1.26f,
                DoorH - 0.06f,
                FloorY,
                ItemId.None,
                false,
                "Door",
                "",
                "",
                palette.woodDark,
                palette.iron);

            BuildDoor(
                "MasterDoor",
                new Vector3(4.8f, 0f, ZMaster),
                Vector3.right,
                100f,
                1.26f,
                DoorH - 0.06f,
                FloorY,
                ItemId.MasterKey,
                false,
                "Door",
                "It's locked.",
                "The key turns. The bedroom is open.",
                palette.woodDark,
                palette.brass);
        }

        static void BuildRelicLock(Palette palette)
        {
            Transform hinge = FrontDoor.transform;
            GameObject plate = new GameObject("RelicLock");
            plate.transform.SetParent(hinge, false);
            // Local -Z faces the foyer. The rig itself is unscaled so the slots stay a readable size.
            plate.transform.localPosition = new Vector3(0.66f, 1.32f, -0.16f);
            plate.transform.localRotation = Quaternion.identity;
            Box(plate.transform, "Plate", Vector3.zero, new Vector3(0.86f, 0.42f, 0.06f), palette.brass, false);
            BoxCollider plateCollider = plate.AddComponent<BoxCollider>();
            plateCollider.size = new Vector3(0.92f, 0.5f, 0.18f);

            GameObject glowObject = new GameObject("LockGlow");
            glowObject.transform.SetParent(plate.transform, false);
            glowObject.transform.localPosition = new Vector3(0f, 0f, -0.12f);
            Light glow = glowObject.AddComponent<Light>();
            glow.type = LightType.Point;
            glow.color = new Color(1f, 0.72f, 0.4f);
            glow.useColorTemperature = false;
            glow.intensity = 0.18f;
            glow.range = 1.8f;
            glow.shadows = LightShadows.None;
            glowObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();

            ItemId[] relics = { ItemId.SilverLocket, ItemId.MusicBox, ItemId.CrackedPortrait, ItemId.Candelabra };
            Material[] fills =
            {
                palette.silver,
                palette.wood,
                palette.canvas,
                palette.candle,
            };
            GameObject[] visuals = new GameObject[4];
            for (int i = 0; i < 4; i++)
            {
                float x = -0.27f + i * 0.18f;
                Box(plate.transform, "Slot" + i, new Vector3(x, 0f, -0.02f), new Vector3(0.12f, 0.16f, 0.03f), palette.iron);
                GameObject gem = Box(plate.transform, "Filled" + i, new Vector3(x, 0f, -0.05f), new Vector3(0.08f, 0.11f, 0.04f), fills[i], false);
                gem.SetActive(false);
                visuals[i] = gem;
            }

            GameObject endingObject = new GameObject("Escape");
            EscapeSequence ending = endingObject.AddComponent<EscapeSequence>();

            RelicLock relicLock = plate.AddComponent<RelicLock>();
            SerializedObject serialized = new SerializedObject(relicLock);
            serialized.FindProperty("frontDoor").objectReferenceValue = FrontDoor;
            serialized.FindProperty("ending").objectReferenceValue = ending;
            serialized.FindProperty("glow").objectReferenceValue = glow;
            SerializedProperty slots = serialized.FindProperty("slots");
            slots.arraySize = 4;
            for (int i = 0; i < 4; i++)
            {
                SerializedProperty slot = slots.GetArrayElementAtIndex(i);
                slot.FindPropertyRelative("relic").intValue = (int)relics[i];
                slot.FindPropertyRelative("filledVisual").objectReferenceValue = visuals[i];
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();

            Interactable interactable = MakeInteractable(plate, "Lock", "");
            UnityEditor.Events.UnityEventTools.AddPersistentListener(interactable.OnInteract, relicLock.TryInsert);
        }

        static void BuildDressing(Palette palette)
        {
            BuildFoyer(palette);
            BuildLiving(palette);
            BuildDining(palette);
            BuildKitchen(palette);
            BuildStudy(palette);
            BuildGallery(palette);
            BuildUpper(palette);
        }

        static void BuildFoyer(Palette palette)
        {
            Transform console = Prop("Console", new Vector3(2.35f, 0f, 7.55f));
            Box(console, "Top", new Vector3(0f, 0.78f, 0f), new Vector3(1.3f, 0.08f, 0.46f), palette.wood);
            Box(console, "Leg", new Vector3(-0.55f, 0.38f, 0f), new Vector3(0.08f, 0.76f, 0.4f), palette.wood);
            Box(console, "LegB", new Vector3(0.55f, 0.38f, 0f), new Vector3(0.08f, 0.76f, 0.4f), palette.wood);

            Transform torch = Prop("FloorFlashlight", console.TransformPoint(new Vector3(0.15f, 0.86f, 0f)));
            torch.LookAt(torch.position + Vector3.back);
            Cylinder(torch, "Body", Vector3.zero, new Vector3(0.08f, 0.16f, 0.08f), palette.metal, Quaternion.Euler(90f, 0f, 0f), false);
            Cylinder(torch, "Lens", new Vector3(0f, 0f, 0.18f), new Vector3(0.1f, 0.03f, 0.1f), palette.glow, Quaternion.Euler(90f, 0f, 0f), false);
            BoxCollider pickupCollider = torch.gameObject.AddComponent<BoxCollider>();
            pickupCollider.center = new Vector3(0f, 0.05f, 0.05f);
            pickupCollider.size = new Vector3(0.22f, 0.22f, 0.48f);
            Interactable interactable = MakeInteractable(torch.gameObject, "Flashlight", "A flashlight. The glass is intact. Press F to turn it on.");
            FlashlightPickup pickup = torch.gameObject.AddComponent<FlashlightPickup>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(interactable.OnInteract, pickup.Collect);

            Transform clock = Prop("Clock", new Vector3(-2.45f, 0f, 7.85f));
            GameObject caseBox = Box(clock, "Case", new Vector3(0f, 1.15f, 0f), new Vector3(0.46f, 2.3f, 0.28f), palette.woodDark);
            Box(clock, "Face", new Vector3(0f, 1.7f, -0.15f), new Vector3(0.28f, 0.28f, 0.04f), palette.canvas, false);
            MakeInteractable(caseBox, "Clock", "It stopped at 3:17.");

            Solid(Props, "FoyerRug", -1.4f, 1.4f, 0.01f, 0.03f, 4.4f, 6.6f, palette.fabricRed, false);
        }

        static void BuildLiving(Palette palette)
        {
            Transform hearth = Prop("Fireplace", new Vector3(-9.55f, 0f, 6.3f));
            Box(hearth, "Breast", new Vector3(0f, 0.9f, 0f), new Vector3(0.55f, 1.8f, 1.8f), palette.stone);
            Box(hearth, "Opening", new Vector3(0.18f, 0.45f, 0f), new Vector3(0.28f, 0.7f, 1.05f), palette.woodDark);
            Box(hearth, "Mantel", new Vector3(0.15f, 1.55f, 0f), new Vector3(0.4f, 0.1f, 2f), palette.wood);
            MakeInteractable(hearth.Find("Opening").gameObject, "Fireplace", "Cold. The ash hasn't been disturbed.");

            Transform sofa = Prop("Sofa", new Vector3(-7.3f, 0f, 5.5f));
            Box(sofa, "Seat", new Vector3(0f, 0.4f, 0f), new Vector3(1.7f, 0.4f, 0.7f), palette.fabricRed);
            Box(sofa, "Back", new Vector3(0f, 0.75f, 0.28f), new Vector3(1.7f, 0.55f, 0.16f), palette.fabricRed);
            MakeInteractable(sofa.Find("Seat").gameObject, "Sofa", "The cushion holds the shape of someone who left in a hurry.");

            Transform piano = Prop("Piano", new Vector3(-6.4f, 0f, 3.45f));
            GameObject body = Box(piano, "Body", new Vector3(0f, 0.55f, 0f), new Vector3(1.5f, 1.05f, 0.7f), palette.woodDark);
            Box(piano, "Keys", new Vector3(0f, 0.78f, -0.38f), new Vector3(1.2f, 0.06f, 0.16f), palette.silver, false);
            MakeInteractable(body, "Piano", "The keys are cold. One note sticks.");

            Transform chest = Prop("Chest", new Vector3(-8.3f, 0f, 8.25f));
            Box(chest, "Back", new Vector3(0f, 0.55f, 0.2f), new Vector3(1.1f, 1.05f, 0.06f), palette.wood);
            Box(chest, "SideL", new Vector3(-0.52f, 0.55f, 0f), new Vector3(0.06f, 1.05f, 0.4f), palette.wood);
            Box(chest, "SideR", new Vector3(0.52f, 0.55f, 0f), new Vector3(0.06f, 1.05f, 0.4f), palette.wood);
            Box(chest, "Bottom", new Vector3(0f, 0.08f, 0f), new Vector3(1.05f, 0.08f, 0.4f), palette.wood);
            Box(chest, "Top", new Vector3(0f, 1.05f, 0f), new Vector3(1.1f, 0.08f, 0.42f), palette.wood);
            GameObject drawer = Box(chest, "Drawer", new Vector3(0f, 0.42f, -0.18f), new Vector3(0.92f, 0.28f, 0.06f), palette.woodDark);
            Box(chest, "Pull", new Vector3(0f, 0.42f, -0.24f), new Vector3(0.12f, 0.05f, 0.04f), palette.brass, false);

            Transform locket = Prop("SilverLocket", chest.TransformPoint(new Vector3(0f, 0.46f, 0.02f)));
            GameObject locketMesh = Box(locket, "Locket", Vector3.zero, new Vector3(0.16f, 0.2f, 0.05f), palette.silver);
            BoxCollider locketCollider = locketMesh.GetComponent<BoxCollider>();
            locketCollider.size = new Vector3(1.4f, 1.2f, 3f);
            MakePickup(locketMesh, ItemId.SilverLocket, "Locket", "A silver locket. The clasp is shut.");
            locket.gameObject.SetActive(false);
            WireHold(drawer, drawer.transform, new Vector3(0f, 0f, -0.34f), locket.gameObject, "Drawer", "The drawer is stuck.", "The drawer gives.");
        }

        static void BuildDining(Palette palette)
        {
            Transform table = Prop("DiningTable", new Vector3(-7f, 0f, -0.15f));
            Box(table, "Top", new Vector3(0f, 0.78f, 0f), new Vector3(2.2f, 0.08f, 1.05f), palette.wood);
            Box(table, "LegA", new Vector3(-0.95f, 0.38f, -0.4f), new Vector3(0.08f, 0.76f, 0.08f), palette.wood);
            Box(table, "LegB", new Vector3(0.95f, 0.38f, -0.4f), new Vector3(0.08f, 0.76f, 0.08f), palette.wood);
            Box(table, "LegC", new Vector3(-0.95f, 0.38f, 0.4f), new Vector3(0.08f, 0.76f, 0.08f), palette.wood);
            Box(table, "LegD", new Vector3(0.95f, 0.38f, 0.4f), new Vector3(0.08f, 0.76f, 0.08f), palette.wood);
            MakeInteractable(table.Find("Top").gameObject, "Table", "Four places. Nothing was ever served.");

            PlaceChair(palette, new Vector3(-7f, 0f, -0.9f));
            PlaceChair(palette, new Vector3(-7f, 0f, 0.6f));
            PlaceChair(palette, new Vector3(-8.2f, 0f, -0.15f));
            PlaceChair(palette, new Vector3(-5.8f, 0f, -0.15f));

            Transform setting = Prop("PlaceSetting", new Vector3(-6.3f, 0.84f, -0.15f));
            GameObject plate = Cylinder(setting, "Plate", Vector3.zero, new Vector3(0.28f, 0.02f, 0.28f), palette.silver, Quaternion.identity, false);
            setting.gameObject.AddComponent<BoxCollider>().size = new Vector3(0.3f, 0.12f, 0.3f);
            MakeInteractable(setting.gameObject, "Plate", "Dust, and a ring where a glass stood.");
            plate.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        static void PlaceChair(Palette palette, Vector3 position)
        {
            Transform chair = Prop("Chair", position);
            Box(chair, "Seat", new Vector3(0f, 0.46f, 0f), new Vector3(0.42f, 0.08f, 0.42f), palette.wood);
            Box(chair, "Back", new Vector3(0f, 0.75f, 0.16f), new Vector3(0.42f, 0.5f, 0.06f), palette.wood);
            Box(chair, "Leg", new Vector3(0f, 0.22f, 0f), new Vector3(0.34f, 0.44f, 0.34f), palette.woodDark);
        }

        static void BuildKitchen(Palette palette)
        {
            Transform counter = Prop("Counter", new Vector3(-7.4f, 0f, -8.45f));
            Box(counter, "Top", new Vector3(0f, 0.92f, 0f), new Vector3(3.4f, 0.08f, 0.7f), palette.stone);
            Box(counter, "Base", new Vector3(0f, 0.44f, 0.05f), new Vector3(3.4f, 0.88f, 0.6f), palette.wood);

            Transform stove = Prop("Stove", new Vector3(-8.9f, 0f, -6.4f));
            GameObject stoveBody = Box(stove, "Body", new Vector3(0f, 0.5f, 0f), new Vector3(0.7f, 1f, 0.7f), palette.iron);
            Box(stove, "Burner", new Vector3(0f, 1.02f, 0f), new Vector3(0.5f, 0.04f, 0.5f), palette.metal, false);
            MakeInteractable(stoveBody, "Stove", "The burners are cold.");

            Transform key = Prop("MasterKey", new Vector3(-6.3f, 1.02f, -8.35f));
            GameObject bow = Box(key, "Bow", new Vector3(0f, 0.08f, 0f), new Vector3(0.18f, 0.16f, 0.06f), palette.brass);
            Box(key, "Shaft", new Vector3(0.12f, 0.06f, 0f), new Vector3(0.16f, 0.03f, 0.03f), palette.brass, false);
            MakePickup(bow, ItemId.MasterKey, "Key", "A brass key, still warm.");
        }

        static void BuildStudy(Palette palette)
        {
            Transform desk = Prop("Desk", new Vector3(7.3f, 0f, 7.55f));
            Box(desk, "Top", new Vector3(0f, 0.78f, 0f), new Vector3(1.6f, 0.08f, 0.7f), palette.wood);
            Box(desk, "Pedestal", new Vector3(-0.55f, 0.38f, 0f), new Vector3(0.4f, 0.76f, 0.6f), palette.woodDark);
            Box(desk, "PedestalB", new Vector3(0.55f, 0.38f, 0f), new Vector3(0.4f, 0.76f, 0.6f), palette.woodDark);

            Transform music = Prop("MusicBox", desk.TransformPoint(new Vector3(0.15f, 0.92f, 0.05f)));
            GameObject box = Box(music, "Box", Vector3.zero, new Vector3(0.28f, 0.16f, 0.2f), palette.wood);
            Cylinder(music, "Crank", new Vector3(0.14f, 0.04f, 0f), new Vector3(0.03f, 0.06f, 0.03f), palette.brass, Quaternion.Euler(0f, 0f, 90f), false);
            MakePickup(box, ItemId.MusicBox, "Music box", "A music box. The cylinder is still.");

            Transform shelf = Prop("Bookcase", new Vector3(9.55f, 0f, 5.4f));
            Box(shelf, "Side", new Vector3(0f, 1.1f, -0.45f), new Vector3(0.4f, 2.2f, 0.06f), palette.woodDark);
            Box(shelf, "SideB", new Vector3(0f, 1.1f, 0.45f), new Vector3(0.4f, 2.2f, 0.06f), palette.woodDark);
            Box(shelf, "Shelf1", new Vector3(0f, 0.5f, 0f), new Vector3(0.36f, 0.05f, 0.9f), palette.wood);
            Box(shelf, "Shelf2", new Vector3(0f, 1.15f, 0f), new Vector3(0.36f, 0.05f, 0.9f), palette.wood);
            Box(shelf, "Shelf3", new Vector3(0f, 1.8f, 0f), new Vector3(0.36f, 0.05f, 0.9f), palette.wood);
            GameObject books = Box(shelf, "Books", new Vector3(0.02f, 0.78f, 0f), new Vector3(0.22f, 0.32f, 0.7f), palette.canvas);
            MakeInteractable(books, "Books", "The titles have been scratched out.");

            Transform chair = Prop("DeskChair", new Vector3(7.3f, 0f, 6.7f));
            Box(chair, "Seat", new Vector3(0f, 0.46f, 0f), new Vector3(0.46f, 0.08f, 0.46f), palette.wood);
            Box(chair, "Back", new Vector3(0f, 0.8f, 0.18f), new Vector3(0.46f, 0.6f, 0.06f), palette.wood);
        }

        static void BuildGallery(Palette palette)
        {
            Transform row = Prop("GalleryPortraits", new Vector3(9.7f, 1.6f, -2f));
            for (int i = 0; i < 3; i++)
            {
                GameObject frame = Box(row, "Portrait" + i, new Vector3(0f, 0f, -2.2f + i * 2.2f), new Vector3(0.06f, 0.8f, 0.55f), palette.wood);
                Box(frame.transform, "Paint", new Vector3(-0.04f, 0f, 0f), new Vector3(0.02f, 0.6f, 0.38f), palette.canvas, false);
            }

            MakeInteractable(row.Find("Portrait0").gameObject, "Portrait", "The eyes are painted too carefully.");
            MakeInteractable(row.Find("Portrait1").gameObject, "Portrait", "A family. The smallest face is scraped away.");
            MakeInteractable(row.Find("Portrait2").gameObject, "Portrait", "The frame is warm, as if someone just let go.");

            Transform bench = Prop("GalleryBench", new Vector3(6.4f, 0f, -6.4f));
            GameObject seat = Box(bench, "Seat", new Vector3(0f, 0.45f, 0f), new Vector3(1.2f, 0.1f, 0.4f), palette.wood);
            Box(bench, "Leg", new Vector3(-0.5f, 0.22f, 0f), new Vector3(0.08f, 0.44f, 0.32f), palette.wood);
            Box(bench, "LegB", new Vector3(0.5f, 0.22f, 0f), new Vector3(0.08f, 0.44f, 0.32f), palette.wood);
            MakeInteractable(seat, "Bench", "Sat in recently. The dust is missing.");
        }

        static void BuildUpper(Palette palette)
        {
            Transform crib = Prop("Crib", new Vector3(-8.7f, FloorY, 6.9f));
            Box(crib, "Rail", new Vector3(0f, 0.45f, 0f), new Vector3(0.7f, 0.08f, 1.2f), palette.wood, false);
            Box(crib, "Mattress", new Vector3(0f, 0.38f, 0f), new Vector3(0.6f, 0.1f, 1.05f), palette.fabric, false);
            Box(crib, "End", new Vector3(0f, 0.55f, 0.58f), new Vector3(0.74f, 0.7f, 0.06f), palette.wood);
            Box(crib, "EndB", new Vector3(0f, 0.55f, -0.58f), new Vector3(0.74f, 0.7f, 0.06f), palette.wood);
            Box(crib, "Side", new Vector3(-0.34f, 0.6f, 0f), new Vector3(0.06f, 0.4f, 1.1f), palette.wood);
            Box(crib, "SideB", new Vector3(0.34f, 0.6f, 0f), new Vector3(0.06f, 0.4f, 1.1f), palette.wood);
            MakeInteractable(crib.Find("End").gameObject, "Crib", "The mobile doesn't turn anymore.");

            Transform studyKey = Prop("StudyKey", new Vector3(-8.7f, FloorY + 0.12f, 6.05f));
            GameObject studyBow = Box(studyKey, "Bow", new Vector3(0f, 0.08f, 0f), new Vector3(0.18f, 0.16f, 0.06f), palette.iron);
            Box(studyKey, "Shaft", new Vector3(0.12f, 0.05f, 0f), new Vector3(0.16f, 0.03f, 0.03f), palette.iron, false);
            MakePickup(studyBow, ItemId.StudyKey, "Key", "A small iron key. A book is stamped into the bow.");

            Transform tub = Prop("Tub", new Vector3(-4.7f, FloorY, 2.5f));
            GameObject tubBody = Box(tub, "Basin", new Vector3(0f, 0.4f, 0f), new Vector3(0.8f, 0.45f, 1.5f), palette.cool);
            MakeInteractable(tubBody, "Tub", "The water is long gone.");

            Transform sink = Prop("Sink", new Vector3(-6.7f, FloorY, 3.55f));
            Box(sink, "Stand", new Vector3(0f, 0.45f, 0f), new Vector3(0.5f, 0.9f, 0.4f), palette.cool);
            GameObject mirror = Box(sink, "Mirror", new Vector3(0f, 1.55f, 0f), new Vector3(0.5f, 0.7f, 0.05f), palette.glass);
            MakeInteractable(mirror, "Mirror", "The glass is fogged, though the room is dry.");

            Transform bed = Prop("Bed", new Vector3(6.6f, FloorY, 6.7f));
            Box(bed, "Frame", new Vector3(0f, 0.28f, 0f), new Vector3(1.5f, 0.25f, 2.1f), palette.woodDark);
            Box(bed, "Mattress", new Vector3(0f, 0.48f, 0f), new Vector3(1.4f, 0.18f, 2f), palette.fabricRed);
            GameObject head = Box(bed, "Headboard", new Vector3(0f, 0.8f, 0.95f), new Vector3(1.5f, 0.9f, 0.08f), palette.wood);
            MakeInteractable(head, "Bed", "The sheets are turned down. For no one.");

            Transform dresser = Prop("Dresser", new Vector3(9.15f, FloorY, 4.3f));
            Box(dresser, "Body", new Vector3(0f, 0.55f, 0f), new Vector3(0.55f, 1.1f, 1.2f), palette.wood);

            Transform portrait = Prop("CrackedPortrait", new Vector3(8.35f, FloorY + 0.85f, 4.3f));
            portrait.rotation = Quaternion.Euler(0f, 18f, 8f);
            GameObject frame = Box(portrait, "Frame", Vector3.zero, new Vector3(0.08f, 0.7f, 0.52f), palette.woodDark);
            Box(portrait, "Paint", new Vector3(-0.05f, 0f, 0f), new Vector3(0.02f, 0.52f, 0.36f), palette.canvas, false);
            Box(portrait, "Crack", new Vector3(-0.07f, 0.05f, 0.04f), new Vector3(0.02f, 0.4f, 0.03f), palette.silver, false);
            MakePickup(frame, ItemId.CrackedPortrait, "Portrait", "A cracked portrait. The glass is still in the frame.");

            Transform cupboard = Prop("Cupboard", new Vector3(9.45f, FloorY, 1.15f));
            Box(cupboard, "Back", new Vector3(0.2f, 0.9f, 0f), new Vector3(0.08f, 1.7f, 0.9f), palette.woodDark);
            Box(cupboard, "Side", new Vector3(0f, 0.9f, -0.42f), new Vector3(0.4f, 1.7f, 0.06f), palette.woodDark);
            Box(cupboard, "SideB", new Vector3(0f, 0.9f, 0.42f), new Vector3(0.4f, 1.7f, 0.06f), palette.woodDark);
            Box(cupboard, "Top", new Vector3(0f, 1.72f, 0f), new Vector3(0.42f, 0.08f, 0.9f), palette.wood);
            GameObject hatch = Box(cupboard, "Hatch", new Vector3(0f, 2.15f, 0f), new Vector3(0.5f, 0.06f, 0.5f), palette.woodDark);
            MakeInteractable(hatch, "Hatch", "Nailed shut from the other side.");

            GameObject cupboardDoor = Box(cupboard, "Door", new Vector3(-0.18f, 0.9f, 0f), new Vector3(0.06f, 1.45f, 0.7f), palette.wood);
            Box(cupboardDoor.transform, "Pull", new Vector3(-0.05f, 0f, 0.22f), new Vector3(0.04f, 0.08f, 0.08f), palette.iron, false);

            Transform candelabra = Prop("Candelabra", cupboard.TransformPoint(new Vector3(0.02f, 0.85f, 0f)));
            Box(candelabra, "Base", new Vector3(0f, 0f, 0f), new Vector3(0.16f, 0.04f, 0.16f), palette.brass, false);
            Box(candelabra, "Stem", new Vector3(0f, 0.16f, 0f), new Vector3(0.04f, 0.28f, 0.04f), palette.brass, false);
            Box(candelabra, "Arms", new Vector3(0f, 0.28f, 0f), new Vector3(0.28f, 0.04f, 0.06f), palette.brass, false);
            Box(candelabra, "Candle", new Vector3(-0.1f, 0.38f, 0f), new Vector3(0.04f, 0.14f, 0.04f), palette.candle, false);
            Box(candelabra, "CandleB", new Vector3(0.1f, 0.38f, 0f), new Vector3(0.04f, 0.14f, 0.04f), palette.candle, false);
            BoxCollider relicCollider = candelabra.gameObject.AddComponent<BoxCollider>();
            relicCollider.center = new Vector3(0f, 0.2f, 0f);
            relicCollider.size = new Vector3(0.36f, 0.5f, 0.22f);
            MakePickup(candelabra.gameObject, ItemId.Candelabra, "Candelabra", "A candelabra. The wax is cold.");
            candelabra.gameObject.SetActive(false);
            WireHold(cupboardDoor, cupboardDoor.transform, new Vector3(0f, 0f, 0.9f), candelabra.gameObject, "Cupboard", "It won't move.", "The cupboard shifts aside.");

            Transform hallBench = Prop("HallBench", new Vector3(-2.2f, FloorY, -6.2f));
            GameObject hallSeat = Box(hallBench, "Seat", new Vector3(0f, 0.45f, 0f), new Vector3(1.3f, 0.1f, 0.4f), palette.wood);
            MakeInteractable(hallSeat, "Bench", "Facing the stairs. Waiting.");

            Transform hallPortrait = Prop("HallPortrait", new Vector3(0f, FloorY + 1.6f, -8.7f));
            GameObject hallFrame = Box(hallPortrait, "Frame", Vector3.zero, new Vector3(0.9f, 1.1f, 0.06f), palette.woodDark);
            Box(hallPortrait, "Paint", new Vector3(0f, 0f, -0.04f), new Vector3(0.7f, 0.85f, 0.02f), palette.canvas, false);
            MakeInteractable(hallFrame, "Portrait", "The same house, with every window lit.");

            Transform toy = Prop("ToyChest", new Vector3(-9.2f, FloorY, 3.2f));
            GameObject toyBox = Box(toy, "Box", new Vector3(0f, 0.28f, 0f), new Vector3(0.6f, 0.5f, 0.4f), palette.wood);
            MakeInteractable(toyBox, "Chest", "Empty. The lid won't stay up.");
        }

        static Transform Prop(string name, Vector3 position)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(Props, false);
            go.transform.position = position;
            return go.transform;
        }
    }
}
