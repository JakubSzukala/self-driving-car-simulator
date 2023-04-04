using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class VoronoiDiagramScript : MonoBehaviour
{
    // References to model and view
    private VoronoiDiagramModel vModel;
    private VoronoiDiagramView vView;
    private RawImage map;
    private int mapSize;
    public int gridSize = 10;

    private void Awake()
    {
        map = GetComponent<RawImage>();
        mapSize = Mathf.RoundToInt(map.GetComponent<RectTransform>().sizeDelta.x);
        vModel = new VoronoiDiagramModel(mapSize, gridSize);
        vView = new VoronoiDiagramView();
    }


    private void Start()
    {
        vModel.GenerateRootPoints();
        Texture2D targetTexture;
        vView.DrawRootPoints(vModel.RootPoints, vModel.MapSize, out targetTexture);
        map.texture = targetTexture;
    }

    /*
    private RawImage img;
    private int imgSize;
    public int gridSize;
    private int pixelsPerGridSqr;
    private Vector2Int[, ] rootPointsPerGridSqr;


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


    private Color[] GenerateColors()
    {

        Color[] rootPointsColors;
        rootPointsColors = new Color[gridSize * gridSize];
        for (int i = 0; i < rootPointsColors.Length; i++)
        {
            rootPointsColors[i] = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        }
        return rootPointsColors;
    }


    private void AssignMembership(Texture2D texture)
    {

        Color[] rootPointsColors = GenerateColors();
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
    */
}


public class VoronoiDiagramModel
{
    public int MapSize
    { get; private set; }
    public int GridSize
    { get; private set; }
    private int pxPerGridSqr;
    public Vector2Int[,] RootPoints
    { get; private set; }


    public VoronoiDiagramModel(int mapSize, int gridSize)
    {
        this.MapSize = mapSize;
        this.GridSize = gridSize;
        this.pxPerGridSqr = mapSize / gridSize;
    }


    public void GenerateRootPoints()
    {
        RootPoints = new Vector2Int[GridSize, GridSize];
        for (int i = 0; i < GridSize; i++)
        {
            for (int j = 0; j < GridSize; j++)
            {
                // Indexed by grid squares but coordinates are absolute, not relative to grid square
                RootPoints[i, j] = new Vector2Int(
                    Random.Range(0, pxPerGridSqr) + (i * pxPerGridSqr),
                    Random.Range(0, pxPerGridSqr) + (j * pxPerGridSqr)
                    );
            }
        }
    }
}


public class VoronoiDiagramView
{
    public void DrawRootPoints(Vector2Int[,] rootPoints, int textureSize, out Texture2D texture)
    {
        texture = new Texture2D(textureSize, textureSize);
        texture.filterMode = FilterMode.Point;
        Color[] whiteBackground = new Color[textureSize * textureSize]; //{ new Color(1f, 1f, 1f) };
        System.Array.Fill<Color>(whiteBackground, new Color(1f, 1f, 1f));
        texture.SetPixels(0, 0, textureSize, textureSize, whiteBackground);

        // rootPoints are indexed in a form of a grid but their coordinates are absolute
        Color blackPoints = new Color(0f, 0f, 0f);
        for (int i = 0; i < rootPoints.GetLength(0); i++)
        {
            for (int j = 0; j < rootPoints.GetLength(0); j++)
            {
                texture.SetPixel(rootPoints[i, j].x, rootPoints[i, j].y, blackPoints);
            }
        }
        texture.Apply();
    }


    private Color[] GenerateRandomColors(int n)
    {
        Color[] colors;
        colors = new Color[n];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f));
        }
        return colors;
    }
}