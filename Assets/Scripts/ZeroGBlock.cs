using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZeroGBlock : MonoBehaviour {

    Rigidbody rb;

    void FixedUpdate () {
		if (!GameState.isInCreatorMode)
        {
            if (rb == null)
                rb = GetComponentInParent<Rigidbody>();

            RaycastHit hitInfo;

            if (!Physics.Raycast(transform.position + 0.25f * Vector3.down, Vector3.down, out hitInfo, 1.0f))
                return;
            else
            {
                if (hitInfo.collider.GetComponentInChildren<Bloc>() != null)
                    return;
            }

            rb.AddForceAtPosition(Vector3.up * GameState.repulseStrength, transform.position);
        }
	}
}
