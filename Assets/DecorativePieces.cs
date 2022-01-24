using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorativePieces : MonoBehaviour
{
    private Vector3 forceImpulse;

    private void Start()
    {
        Rigidbody rb = GetComponentInChildren<Rigidbody>();

        float xForce = Random.Range(-2f, 2f);
        float yForce = Random.Range(-2f,0f);
        float zForce = Random.Range(-2f,2f);

        forceImpulse = new Vector3(xForce, yForce, zForce);

        rb.AddForce(forceImpulse, ForceMode.Impulse);
    }
}
