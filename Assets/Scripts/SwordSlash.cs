/* Ethan Gapic-Kott, 000923124 */

using UnityEngine;
using System.Collections;

public class SwordSlash : MonoBehaviour
{
    public float slashAngle = 60f;   // How far the sword swings
    public float slashSpeed = 12f;   // Swing speed

    Quaternion startRot;             // Original sword rotation

    void Start()
    {
        // Store starting rotation
        startRot = transform.localRotation;
    }

    // Called when player attacks
    public void PlaySlash()
    {
        StopAllCoroutines();
        StartCoroutine(Slash());
    }

    // Handles sword swing animation
    IEnumerator Slash()
    {
        Quaternion slashRot = startRot * Quaternion.Euler(0, 0, -slashAngle);

        float t = 0;

        // Swing forward
        while (t < 1)
        {
            t += Time.deltaTime * slashSpeed;
            transform.localRotation = Quaternion.Slerp(startRot, slashRot, t);
            yield return null;
        }

        t = 0;

        // Returns to the original position
        while (t < 1)
        {
            t += Time.deltaTime * slashSpeed;
            transform.localRotation = Quaternion.Slerp(slashRot, startRot, t);
            yield return null;
        }
    }
}