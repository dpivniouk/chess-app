using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpaceExtensions : MonoBehaviour
{
    //State Machine
    public BoardSpaceBaseState currentState;
    public readonly BoardSpaceIdleState IdleState = new BoardSpaceIdleState();
    public readonly BoardSpaceValidMovementTargetSpace MovementTargetState = new BoardSpaceValidMovementTargetSpace();
    public readonly BoardSpaceValidCaptureTargetState CaptureTargetState = new BoardSpaceValidCaptureTargetState();
    public readonly BoardSpaceValidCastleTargetState CastleTargetState = new BoardSpaceValidCastleTargetState();

    private void Update()
    {
        currentState.Update(this);
    }

    void Start()
    {
        TransitionToState(IdleState);
    }

    void OnMouseEnter()
    {
        currentState.OnMouseEnter(this);
    }


    void OnMouseExit()
    {
        currentState.OnMouseExit(this);
    }

    public void TransitionToState(BoardSpaceBaseState state)
    {
        currentState = state;
        currentState.OnStateEnter(this);
    }
}
