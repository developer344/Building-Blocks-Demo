using System.Collections;
using Oculus.Interaction;
using UnityEngine;

/// <summary>
/// An item that stays hidden until <see cref="Reveal"/> is called. It then rises out of a burst
/// of sparkles and hovers, slowly spinning, until the player grabs it.
/// </summary>
public class HiddenItem : MonoBehaviour
{
    [SerializeField] private ParticleSystem revealSparkles;
    [Tooltip("How long the item takes to rise to its hover height.")]
    [SerializeField] private float growDuration = 1f;
    [Tooltip("How high above its resting place the item floats until it is picked up.")]
    [SerializeField] private float hoverHeight = 0.25f;
    [SerializeField] private float spinSpeed = 60f;

    private bool revealed;

    public void Reveal()
    {
        if (revealed)
            return;

        revealed = true;
        gameObject.SetActive(true);
        StartCoroutine(AppearAndHover());
        Debug.Log($"[Escape Room] {name} revealed at {transform.position}");
    }

    private IEnumerator AppearAndHover()
    {
        var body = GetComponent<Rigidbody>();
        var grabbable = GetComponent<Grabbable>();
        bool IsHeld() => grabbable != null && grabbable.SelectingPointsCount > 0;

        if (body != null)
            body.isKinematic = true;
        if (revealSparkles != null)
            revealSparkles.Play();

        // Rise up out of the table. The scale is never animated: a grab locks in whatever
        // scale the item has at that moment, so it always stays full size.
        Vector3 start = transform.position;
        Vector3 hover = start + Vector3.up * hoverHeight;

        for (float t = 0f; t < growDuration && !IsHeld(); t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(start, hover, Mathf.SmoothStep(0f, 1f, t / growDuration));
            transform.Rotate(0f, spinSpeed * 3f * Time.deltaTime, 0f, Space.World);
            yield return null;
        }

        // Float and spin in place so it is easy to spot, until someone picks it up.
        for (float t = 0f; !IsHeld(); t += Time.deltaTime)
        {
            transform.position = hover + Vector3.up * (Mathf.Sin(t * 2f) * 0.03f);
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
            yield return null;
        }

        // Hand it over to physics once it has been let go.
        while (IsHeld())
            yield return null;
        yield return null;

        if (body != null)
        {
            body.isKinematic = false;
            body.useGravity = true;
        }
    }
}
