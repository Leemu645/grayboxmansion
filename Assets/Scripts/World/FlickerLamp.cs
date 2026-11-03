using UnityEngine;

namespace GrayBoxMansion
{
    /// <summary>
    /// Wobbles a point light and, now and then, dips it. Cheap enough for a few lamps.
    /// </summary>
    public class FlickerLamp : MonoBehaviour
    {
        [Range(0f, 0.8f)]
        [SerializeField] float strength = 0.35f;

        [SerializeField] float speed = 7f;

        Light lamp;
        float baseIntensity;
        float seed;

        void Awake()
        {
            lamp = GetComponent<Light>();
            baseIntensity = lamp != null ? lamp.intensity : 1f;
            seed = Random.Range(0f, 50f);
        }

        void Update()
        {
            if (lamp == null)
                return;

            float wobble = Mathf.PerlinNoise(Time.time * speed, seed);
            float scale = Mathf.Lerp(1f - strength, 1f, wobble);
            float dip = Mathf.PerlinNoise(seed, Time.time * 2.1f);
            if (dip > 0.93f)
                scale *= 0.35f;
            lamp.intensity = baseIntensity * scale;
        }
    }
}
