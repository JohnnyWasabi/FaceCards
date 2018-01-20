using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Image : MonoBehaviour
{

    private Vector3 pos;

    static Dictionary<string, Texture2D> dictImages = new Dictionary<string, Texture2D>();

    protected Texture2D texture2D;

    protected MeshFilter meshFilter;
    public MeshRenderer meshRenderer;



    void Awake()
    {
        texture2D = LoadPNG(System.IO.Path.Combine(Application.streamingAssetsPath, "Graphics/Default.png"));
    }

    public static Texture2D LoadPNG(string imagePath)
    {
        Texture2D tex = null;

        if (dictImages.TryGetValue(imagePath, out tex))
        {
            return tex;
        }


        byte[] fileData;

        if (File.Exists(imagePath))
        {
            fileData = File.ReadAllBytes(imagePath);
            tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        dictImages.Add(imagePath, tex);
        return tex;
    }

    //    public static T CreateEntity<T>(Vector3 pos) where T : EntityBase

    public static T CreateImage<T>(Transform parent, Texture2D texture2D = null, float width = 1, float height = 1, float u0 = 0, float v0 = 0, float u1 = 1, float v1 = 1) where T : Image
    {

        GameObject plane = new GameObject("Image");
        plane.transform.parent = parent;

        T image = plane.AddComponent(typeof(T)) as T;
        image.meshFilter = (MeshFilter)plane.AddComponent(typeof(MeshFilter));

        if (texture2D == null)
        {
            texture2D = image.texture2D;
            width = texture2D.width;
            height = texture2D.height;
        }

        image.meshFilter.mesh = CreateMesh(width, height, u0, v0, u1, v1);

        image.meshRenderer = plane.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        image.meshRenderer.material.shader = Shader.Find("Sprites/Default");
        image.meshRenderer.receiveShadows = false;
        image.meshRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        image.meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        image.meshRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        image.meshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

        texture2D.filterMode = FilterMode.Point;
        image.meshRenderer.material.mainTexture = texture2D;
        return image;
    }

    public static Mesh CreateMesh(float width, float height, float u0, float v0, float u1, float v1)
    {
        float depth = 0f;
        Mesh mesh = new Mesh();
        mesh.name = "ScriptedMesh";
#if true // centered
        float hWidth = width * 0.5f;
        float hHeight = height * 0.5f;
		mesh.vertices = new Vector3[]
        {
                new Vector3(-hWidth, hHeight, depth), //top left
                new Vector3( hWidth, hHeight, depth), //top right
                new Vector3( hWidth,  -hHeight, depth), //bottom right
                new Vector3(-hWidth,  -hHeight, depth)  //bottom left
        };
#else // top left origin
		mesh.vertices = new Vector3[]
		{
				new Vector3(0, 0, depth), //top left
                new Vector3( width, 0, depth), //top right
                new Vector3( width,  -height, depth), //bottom right
                new Vector3(0, -height, depth)  //bottom left
        };
#endif
		mesh.uv = new Vector2[]
        {
                new Vector2(u0, v1),
                new Vector2(u1, v1),
                new Vector2(u1, v0),
                new Vector2(u0, v0)
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateNormals();

        return mesh;
    }

    public void ChangeImageTo(Texture2D image)
    {

        image.filterMode = FilterMode.Point;
        meshRenderer.material.mainTexture = image;

        float width = image.width * 0.5f;
        float height = image.height * 0.5f;
        float depth = 0f;

        meshFilter.mesh.vertices = new Vector3[]
        {
                new Vector3(-width, height, depth), //top left
                new Vector3( width, height, depth), //top right
                new Vector3( width,  -height, depth), //bottom right
                new Vector3(-width,  -height, depth)  //bottom left
        };

    }

    public void ChangeFrameTo(int frame, int totalFrames, Texture2D image)
    {
        float frameWidth = image.width / totalFrames;

        float width = frameWidth * 0.5f;
        float height = image.height * 0.5f;
        float depth = 0f;

        meshFilter.mesh.vertices = new Vector3[]
        {
                new Vector3(-width, height, depth), //top left
                new Vector3( width, height, depth), //top right
                new Vector3( width,  -height, depth), //bottom right
                new Vector3(-width,  -height, depth)  //bottom left
        };

        float u0 = frameWidth * frame;
        float u1 = (frameWidth * frame) + frameWidth;
        float v0 = 0;
        float v1 = 1;

        u0 = u0 / image.width;
        u1 = u1 / image.width;

        meshFilter.mesh.uv = new Vector2[]
        {
                new Vector2(u0, v1),
                new Vector2(u1, v1),
                new Vector2(u1, v0),
                new Vector2(u0, v0)
        };
        //meshFilter.mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 }; //TODO: Do we need to do this?
        //meshFilter.mesh.RecalculateNormals();
    }

    public void MirrorX(bool isTrue)
    {
        transform.localScale = new Vector3((isTrue ? -1 : 1), transform.localScale.y, 1);
    }
	public void MirrorY(bool isTrue)
	{
		transform.localScale = new Vector3(transform.localScale.x, (isTrue ? -1 : 1), 1);
	}
}
