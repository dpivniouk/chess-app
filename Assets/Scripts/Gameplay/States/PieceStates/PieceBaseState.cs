using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PieceBaseState
{
    public abstract void Update(PieceController piece);

    public abstract void OnStateEnter(PieceController piece);

    public abstract void OnMouseEnter(PieceController piece);

    public abstract void OnMouseExit(PieceController piece);

    public abstract void OnMouseDown(PieceController piece);
}
