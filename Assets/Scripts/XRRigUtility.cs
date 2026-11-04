using UnityEngine;

/// <summary>
/// Finds the player's head and hands on the Meta camera rig, with a fallback to the main camera.
/// </summary>
public static class XRRigUtility
{
    private static OVRCameraRig rig;

    private static OVRCameraRig Rig
    {
        get
        {
            if (rig == null)
                rig = Object.FindAnyObjectByType<OVRCameraRig>();
            return rig;
        }
    }

    public static Transform Head
    {
        get
        {
            if (Rig != null && Rig.centerEyeAnchor != null)
                return Rig.centerEyeAnchor;
            return Camera.main != null ? Camera.main.transform : null;
        }
    }

    public static Transform LeftHand => Rig != null ? Rig.leftHandAnchor : null;
    public static Transform RightHand => Rig != null ? Rig.rightHandAnchor : null;

    /// <summary>World-space centre of an object, using its rigidbody when it has one.</summary>
    public static Vector3 CenterOf(Component target)
    {
        var body = target.GetComponent<Rigidbody>();
        return body != null ? body.worldCenterOfMass : target.transform.position;
    }

    /// <summary>Combined world bounds of all renderers under the given transform.</summary>
    public static Bounds RendererBounds(Transform root)
    {
        var renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return new Bounds(root.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }
}
