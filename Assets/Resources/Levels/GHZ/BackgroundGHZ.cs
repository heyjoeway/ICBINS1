using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundGHZ : Background {
    static Texture2D[] textures;
    static Material material;

    bool _initStaticDone = false;
    void InitStatic() {
        if (_initStaticDone) return;

        material = Resources.Load<Material>("Levels/GHZ/Palette Cycle");

        textures = new Texture2D[] {
            Resources.Load<Texture2D>("Levels/GHZ/Tiles/BG/48"), // 0
            Resources.Load<Texture2D>("Levels/GHZ/Tiles/BG/57"), // 1
            Resources.Load<Texture2D>("Levels/GHZ/Tiles/BG/58"), // 2
            Resources.Load<Texture2D>("Levels/GHZ/Tiles/BG/59"), // 3
            Resources.Load<Texture2D>("Levels/GHZ/Tiles/BG/61"), // 4
            Resources.Load<Texture2D>("Levels/GHZ/Tiles/BG/62"), // 5
            Resources.Load<Texture2D>("Levels/GHZ/Tiles/BG/63")  // 6
        };

        _initStaticDone = true;
    }

    static int[] layout = new int[] { // by texture id from `textures`
        5,
        4,
        2,
        6,
        0,
        1,
        3,
        4,
        2,
        0,
        5,
        1,
        4,
        3,
        3,
        4,
        5,
        2,
        6,
        5,
        2,
        6,
        0,
        1,
        3,
        4,
        2,
        0,
        5,
        1,
        4,
        3
    };

    static int[] lineHeights = new int[] { // in pixels
        32,
        16,
        16,
        48,
        40,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1,
        1, 1, 1, 1, 1, 1, 1, 1
    };

    static float[] lineDeformationTime = new float[] { // in-game units / second
        -1,
        -0.5F,
        -0.25F,
        0,
        0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0
    };

    static float[] lineDeformationCamera = new float[] { // percentage of in-game units
        -0.4375F,
        -0.4375F,
        -0.4375F,
        -0.375F,
        -0.5F,
        -0.5F, 
        -0.5036057692307693F, -0.5072115384615384F, -0.5108173076923077F, 
        -0.514423076923077F, -0.5180288461538461F, -0.5216346153846154F, 
        -0.5252403846153846F, -0.5288461538461539F, -0.532451923076923F, 
        -0.5360576923076923F, -0.5396634615384616F, -0.5432692307692307F, 
        -0.546875F, -0.5504807692307693F, -0.5540865384615385F, 
        -0.5576923076923077F, -0.5612980769230769F, -0.5649038461538461F, 
        -0.5685096153846154F, -0.5721153846153846F, -0.5757211538461539F, 
        -0.5793269230769231F, -0.5829326923076923F, -0.5865384615384615F, 
        -0.5901442307692307F, -0.59375F, -0.5973557692307693F, 
        -0.6009615384615385F, -0.6045673076923077F, -0.6081730769230769F, 
        -0.6117788461538461F, -0.6153846153846154F, -0.6189903846153846F, 
        -0.6225961538461539F, -0.6262019230769231F, -0.6298076923076923F, 
        -0.6334134615384615F, -0.6370192307692307F, -0.640625F, 
        -0.6442307692307693F, -0.6478365384615385F, -0.6514423076923077F, 
        -0.6550480769230769F, -0.6586538461538461F, -0.6622596153846154F, 
        -0.6658653846153846F, -0.6694711538461539F, -0.6730769230769231F, 
        -0.6766826923076923F, -0.6802884615384615F, -0.6838942307692307F, 
        -0.6875F, -0.6911057692307692F, -0.6947115384615385F, 
        -0.6983173076923077F, -0.7019230769230769F, -0.7055288461538461F, 
        -0.7091346153846154F, -0.7127403846153846F, -0.7163461538461539F, 
        -0.7199519230769231F, -0.7235576923076923F, -0.7271634615384615F, 
        -0.7307692307692308F, -0.734375F, -0.7379807692307692F, 
        -0.7415865384615385F, -0.7451923076923077F, -0.7487980769230769F, 
        -0.7524038461538461F, -0.7560096153846154F, -0.7596153846153846F, 
        -0.7632211538461539F, -0.7668269230769231F, -0.7704326923076923F, 
        -0.7740384615384615F, -0.7776442307692308F, -0.78125F, 
        -0.7848557692307692F, -0.7884615384615385F, -0.7920673076923077F, 
        -0.7956730769230769F, -0.7992788461538461F, -0.8028846153846154F, 
        -0.8064903846153846F, -0.8100961538461539F, -0.8137019230769231F, 
        -0.8173076923076923F, -0.8209134615384615F, -0.8245192307692308F, 
        -0.828125F, -0.8317307692307692F, -0.8353365384615385F, 
        -0.8389423076923077F, -0.8425480769230769F, -0.8461538461538461F, 
        -0.8497596153846154F, -0.8533653846153846F, -0.8569711538461539F, 
        -0.8605769230769231F, -0.8641826923076923F, -0.8677884615384615F, 
        -0.8713942307692308F, -0.875F
    };

    const float deformationSpeedVertical = 0.015625F;
    const int layoutPaddingCount = 3;

    List<Transform> lines; 
    Transform linesTransform;

    float GetTotalLineHeight(int lineIndex) {
        float totalLineHeight = 0;
        for (int totalLineIndex = 0; totalLineIndex < lineIndex; totalLineIndex++)
            totalLineHeight += lineHeights[totalLineIndex];
        return totalLineHeight;
    }

    void AddPieceToLine(Transform line, int layoutIndex, int lineIndex) {
        AddPieceToLine(line, layoutIndex, lineIndex, Vector2.zero);
    }

    void AddPieceToLine(Transform line, int layoutIndex, int lineIndex, Vector2 positionOffset) {
        float totalLineHeight = GetTotalLineHeight(lineIndex);
        float lineHeight = lineHeights[lineIndex];

        GameObject chunk = new GameObject();
        chunk.transform.parent = line;
        chunk.transform.localPosition = positionOffset + new Vector2(
            layoutIndex * 256 / 32F,
            0
        );

        int textureIndex = layout[layoutIndex];
        Texture2D texture = textures[textureIndex];
        Sprite chunkSprite = Sprite.Create(
            texture,
            new Rect(0, (256 - totalLineHeight) - lineHeight, 256, lineHeight),
            Vector2.zero,
            32
        );

        SpriteRenderer spriteRenderer = chunk.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = chunkSprite;
        spriteRenderer.material = material;

        // Hack?
        material.SetFloat(
            "_Lerp",
            GlobalOptions.Get<bool>("linearInterpolation") ? 1 : 0
        );
    }

    public override void Start() {
        base.Start();
        InitStatic();

        linesTransform = transform.Find("Lines");
        lines = new List<Transform>();
        
        int totalLineHeight = 0;

        for(int lineIndex = 0; lineIndex < lineHeights.Length; lineIndex++) {
            int lineHeight = lineHeights[lineIndex];

            Transform line = (new GameObject()).transform;
            lines.Add(line);
            line.parent = linesTransform;
            line.localPosition = new Vector3(
                0,
                (-totalLineHeight - lineHeight) / 32F,
                0
            );

            for (int layoutIndex = 0; layoutIndex < layout.Length; layoutIndex++)
                AddPieceToLine(line, layoutIndex, lineIndex);

            for (int layoutIndex = 0; layoutIndex < layoutPaddingCount; layoutIndex++)
                AddPieceToLine(line, layoutIndex, lineIndex, new Vector2(
                    layout.Length * 256 / 32F,
                    0
                ));

            totalLineHeight += lineHeight;
        }
    }

    // Update is called once per frame
    public override void Update() {
        base.Update();

        for(int lineIndex = 0; lineIndex < lines.Count; lineIndex++) {
            Transform line = lines[lineIndex];
            Vector3 linePosition = line.localPosition;
            linePosition.x = (
                (Time.time * lineDeformationTime[lineIndex]) +
                (targetPosition.x * lineDeformationCamera[lineIndex])
            );
            linePosition.x %= layout.Length * 8F;
            line.localPosition = linePosition;
        }

        Vector3 linesPosition = linesTransform.position;
        linesPosition.y = (
            (
                targetPosition.y *
                deformationSpeedVertical
            ) +
            3.75F
        );
        linesTransform.position = linesPosition;
    }
}