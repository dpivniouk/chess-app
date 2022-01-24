using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PieceCaptureTargetState : PieceBaseState
{
    public override void OnMouseDown(PieceController piece)
    {
    }

    public override void OnMouseEnter(PieceController piece)
    {
        foreach (Component child in piece.GetComponentsInChildren<MeshRenderer>())
        {
            child.gameObject.layer = 13;
        }
    }

    public override void OnMouseExit(PieceController piece)
    {
        foreach (Component child in piece.GetComponentsInChildren<MeshRenderer>())
        {
            child.gameObject.layer = 15;
        }
    }

    public override void OnStateEnter(PieceController piece)
    {
        foreach (Component child in piece.GetComponentsInChildren<MeshRenderer>())
        {
            child.gameObject.layer = 15;
        }
    }

    public override void Update(PieceController piece)
    {
    }
}
