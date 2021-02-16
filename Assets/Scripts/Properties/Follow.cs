using UnityEngine;

public class Follow : MonoBehaviour
{
    public Vector3 baseSouthDistance = Vector3.zero;
    public float speed = 1.0f;

    private Transform target = null;
    private Vector3 distance = Vector3.zero;

    public void SetTarget(Transform target)
    {
        this.target = target;
    }

    public void SetDistance(Vector3 value)
    {
        this.distance = value;
    }

    // TODO: common 
    public DirectionUtil.Direction GetCalcuratedDirection()
    {
        Vector3 diff = this.target.transform.position - this.transform.position;

        if (Mathf.Abs(diff.x) < Mathf.Abs(diff.z))
            if (this.target.transform.position.z > this.transform.position.z)
                return DirectionUtil.Direction.South;
            else
                return DirectionUtil.Direction.North;

        if (this.target.transform.position.x > this.transform.position.x)
            return DirectionUtil.Direction.West;
        else
            return DirectionUtil.Direction.East;
    }

    public void RefreshDistance()
    {
        DirectionUtil.Direction currentDirection = this.GetCalcuratedDirection();
        if (currentDirection == DirectionUtil.Direction.None)
            currentDirection = DirectionUtil.Direction.South;

        Vector3 baseDistance = this.baseSouthDistance;
        Vector3 newDistance = baseDistance;

        switch (currentDirection)
        {
            case DirectionUtil.Direction.North:
                {
                    newDistance.x = -baseDistance.x;
                    newDistance.z = -baseDistance.z;
                    break;
                }
            case DirectionUtil.Direction.East:
                {
                    newDistance.x = -baseDistance.z;
                    newDistance.z = baseDistance.x;
                    break;
                }
            case DirectionUtil.Direction.West:
                {
                    newDistance.x = baseDistance.z;
                    newDistance.z = -baseDistance.x;
                    break;
                }
        }

        this.SetDistance(newDistance);
    }

    private void Start()
    {
        this.distance = baseSouthDistance;
    }

    private void LateUpdate()
    {
        if (this.target == null)
            return;

        Vector3 destPos = this.target.transform.position + this.distance;
        this.transform.position = Vector3.Lerp(this.transform.position, destPos, this.speed); ;

        Quaternion destRot = Quaternion.LookRotation(this.target.transform.position - this.transform.position, Vector3.up);
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, destRot, this.speed);
    }
}
