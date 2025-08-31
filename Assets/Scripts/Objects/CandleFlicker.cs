using UnityEngine;

public class CandleFlicker : MonoBehaviour
{
    private Light candleLight;
    private float baseIntensity;

    void Start()
    {
        candleLight = GetComponent<Light>();
        baseIntensity = candleLight.intensity;
    }

    void Update()
    {
        candleLight.intensity = baseIntensity + Random.Range(-0.2f, 0.2f);
        candleLight.range = 2f + Random.Range(-0.1f, 0.1f);
    }
}
