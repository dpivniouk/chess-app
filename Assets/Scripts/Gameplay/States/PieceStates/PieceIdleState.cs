using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceIdleState : PieceBaseState
{
    public override void OnMouseDown(PieceController piece)
    {
        if (GameManager.instance.enableInteraction && GameManager.instance.selectedObject == null)
        {
            piece.TransitionToState(piece.SelectedState);
        }
    }

    public override void OnMouseEnter(PieceController piece)
    {
        if (GameManager.instance.activeColor != piece.pieceColor) return;

        if (GameManager.instance.enableInteraction && GameManager.instance.selectedObject == null)
        {
            foreach (Component child in piece.GetComponentsInChildren<MeshRenderer>())
            {
                child.gameObject.layer = 13;
            }
        }
    }

    public override void OnMouseExit(PieceController piece)
    {
        if (GameManager.instance.activeColor != piece.pieceColor) return;

        if (GameManager.instance.enableInteraction && GameManager.instance.selectedObject == null)
        {
            foreach (Component child in piece.GetComponentsInChildren<MeshRenderer>())
            {
                child.gameObject.layer = 9;
            }
        }

    }

    public override void OnStateEnter(PieceController piece)
    {
        foreach (Component child in piece.GetComponentsInChildren<MeshRenderer>())
        {
            child.gameObject.layer = 9;
        }
        if (GameManager.instance.selectedObject == piece.gameObject)
        {
            GameManager.instance.selectedObject = null;
        }

    }

    public override void Update(PieceController piece)
    {
    }
}
