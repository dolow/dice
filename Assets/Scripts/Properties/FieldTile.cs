using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(AtlasMaterial))]
public class FieldTile : MonoBehaviour
{
    public void InitCombinedFloor(CombineInstance[] combine, int atlasIndex)
    {
        MeshFilter filter = this.GetComponent<MeshFilter>();
        filter.mesh = new Mesh();
        filter.mesh.CombineMeshes(combine);

        this.gameObject.SetActive(true);
        this.gameObject.isStatic = true;

        AtlasMaterial material = this.gameObject.GetComponent<AtlasMaterial>();
        material.uvTieX = 16;
        material.uvTieY = 16;
        material.initialIndex = atlasIndex;
        material.maxIndex = material.initialIndex;
        material.fps = 0;
    }
}
