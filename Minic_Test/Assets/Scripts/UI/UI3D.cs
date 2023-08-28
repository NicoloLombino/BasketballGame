using UnityEngine;
using UnityEngine.UI;

public class UI3D : MonoBehaviour
{
	private RenderTexture rt = null;
	[SerializeField]
	private Camera renderCamera = null;
	[SerializeField]
	private RawImage cameraCanvas = null;

	void Awake()
	{
		Rect rect = cameraCanvas.rectTransform.rect;
		rt = new RenderTexture((int)rect.width, (int)rect.height, 32);
		renderCamera.targetTexture = rt;
		cameraCanvas.texture = rt;
	}
	void OnDestroy()
	{
		if (rt != null)
		{
			cameraCanvas.texture = null;
			rt.Release();
		}
	}
}
