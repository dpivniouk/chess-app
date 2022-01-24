using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpaceValidCastleTargetState: BoardSpaceBaseState
{
    public override void Update(BoardSpaceExtensions space)
    {
    }

    public override void OnMouseEnter(BoardSpaceExtensions space)
    {
        if (GameManager.instance.enableInteraction)
        {
            foreach (Component child in space.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                child.gameObject.layer = 13;
            }
        }

    }

    public override void OnMouseExit(BoardSpaceExtensions space)
    {
        if (GameManager.instance.enableInteraction)
        {
            foreach (Component child in space.gameObject.GetComponentsInChildren<MeshRenderer>())
            {
                child.gameObject.layer = 17;
            }
        }
    }

    public override void OnStateEnter(BoardSpaceExtensions space)
    {
        foreach (Component child in space.gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            child.gameObject.layer = 17;
        }
    }
}
