using UnityEngine;

public class Static : MonoBehaviour
{
    public Vector3 position = Vector3.zero;
    public Vector3 rotation = Vector3.zero;
    public float speed = 1.0f;

    private void LateUpdate()
    {
        this.transform.position = Vector3.Lerp(this.transform.position, this.position, this.speed); ;
        this.transform.rotation = Quaternion.Lerp(this.transform.rotation, Quaternion.Euler(this.rotation), this.speed);
    }
}
