using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public static CameraMover instance { get; private set; }

    private void Awake()
    {
        instance = this;
    }
}
