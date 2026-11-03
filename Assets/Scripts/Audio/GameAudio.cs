using UnityEngine;

namespace GrayBoxMansion
{
    /// <summary>
    /// Shared one-shots. World sounds are 3D. The player's own actions are flat.
    /// Master volume is applied to the listener.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameAudio : MonoBehaviour
    {
        public static GameAudio Instance { get; private set; }

        const int VoiceCount = 12;

        [Range(0f, 1f)]
        [SerializeField] float masterVolume = 0.85f;

        [SerializeField] AudioClip[] footstepWood;
        [SerializeField] AudioClip jump;
        [SerializeField] AudioClip land;
        [SerializeField] AudioClip pickup;
        [SerializeField] AudioClip keyJingle;
        [SerializeField] AudioClip flashlightOn;
        [SerializeField] AudioClip flashlightOff;
        [SerializeField] AudioClip[] doorCreak;
        [SerializeField] AudioClip[] lockUnlock;
        [SerializeField] AudioClip lockRattle;
        [SerializeField] AudioClip slotFill;

        AudioSource[] voices;
        int nextVoice;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("More than one GameAudio is in the scene. Keeping the first.", this);
                return;
            }

            Instance = this;
            voices = new AudioSource[VoiceCount];
            for (int i = 0; i < voices.Length; i++)
            {
                GameObject child = new GameObject("Voice" + i);
                child.transform.SetParent(transform, false);
                AudioSource source = child.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.dopplerLevel = 0f;
                source.rolloffMode = AudioRolloffMode.Linear;
                source.minDistance = 1.4f;
                source.maxDistance = 16f;
                source.spatialBlend = 1f;
                voices[i] = source;
            }
        }

        void LateUpdate()
        {
            AudioListener.volume = masterVolume;
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
                AudioListener.volume = 1f;
            }
        }

        void OnValidate()
        {
            masterVolume = Mathf.Clamp01(masterVolume);
            if (Application.isPlaying)
                AudioListener.volume = masterVolume;
        }

        public void PlayFootstep(Vector3 position, bool sprint)
        {
            Play(Pick(footstepWood), position, sprint ? 0.48f : 0.34f, 1f, 0.08f);
        }

        public void PlayJump()
        {
            Play(jump, transform.position, 0.4f, 0f, 0.04f);
        }

        public void PlayLand(Vector3 position)
        {
            Play(land, position, 0.55f, 1f, 0.05f);
        }

        public void PlayPickup()
        {
            Play(pickup, transform.position, 0.5f, 0f, 0.03f);
        }

        public void PlayKeyJingle(Vector3 position)
        {
            Play(keyJingle, position, 0.62f, 1f, 0.04f);
        }

        public void PlayFlashlight(bool turnedOn)
        {
            Play(turnedOn ? flashlightOn : flashlightOff, transform.position, 0.45f, 0f, 0.02f);
        }

        public void PlayDoorCreak(Vector3 position)
        {
            Play(Pick(doorCreak), position, 0.62f, 1f, 0.04f);
        }

        public void PlayLockUnlock(Vector3 position)
        {
            Play(Pick(lockUnlock), position, 0.7f, 1f, 0.03f);
        }

        public void PlayLockRattle(Vector3 position)
        {
            Play(lockRattle, position, 0.55f, 1f, 0.04f);
        }

        public void PlaySlotFill(Vector3 position, int filledCount)
        {
            float pitch = 1f + Mathf.Clamp(filledCount - 1, 0, 3) * 0.07f;
            Play(slotFill, position, 0.62f, 0.35f, 0.01f, pitch);
        }

        public void PlayAt(AudioClip clip, Vector3 position, float volume = 1f, float spatialBlend = 1f)
        {
            Play(clip, position, volume, spatialBlend, 0.04f);
        }

        public void Play2D(AudioClip clip, float volume = 1f)
        {
            Play(clip, transform.position, volume, 0f, 0.03f);
        }

        void Play(AudioClip clip, Vector3 position, float volume, float spatialBlend, float pitchJitter, float pitchScale = 1f)
        {
            if (clip == null || voices == null || masterVolume <= 0.001f)
                return;

            AudioSource source = TakeVoice();
            source.transform.position = position;
            source.clip = clip;
            source.volume = Mathf.Clamp01(volume);
            source.spatialBlend = Mathf.Clamp01(spatialBlend);
            source.minDistance = 1.4f;
            source.maxDistance = 16f;
            float jitter = Random.Range(-Mathf.Abs(pitchJitter), Mathf.Abs(pitchJitter));
            source.pitch = Mathf.Clamp(pitchScale + jitter, 0.2f, 3f);
            source.Play();
        }

        AudioSource TakeVoice()
        {
            for (int i = 0; i < voices.Length; i++)
            {
                int index = (nextVoice + i) % voices.Length;
                if (!voices[index].isPlaying)
                {
                    nextVoice = (index + 1) % voices.Length;
                    return voices[index];
                }
            }

            AudioSource stolen = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;
            stolen.Stop();
            return stolen;
        }

        static AudioClip Pick(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
                return null;

            int start = Random.Range(0, clips.Length);
            for (int i = 0; i < clips.Length; i++)
            {
                AudioClip clip = clips[(start + i) % clips.Length];
                if (clip != null)
                    return clip;
            }

            return null;
        }
    }
}
