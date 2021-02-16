using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Dice : MonoBehaviour
{
    static public int NumberUnknown = -1;
    public delegate void NumberDecided(int number);

    public Vector3 force = Vector3.zero;

    public NumberDecided OnNumberDecided = null;

    private System.Random rand = new System.Random();
    private int stoppedFrames = 0;
    private Vector3 lastPosition = Vector3.zero;
    private Quaternion lastRotation = Quaternion.identity;
    private int numberCache = NumberUnknown;

    public bool IsStable()
    {
        return this.stoppedFrames > 10;
    }

    public void Roll()
    {
        this.numberCache = NumberUnknown;
        this.stoppedFrames = 0;

        Rigidbody rigidbody = this.GetComponent<Rigidbody>();
        rigidbody.AddForce(0.0f, 400.0f, 0.0f);
        Vector3 randomizedForce = this.force;
        randomizedForce.x += (float)this.rand.Next(1, 10);
        randomizedForce.y += (float)this.rand.Next(1, 10);
        randomizedForce.z += (float)this.rand.Next(1, 10);
        rigidbody.AddTorque(randomizedForce);
    }

    public int GetNumber()
    {
        if (!this.IsStable())
        {
            return NumberUnknown;
        }

        if (this.numberCache != NumberUnknown)
        {
            return this.numberCache;
        }

        TextMesh[] textMeshes = this.GetComponentsInChildren<TextMesh>();
        int number = -1;
        TextMesh highest = null;
        for (int i = 0; i < textMeshes.Length; i++)
        {
            TextMesh textMesh = textMeshes[i];
            if (highest == null || highest.transform.position.y < textMesh.transform.position.y)
            {
                highest = textMesh;
                number = Int32.Parse(highest.text);
            }
        }

        this.numberCache = number;

        return number;
    }

    private void Update()
    {
        if (this.transform.position == this.lastPosition && this.transform.rotation == this.lastRotation)
        {
            this.stoppedFrames++;
        }
        else
        {
            this.stoppedFrames = 0;
            this.numberCache = NumberUnknown;
        }

        this.lastPosition = this.transform.position;
        this.lastRotation = this.transform.rotation;

        if (this.numberCache == NumberUnknown)
        {
            int number = this.GetNumber();
            if (number != NumberUnknown)
            {
                this.OnNumberDecided?.Invoke(number);
            }
        }
    }
}
