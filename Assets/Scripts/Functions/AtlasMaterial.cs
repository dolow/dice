using UnityEngine;

public class AtlasMaterial : MonoBehaviour
{
	public int uvTieX = 1;
	public int uvTieY = 1;
	public int fps = 6;
	public int initialIndex = 0;
	public int maxIndex = 16;

	private Vector2 spriteSheetSize;
	private Renderer rendererRef;
	private int lastIndex = -1;

	private void Start()
	{
		this.spriteSheetSize = new Vector2(1.0f / this.uvTieX, 1.0f / this.uvTieY);
		this.rendererRef = this.GetComponent<Renderer>();

		if (this.rendererRef == null)
			enabled = false;
	}
	
	private void Update()
	{
		int relativeIndex = this.maxIndex - this.initialIndex;
		int index = (relativeIndex == 0)
			? this.initialIndex
			: (int)(Time.timeSinceLevelLoad * this.fps) % relativeIndex + this.initialIndex;

		if (index == this.lastIndex)
			return;
		
		int col = index % this.uvTieX;
		int row = index / this.uvTieY;

		Vector2 offset = new Vector2(col * this.spriteSheetSize.x, 1.0f - this.spriteSheetSize.y - row * this.spriteSheetSize.y);

		this.rendererRef.material.SetTextureOffset("_MainTex", offset);
		this.rendererRef.material.SetTextureScale("_MainTex", this.spriteSheetSize);

		this.lastIndex = index;
	}
	
}
