using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class VoronoiDiagramScript : MonoBehaviour
{
    private RawImage img;
    private int imgSize;
    public int gridSize;
    private int pixelsPerGridSqr;
    private Vector2Int[, ] rootPointsPerGridSqr;
    private Color[] rootPointsColors;


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
        GenerateColors();

        // For conversion between pixel coords and grid square
        pixelsPerGridSqr = (int)(imgSize / gridSize);

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
        GenerateRootPoints();

        AssignMembership(texture);

        // Place a root points on the diagram
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                Vector2Int point = rootPointsPerGridSqr[i, j];
                texture.SetPixel(point.x, point.y, new Color(0f, 0f, 0f));
            }
        }

        texture.Apply(); // Copy from CPU to GPU memory to render
        img.texture = texture;
    }


    private void GenerateRootPoints()
    {
        rootPointsPerGridSqr = new Vector2Int[gridSize, gridSize];
        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                // Indexed by grid squares but coordinates are absolute, not relative to grid square
                rootPointsPerGridSqr[i, j] = new Vector2Int(
                    Random.Range(0, pixelsPerGridSqr) + (i * pixelsPerGridSqr),
                    Random.Range(0, pixelsPerGridSqr) + (j * pixelsPerGridSqr)
                    );
            }
        }
    }


    private void GenerateColors()
    {
        rootPointsColors = new Color[gridSize * gridSize];
        for (int i = 0; i < rootPointsColors.Length; i++)
        {
            rootPointsColors[i] = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        }
    }


    private void AssignMembership(Texture2D texture)
    {
        for (int i = 0; i < imgSize; i++)
        {
            for (int j = 0; j < imgSize; j++)
            {
                // Check which grid square the point is in
                int currPtGridSqrIdxX = (int)(i / pixelsPerGridSqr);
                int currPtGridSqrIdxY = (int)(j / pixelsPerGridSqr);
                float smallestDistance = Mathf.Infinity;
                Vector2Int closestRootPointGridSqrIdx = new Vector2Int();
                for (int a = -1; a < 2; a++)
                {
                    for (int b = -1; b < 2; b++)
                    {
                        int PtGridIdxX = currPtGridSqrIdxX + a;
                        int PtGridIdxY = currPtGridSqrIdxY + b;
                        if(PtGridIdxX >= 0 && PtGridIdxY >= 0 && PtGridIdxX < gridSize && PtGridIdxY < gridSize)
                        {
                            Vector2Int rootPoint = rootPointsPerGridSqr[PtGridIdxX, PtGridIdxY];
                            float distance = Vector2Int.Distance(new Vector2Int(i, j), rootPoint);
                            if(distance < smallestDistance)
                            {
                                smallestDistance = distance;
                                closestRootPointGridSqrIdx = new Vector2Int(PtGridIdxX, PtGridIdxY);
                            }
                        }
                    }
                }
                // Color the point
                Color clr = rootPointsColors[closestRootPointGridSqrIdx.y * gridSize + closestRootPointGridSqrIdx.x];
                texture.SetPixel(i, j, clr);
            }
        }
    }
}
