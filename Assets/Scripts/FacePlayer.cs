using UnityEngine;

/// <summary>
/// Turns this object around the vertical axis so text stays readable from where the player stands.
/// </summary>
public class FacePlayer : MonoBehaviour
{
    private void LateUpdate()
    {
        Transform head = XRRigUtility.Head;
        if (head == null)
            return;

        Vector3 away = transform.position - head.position;
        away.y = 0f;
        if (away.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(away);
    }
}
