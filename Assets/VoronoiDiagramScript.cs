using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class VoronoiDiagramScript : MonoBehaviour
{
    private RawImage img;
    private int imgSize;
    private Vector2Int[] rootPoints;

    public uint rootPointsN;

    private void Awake()
    {
        img = GetComponent<RawImage>();
        imgSize = Mathf.RoundToInt(img.GetComponent<RectTransform>().sizeDelta.x);
    }


    void Start()
    {
        GenerateDiagram();
    }


    public void GenerateDiagram()
    {
        // Create a white background
        Texture2D texture = new Texture2D(imgSize, imgSize);
        texture.filterMode = FilterMode.Point;
        for (int i = 0; i < imgSize; i++)
        {
            for (int j = 0; j < imgSize; j++)
            {
                //float clr = Random.Range(0, 1f);
                float clr = 1f;
                texture.SetPixel(i, j, new Color(clr, clr, clr));
            }
        }

        // Generate and place root points
        GenerateRootPoints(rootPointsN);
        foreach(var point in rootPoints)
        {
            texture.SetPixel(point.x, point.y, new Color(0f, 0f, 0f));
        }

        texture.Apply(); // Copy from CPU to GPU memory to render
        img.texture = texture;
    }


    private void GenerateRootPoints(uint n)
    {
        rootPoints = new Vector2Int[n];
        for (int i = 0; i < rootPoints.Length; i++)
        {
            rootPoints[i] = new Vector2Int(Random.Range(0, imgSize), Random.Range(0, imgSize));
        }
    }
}
