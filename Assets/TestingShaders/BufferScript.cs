using UnityEngine;
using UnityEngine.Rendering;

public class BufferScript : MonoBehaviour {

	[SerializeField]
	private Material material;

	[SerializeField, Range(10, 200)]
	private int width = 100;

	[SerializeField, Range(10, 200)]
	private int height = 100;


	private void Awake()
	{

		int lowResRenderTarget = Shader.PropertyToID ("_LowResRenderTarget");



		CommandBuffer cb = new CommandBuffer ();

		cb.GetTemporaryRT (lowResRenderTarget, this.width, this.height, 0, FilterMode.Trilinear, RenderTextureFormat.ARGB32);


		cb.Blit (lowResRenderTarget, lowResRenderTarget, this.material);

		cb.Blit (lowResRenderTarget, BuiltinRenderTextureType.CameraTarget);

		cb.ReleaseTemporaryRT (lowResRenderTarget);

		this.GetComponent<Camera>().AddCommandBuffer(CameraEvent.BeforeForwardOpaque, cb);

	}
}
