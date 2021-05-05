using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour {
    public uint rows = 1;
    public uint targetVerticalResolution = 224;
    public Texture2D[] textures;
    public float[] textureOrder;
    public float[] lineDeformationsHeight;
    public float[] lineDeformationsTime;
    public float[] lineDeformationsCamera;
    public float verticalDeformationCamera;
    public Vector2 baseOffset;
    public Vector2 autoscrollSpeed;
    public Vector2 positionMax;
    public Vector2 positionMin;
    [HideInInspector]
    public Texture2DArray texture2DArray;
    public FilterMode filterMode;
    public TextureWrapMode wrapMode;

    Material material;

    // Start is called before the first frame update
    void Awake() {
        texture2DArray = new Texture2DArray(
            textures[0].width,
            textures[0].height,
            textures.Length,
            UnityEngine.Experimental.Rendering.DefaultFormat.LDR,
            UnityEngine.Experimental.Rendering.TextureCreationFlags.None
        );
        texture2DArray.filterMode = filterMode;
        texture2DArray.wrapMode = wrapMode;

        for (int i = 0; i < textures.Length; i++)
            texture2DArray.SetPixels(textures[i].GetPixels(0), i);

        texture2DArray.Apply();

        material = GetComponent<Renderer>().materials[0];
        material.SetTexture("_Textures", texture2DArray);

        SetParams();
    }

    void Update() { SetParams(); }

    void SetParams() {
        material.SetFloatArray("_TextureOrder", textureOrder);
        material.SetInt("_TextureOrder_Length", textureOrder.Length);
        
        material.SetFloatArray("_LineDeformationsHeight", lineDeformationsHeight);
        material.SetFloatArray("_LineDeformationsTime", lineDeformationsTime);
        material.SetFloatArray("_LineDeformationsCamera", lineDeformationsCamera);

        material.SetFloat("_VerticalDeformationCamera", verticalDeformationCamera);

        material.SetVector("_BaseOffset", baseOffset);
        material.SetVector("_AutoscrollSpeed", autoscrollSpeed);

        material.SetInt("_Rows", (int)rows);

        material.SetVector("_PosMax", positionMax);
        material.SetVector("_PosMin", positionMin);

        material.SetInt("_TargetVerticalResolution", (int)targetVerticalResolution);
    }
}
