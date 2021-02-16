using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Direction = DirectionUtil.Direction;

public class Orbit : MonoBehaviour
{
    public delegate void RotateFinished();

    public RotateFinished OnRotateFinished = null;

    public float speed = 200.0f;

    private Transform target = null;
    private Vector3 axis = Vector3.zero;

    private Direction destination = Direction.None;

    //TODO: common
    public Direction GetCalcuratedDirection()
    {
        Vector3 diff = this.target.transform.position - this.transform.position;

        if (Mathf.Abs(diff.x) < Mathf.Abs(diff.z))
            if (this.target.transform.position.z > this.transform.position.z)
                return Direction.South;
            else
                return Direction.North;

        if (this.target.transform.position.x > this.transform.position.x)
            return Direction.West;
        else
            return Direction.East;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public bool Rotating()
    {
        return this.destination != Direction.None;
    }

    public void RotateRight()
    {
        this.axis.y = -1.0f;
        switch (DirectionUtil.RelativeDirection(this.transform.position, this.target.transform.position))
        {
            case Direction.South: this.destination = Direction.East; break;
            case Direction.East: this.destination = Direction.North; break;
            case Direction.North: this.destination = Direction.West; break;
            case Direction.West: this.destination = Direction.South; break;
        }
    }
    public void RotateLeft()
    {
        this.axis.y = 1.0f;
        switch (DirectionUtil.RelativeDirection(this.transform.position, this.target.transform.position))
        {
            case Direction.South: this.destination = Direction.West; break;
            case Direction.East: this.destination = Direction.South; break;
            case Direction.North: this.destination = Direction.East; break;
            case Direction.West: this.destination = Direction.North; break;
        }
    }

    private void Update()
    {
        if (this.target == null)
            return;

        if (this.destination == Direction.None)
            return;

        Vector3 lastRel = this.target.transform.position - this.transform.position;

        this.transform.RotateAround(this.target.position, this.axis, Time.deltaTime * this.speed);

        Vector3 rel = this.target.transform.position - this.transform.position;

        switch (this.destination)
        {
            case Direction.South:
                {
                    if ((lastRel.x > 0.0f && rel.x <= 0.0f) || (lastRel.x < 0.0f && rel.x >= 0.0f))
                        this.destination = Direction.None;
                    break;
                }
            case Direction.North:
                {
                    if ((lastRel.x > 0.0f && rel.x <= 0.0f) || (lastRel.x < 0.0f && rel.x >= 0.0f))
                        this.destination = Direction.None;
                    break;
                }
            case Direction.East:
                {
                    if ((lastRel.z > 0.0f && rel.z <= 0.0f) || (lastRel.z < 0.0f && rel.z >= 0.0f))
                        this.destination = Direction.None;
                    break;
                }
            case Direction.West:
                {
                    if ((lastRel.z > 0.0f && rel.z <= 0.0f) || (lastRel.z < 0.0f && rel.z >= 0.0f))
                        this.destination = Direction.None;
                    break;
                }
        }

        this.transform.LookAt(this.target.position);

        if (this.destination == Direction.None)
        {
            this.axis.y = 0.0f;
            this.OnRotateFinished?.Invoke();
        }
    }
}