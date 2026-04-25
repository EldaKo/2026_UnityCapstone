using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class TargetPulse : MonoBehaviour
{
    private Material mat;

    [Header("Glow Settings")]
    public Color glowColor = Color.yellow;
    public float minIntensity = 0.5f;
    public float maxIntensity = 2.0f;
    public float glowSpeed = 2f;

    [Header("Scale Settings")]
    public float scaleAmount = 0.1f;
    public float scaleSpeed = 2f;

    private Vector3 baseScale;

    void Start()
    {

        mat = GetComponent<Renderer>().material;
        mat.EnableKeyword("_EMISSION");

        baseScale = transform.localScale;
    }

    void Update()
    {

        float t = Mathf.PingPong(Time.time * glowSpeed, 1f);
        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        mat.SetColor("_EmissionColor", glowColor * intensity);


        float scale = 1f + Mathf.Sin(Time.time * scaleSpeed) * scaleAmount;
        transform.localScale = baseScale * scale;
    }
}