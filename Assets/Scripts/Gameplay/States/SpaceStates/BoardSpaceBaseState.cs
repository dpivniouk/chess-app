using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoardSpaceBaseState
{
    public abstract void Update(BoardSpaceExtensions space);

    public abstract void OnMouseEnter(BoardSpaceExtensions space);

    public abstract void OnMouseExit(BoardSpaceExtensions space);

    public abstract void OnStateEnter(BoardSpaceExtensions space);
}
