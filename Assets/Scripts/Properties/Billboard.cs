using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField]
    public Camera targetCamera = null;

    public Vector3 RelativeCameraPosition()
    {
        return this.targetCamera.transform.position - this.transform.position;
    }

    private void Update()
    {
        if (this.targetCamera == null)
        {
            return;
        }

        Vector3 euler = this.transform.rotation.eulerAngles;
        Vector3 lookAt = Quaternion.LookRotation(this.transform.position - this.targetCamera.transform.position).eulerAngles;
        lookAt.x = euler.x;
        lookAt.z = euler.z;
        this.transform.rotation = Quaternion.Euler(lookAt);
    }
}
