using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace GrayBoxMansion
{
    /// <summary>
    /// A graybox placeholder, built from primitives when Play starts. It is not an imported rig.
    ///
    /// Speeds are tuned to the player defaults on FirstPersonController: walk 4.4, sprint 7.
    /// Patrol is slower than a walk. Chase sits between a walk and a sprint, so walking away
    /// loses ground and sprinting away gains it. It only chases a position it has actually seen.
    /// </summary>
    [DisallowMultipleComponent]
    public class MansionMonster : MonoBehaviour
    {
        enum Mode
        {
            Patrol,
            Chase,
            Search,
            Caught
        }

        [Header("Speed")]
        [Tooltip("Meters per second on patrol. Keep this under the player's walk speed (4.4).")]
        [SerializeField] float patrolSpeed = 2.6f;

        [Tooltip("Meters per second while it can see you, or for a moment after. Keep this above walk (4.4) and under sprint (7).")]
        [SerializeField] float chaseSpeed = 5.5f;

        [SerializeField] float patrolAcceleration = 5.5f;
        [SerializeField] float chaseAcceleration = 7.5f;

        [Header("Sight")]
        [Tooltip("Full view cone, in degrees. Walls, floors, and closed doors block the ray.")]
        [SerializeField] float viewAngle = 80f;

        [SerializeField] float viewDistance = 12f;

        [Tooltip("How long a broken glance still counts as chase, so a door frame does not instantly wipe memory.")]
        [SerializeField] float loseSightGrace = 0.4f;

        [Tooltip("Seconds spent at the last place it saw you before it gives up and patrols.")]
        [SerializeField] float searchDuration = 4.5f;

        [Tooltip("Seconds after Play before it is allowed to notice or catch you.")]
        [SerializeField] float awarenessDelay = 8f;

        [SerializeField] float catchDistance = 1.35f;

        [SerializeField] float playerChestHeight = 1.15f;

        [Header("Patrol")]
        [SerializeField] Transform[] patrolPoints;

        [SerializeField] float patrolPause = 1.15f;

        [Tooltip("Opens an unlocked door this far ahead. Locked doors and the front door stay shut.")]
        [SerializeField] float doorOpenRange = 2.2f;

        [SerializeField] float doorWaitRange = 1.85f;

        [SerializeField] GameOverSequence gameOver;

        NavMeshAgent agent;
        MansionDoor[] doors = System.Array.Empty<MansionDoor>();
        Transform player;
        Transform hips;
        Transform torso;
        Transform head;
        Transform thighGood;
        Transform shinGood;
        Transform footGood;
        Transform thighBad;
        Transform shinBad;
        Transform footBad;
        Transform armGood;
        Transform armBad;
        Material skin;
        Material joint;
        Material eyeMaterial;

        Mode mode = Mode.Patrol;
        bool ready;
        bool holdingDoor;
        bool goalSent;
        float alive;
        float timeSinceSeen;
        float searchTimer;
        float dwell;
        float stuckTimer;
        float stepPhase;
        float previousBad;
        int patrolIndex;
        Vector3 patrolGoal;
        Vector3 lastSeen;
        Vector3 sentGoal;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
                agent = gameObject.AddComponent<NavMeshAgent>();

            CapsuleCollider capsule = GetComponent<CapsuleCollider>();
            if (capsule == null)
                capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 1.7f;
            capsule.radius = 0.32f;
            capsule.center = new Vector3(0f, 0.9f, 0f);
            capsule.direction = 1;

            ConfigureAgent();
            agent.enabled = false;
            BuildRig();
        }

        void ConfigureAgent()
        {
            agent.radius = 0.34f;
            agent.height = 1.65f;
            agent.baseOffset = 0.05f;
            agent.speed = patrolSpeed;
            agent.acceleration = patrolAcceleration;
            agent.angularSpeed = 110f;
            agent.stoppingDistance = 0.45f;
            agent.autoBraking = true;
            agent.autoRepath = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.updatePosition = true;
            agent.updateRotation = true;
        }

        IEnumerator Start()
        {
            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged != null)
                player = tagged.transform;

            doors = FindObjectsByType<MansionDoor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (gameOver == null)
                gameOver = FindAnyObjectByType<GameOverSequence>();

            // Let NavMeshSurface register the baked mesh before the agent enables.
            yield return null;

            Vector3 spawn = transform.position;
            if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
                spawn = patrolPoints[0].position;

            if (!NavMesh.SamplePosition(spawn, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                Debug.LogError("The creature's spawn is not on the NavMesh. Rebuild the mansion scene.", this);
                yield break;
            }

            transform.position = hit.position;
            agent.enabled = true;
            if (!agent.Warp(hit.position))
                Debug.LogError("The creature could not stand on the NavMesh at " + hit.position + ".", this);
            ready = true;
            lastSeen = hit.position;
            if (patrolPoints != null && patrolPoints.Length > 0)
                BeginPatrolPoint();
        }

        void Update()
        {
            if (!ready || mode == Mode.Caught || agent == null || !agent.enabled)
                return;

            if (player == null)
            {
                GameObject tagged = GameObject.FindGameObjectWithTag("Player");
                if (tagged != null)
                    player = tagged.transform;
            }

            if (PlayerControl.Suspended)
            {
                agent.isStopped = true;
                return;
            }

            alive += Time.deltaTime;
            bool aware = alive >= awarenessDelay;
            bool seen = aware && player != null && CanSeePlayer();
            if (seen)
            {
                timeSinceSeen = 0f;
                lastSeen = Sampled(player.position);
                if (mode != Mode.Chase)
                    goalSent = false;
                mode = Mode.Chase;
            }
            else if (mode == Mode.Chase)
            {
                timeSinceSeen += Time.deltaTime;
                if (timeSinceSeen > loseSightGrace)
                    EnterSearch();
            }

            if (aware && player != null && IsTouchingPlayer())
            {
                CatchPlayer();
                return;
            }

            bool waitForDoor = ManageDoors();
            if (holdingDoor && !waitForDoor)
                goalSent = false;
            holdingDoor = waitForDoor;
            agent.isStopped = waitForDoor;
            if (waitForDoor)
            {
                stuckTimer = 0f;
                return;
            }

            if (mode == Mode.Patrol)
                TickPatrol();
            else if (mode == Mode.Chase)
                TickChase();
            else if (mode == Mode.Search)
                TickSearch();
        }

        void LateUpdate()
        {
            PoseBody();
        }

        void OnDestroy()
        {
            if (skin != null)
                Destroy(skin);
            if (joint != null)
                Destroy(joint);
            if (eyeMaterial != null)
                Destroy(eyeMaterial);
        }

        void TickPatrol()
        {
            ApplyPace(false);
            Seek(patrolGoal);
            if (agent.pathPending)
                return;

            if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 2.5f)
                {
                    stuckTimer = 0f;
                    AdvancePatrol();
                }

                return;
            }

            if (agent.velocity.sqrMagnitude < 0.05f && !Arrived())
            {
                stuckTimer += Time.deltaTime;
                if (stuckTimer > 2.5f)
                {
                    stuckTimer = 0f;
                    AdvancePatrol();
                }

                return;
            }

            stuckTimer = 0f;
            if (!Arrived())
                return;

            dwell += Time.deltaTime;
            if (dwell >= patrolPause)
                AdvancePatrol();
        }

        void TickChase()
        {
            ApplyPace(true);
            Seek(lastSeen);
        }

        void TickSearch()
        {
            ApplyPace(false);
            Seek(lastSeen);
            if (agent.pathPending)
                return;

            bool atMark = agent.hasPath && agent.remainingDistance <= 0.75f;
            bool nowhere = agent.pathStatus == NavMeshPathStatus.PathInvalid;
            if (!atMark && !nowhere)
                return;

            searchTimer += Time.deltaTime;
            float giveUp = nowhere ? 1.5f : searchDuration;
            if (searchTimer >= giveUp)
                ResumePatrol();
        }

        void EnterSearch()
        {
            mode = Mode.Search;
            searchTimer = 0f;
            goalSent = false;
            ApplyPace(false);
            Seek(lastSeen);
        }

        void ResumePatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                mode = Mode.Patrol;
                return;
            }

            int best = 0;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] == null)
                    continue;
                float distance = (patrolPoints[i].position - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = i;
                }
            }

            patrolIndex = best;
            BeginPatrolPoint();
        }

        void AdvancePatrol()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
                return;

            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
            BeginPatrolPoint();
        }

        void BeginPatrolPoint()
        {
            mode = Mode.Patrol;
            dwell = 0f;
            stuckTimer = 0f;
            goalSent = false;
            Transform mark = patrolPoints[patrolIndex];
            patrolGoal = mark != null ? Sampled(mark.position) : transform.position;
            ApplyPace(false);
            Seek(patrolGoal);
        }

        void ApplyPace(bool chasing)
        {
            agent.speed = chasing ? chaseSpeed : patrolSpeed;
            agent.acceleration = chasing ? chaseAcceleration : patrolAcceleration;
            agent.autoBraking = !chasing;
            agent.stoppingDistance = chasing ? 0.6f : 0.45f;
        }

        void Seek(Vector3 goal)
        {
            if (!agent.enabled)
                return;
            if (goalSent && (goal - sentGoal).sqrMagnitude < 0.04f)
                return;

            sentGoal = goal;
            goalSent = true;
            agent.SetDestination(goal);
        }

        bool Arrived()
        {
            if (agent.pathPending || !agent.hasPath)
                return false;
            return agent.remainingDistance <= agent.stoppingDistance + 0.15f;
        }

        Vector3 Sampled(Vector3 raw)
        {
            if (NavMesh.SamplePosition(raw, out NavMeshHit hit, 2.5f, NavMesh.AllAreas))
                return hit.position;
            return raw;
        }

        Vector3 CurrentGoal()
        {
            if (mode == Mode.Patrol)
                return patrolGoal;
            return lastSeen;
        }

        bool ManageDoors()
        {
            Vector3 goal = CurrentGoal();
            MansionDoor ahead = NearestDoor(doorOpenRange, true);
            if (ahead != null && ahead.AllowsMonsterThrough && !ahead.IsPassageClear)
            {
                ahead.OpenForMonster();
                if (ahead.DistanceFrom(transform.position) < doorWaitRange)
                    return true;
            }

            if (!agent.pathPending && agent.pathStatus == NavMeshPathStatus.PathPartial)
            {
                MansionDoor between = DoorOnSegment(transform.position, goal);
                if (between != null && between.AllowsMonsterThrough && !between.IsPassageClear)
                {
                    between.OpenForMonster();
                    return true;
                }
            }

            return false;
        }

        MansionDoor NearestDoor(float range, bool aheadOnly)
        {
            MansionDoor best = null;
            float bestDistance = range;
            for (int i = 0; i < doors.Length; i++)
            {
                MansionDoor door = doors[i];
                if (door == null)
                    continue;

                float distance = door.DistanceFrom(transform.position);
                if (distance > bestDistance)
                    continue;

                if (aheadOnly)
                {
                    Vector3 toDoor = door.PanelCenter - transform.position;
                    toDoor.y = 0f;
                    if (toDoor.sqrMagnitude > 0.04f && Vector3.Dot(transform.forward, toDoor.normalized) < 0.15f)
                        continue;
                }

                best = door;
                bestDistance = distance;
            }

            return best;
        }

        MansionDoor DoorOnSegment(Vector3 from, Vector3 to)
        {
            Vector3 flatFrom = from;
            Vector3 flatTo = to;
            flatFrom.y = 0f;
            flatTo.y = 0f;
            Vector3 segment = flatTo - flatFrom;
            float lengthSqr = segment.sqrMagnitude;
            if (lengthSqr < 0.25f)
                return null;

            MansionDoor best = null;
            float bestT = float.MaxValue;
            for (int i = 0; i < doors.Length; i++)
            {
                MansionDoor door = doors[i];
                if (door == null || !door.AllowsMonsterThrough || door.IsPassageClear)
                    continue;

                Vector3 center = door.PanelCenter;
                center.y = 0f;
                float along = Vector3.Dot(center - flatFrom, segment) / lengthSqr;
                if (along < 0.02f || along > 1.05f)
                    continue;

                Vector3 closest = flatFrom + segment * Mathf.Clamp01(along);
                float side = Vector3.Distance(center, closest);
                if (side > 1.7f)
                    continue;
                if (along < bestT)
                {
                    bestT = along;
                    best = door;
                }
            }

            return best;
        }

        bool CanSeePlayer()
        {
            Vector3 eye = EyePosition();
            Vector3 target = player.position + Vector3.up * playerChestHeight;
            Vector3 toTarget = target - eye;
            float distance = toTarget.magnitude;
            if (distance < 0.05f || distance > viewDistance)
                return false;
            if (Vector3.Angle(transform.forward, toTarget) > viewAngle * 0.5f)
                return false;
            return LineReachesPlayer(eye, target);
        }

        bool IsTouchingPlayer()
        {
            Vector3 delta = player.position - transform.position;
            if (Mathf.Abs(delta.y) > 2.2f)
                return false;
            delta.y = 0f;
            if (delta.magnitude > catchDistance)
                return false;
            return LineReachesPlayer(EyePosition(), player.position + Vector3.up * playerChestHeight);
        }

        bool LineReachesPlayer(Vector3 from, Vector3 to)
        {
            Vector3 toTarget = to - from;
            float distance = toTarget.magnitude;
            if (distance < 0.05f)
                return true;

            RaycastHit[] hits = Physics.RaycastAll(
                from,
                toTarget / distance,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore);

            float closest = float.MaxValue;
            Transform closestTransform = null;
            for (int i = 0; i < hits.Length; i++)
            {
                Transform hitTransform = hits[i].transform;
                if (hitTransform == transform || hitTransform.IsChildOf(transform))
                    continue;
                if (hits[i].distance >= closest)
                    continue;
                closest = hits[i].distance;
                closestTransform = hitTransform;
            }

            if (closestTransform == null)
                return false;
            return closestTransform == player || closestTransform.IsChildOf(player);
        }

        Vector3 EyePosition()
        {
            if (head != null)
                return head.position + transform.forward * 0.18f + Vector3.up * 0.05f;
            return transform.position + Vector3.up * 1.6f + transform.forward * 0.2f;
        }

        void CatchPlayer()
        {
            mode = Mode.Caught;
            agent.isStopped = true;
            agent.enabled = false;
            if (gameOver == null)
            {
                GameObject screenObject = new GameObject("GameOver");
                gameOver = screenObject.AddComponent<GameOverSequence>();
            }

            gameOver.Begin();
        }

        void PoseBody()
        {
            if (hips == null)
                return;

            float moved = 0f;
            if (agent != null && agent.enabled)
                moved = agent.velocity.magnitude;

            float hurry = (mode == Mode.Chase) ? 1f : 0f;
            float rate = Mathf.Lerp(1.15f, 2.15f, hurry);
            if (moved > 0.2f)
                stepPhase += Time.deltaTime * rate * Mathf.Clamp(moved / Mathf.Max(0.5f, patrolSpeed), 0.65f, 2.1f);

            float good = Mathf.Sin(stepPhase * Mathf.PI * 2f);
            // The bad leg lags, takes a shorter step, and spends longer dragging.
            float badWave = Mathf.Sin(stepPhase * Mathf.PI * 2f + 0.85f);
            float bad = Mathf.Lerp(badWave, -0.45f, 0.55f);

            PoseLeg(thighGood, shinGood, footGood, good, true, hurry);
            PoseLeg(thighBad, shinBad, footBad, bad, false, hurry);

            float plant = Mathf.Clamp01(-bad);
            float dip = plant * Mathf.Lerp(0.1f, 0.14f, hurry);
            float lean = -10f - plant * 4f;
            hips.localPosition = new Vector3(-0.05f, 0.98f - dip, 0.02f);
            hips.localRotation = Quaternion.Euler(plant * 16f, 0f, lean);
            if (torso != null)
                torso.localRotation = Quaternion.Euler(7f + good * 5f, 0f, 5f);
            if (head != null)
                head.localRotation = Quaternion.Euler(-6f, Mathf.Sin(stepPhase) * 8f, -lean * 0.25f);
            if (armGood != null)
                armGood.localRotation = Quaternion.Euler(-18f - good * (28f + hurry * 10f), 0f, 14f);
            if (armBad != null)
                armBad.localRotation = Quaternion.Euler(36f - bad * 8f, 8f, -28f);

            if (previousBad > 0.15f && bad <= 0.15f && moved > 0.25f && GameAudio.Instance != null)
                GameAudio.Instance.PlayFootstep(transform.position, false);
            previousBad = bad;
        }

        static void PoseLeg(Transform thigh, Transform shin, Transform foot, float stride, bool strongLeg, float hurry)
        {
            if (thigh == null)
                return;

            float swing = strongLeg ? Mathf.Lerp(30f, 36f, hurry) : 14f;
            // Negative pitch steps toward the body's forward (+Z).
            thigh.localRotation = Quaternion.Euler(-stride * swing, 0f, strongLeg ? 2f : -8f);
            float knee = strongLeg
                ? 18f + Mathf.Max(0f, stride) * 34f
                : 48f + Mathf.Max(0f, -stride) * 12f;
            if (shin != null)
                shin.localRotation = Quaternion.Euler(-knee, 0f, strongLeg ? 0f : 6f);
            if (foot != null)
                foot.localRotation = Quaternion.Euler(strongLeg ? -6f : 22f, 0f, strongLeg ? 0f : 10f);
        }

        void BuildRig()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogError("URP Lit shader is missing, so the creature has no material.", this);
                return;
            }

            skin = MakeMaterial(shader, new Color(0.2f, 0.2f, 0.21f), 0f);
            joint = MakeMaterial(shader, new Color(0.1f, 0.1f, 0.11f), 0f);
            Color eyeColor = new Color(0.78f, 0.74f, 0.62f);
            eyeMaterial = MakeMaterial(shader, eyeColor, 0.55f);

            hips = Bone(transform, "Hips", new Vector3(0f, 0.98f, 0f));
            Chunk(hips, "Pelvis", Vector3.zero, new Vector3(0.38f, 0.18f, 0.22f), skin);

            torso = Bone(hips, "Torso", new Vector3(0f, 0.26f, 0.02f));
            Chunk(torso, "Chest", new Vector3(0f, 0.16f, 0f), new Vector3(0.42f, 0.4f, 0.24f), skin);
            Chunk(torso, "ShoulderBad", new Vector3(-0.2f, 0.28f, 0.02f), new Vector3(0.16f, 0.1f, 0.14f), joint);

            head = Bone(torso, "Head", new Vector3(0.02f, 0.42f, 0.04f));
            Chunk(head, "Skull", Vector3.zero, new Vector3(0.22f, 0.26f, 0.24f), skin);
            Chunk(head, "EyeGood", new Vector3(0.06f, 0.03f, 0.11f), new Vector3(0.045f, 0.035f, 0.03f), eyeMaterial);
            Chunk(head, "EyeBad", new Vector3(-0.07f, -0.01f, 0.1f), new Vector3(0.05f, 0.03f, 0.03f), eyeMaterial);

            armGood = Bone(torso, "ArmGood", new Vector3(0.26f, 0.22f, 0f));
            Chunk(armGood, "Upper", new Vector3(0.02f, -0.18f, 0f), new Vector3(0.08f, 0.34f, 0.08f), skin);
            Chunk(armGood, "Lower", new Vector3(0.03f, -0.4f, 0.02f), new Vector3(0.07f, 0.26f, 0.07f), joint);

            armBad = Bone(torso, "ArmBad", new Vector3(-0.26f, 0.16f, 0.03f));
            Chunk(armBad, "Upper", new Vector3(-0.03f, -0.22f, 0f), new Vector3(0.08f, 0.42f, 0.08f), skin);
            Chunk(armBad, "Lower", new Vector3(-0.08f, -0.52f, 0.05f), new Vector3(0.07f, 0.36f, 0.07f), joint);

            thighGood = Bone(hips, "ThighGood", new Vector3(0.12f, -0.06f, 0f));
            Chunk(thighGood, "Thigh", new Vector3(0f, -0.2f, 0f), new Vector3(0.1f, 0.4f, 0.1f), skin);
            shinGood = Bone(thighGood, "ShinGood", new Vector3(0f, -0.4f, 0f));
            Chunk(shinGood, "Shin", new Vector3(0f, -0.18f, 0f), new Vector3(0.08f, 0.36f, 0.08f), joint);
            footGood = Bone(shinGood, "FootGood", new Vector3(0f, -0.36f, 0.04f));
            Chunk(footGood, "Foot", new Vector3(0f, -0.02f, 0.06f), new Vector3(0.1f, 0.05f, 0.18f), skin);

            thighBad = Bone(hips, "ThighBad", new Vector3(-0.14f, -0.05f, 0.02f));
            Chunk(thighBad, "Thigh", new Vector3(0f, -0.16f, 0f), new Vector3(0.1f, 0.32f, 0.1f), skin);
            shinBad = Bone(thighBad, "ShinBad", new Vector3(0f, -0.32f, 0f));
            Chunk(shinBad, "Shin", new Vector3(0f, -0.12f, 0.02f), new Vector3(0.08f, 0.24f, 0.08f), joint);
            footBad = Bone(shinBad, "FootBad", new Vector3(0f, -0.24f, 0.02f));
            Chunk(footBad, "Foot", new Vector3(0.02f, 0.02f, 0.05f), new Vector3(0.12f, 0.05f, 0.16f), skin);
        }

        static Transform Bone(Transform parent, string boneName, Vector3 localPosition)
        {
            GameObject bone = new GameObject(boneName);
            bone.transform.SetParent(parent, false);
            bone.transform.localPosition = localPosition;
            bone.transform.localRotation = Quaternion.identity;
            return bone.transform;
        }

        static void Chunk(Transform parent, string chunkName, Vector3 localPosition, Vector3 scale, Material material)
        {
            GameObject chunk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            chunk.name = chunkName;
            chunk.transform.SetParent(parent, false);
            chunk.transform.localPosition = localPosition;
            chunk.transform.localRotation = Quaternion.identity;
            chunk.transform.localScale = scale;
            Collider chunkCollider = chunk.GetComponent<Collider>();
            if (chunkCollider != null)
                Destroy(chunkCollider);
            Renderer renderer = chunk.GetComponent<Renderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        static Material MakeMaterial(Shader shader, Color color, float emission)
        {
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
    }
}
