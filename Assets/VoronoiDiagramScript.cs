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
        GenerateDiagram();
    }

    public void GenerateDiagram()
    {
        vModel.GridSize = gridSize;
        vModel.GenerateRootPoints();
        Texture2D targetTexture;
        targetTexture = new Texture2D(mapSize, mapSize);
        targetTexture.filterMode = FilterMode.Point;
        vView.DrawDiagramCells(vModel.RootPoints, vModel.MapSize, ref targetTexture);
        vView.DrawRootPoints(vModel.RootPoints, vModel.MapSize, ref targetTexture, false);
        map.texture = targetTexture;
    }
}


public class VoronoiDiagramModel
{
    public int MapSize
    { get; private set; }
    private int _gridSize;
    public int GridSize
    {
        get { return _gridSize; }
        set
        {
            // pxPerGridSqr is tightly coupled with gridsize and hast to be updated with it
            _gridSize = value;
            pxPerGridSqr = MapSize / GridSize;
        }
    }
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
    public void DrawRootPoints(Vector2Int[,] rootPoints, int textureSize, ref Texture2D texture, bool whiteBg)
    {
        if (whiteBg)
        {
            Color[] whiteBackground = new Color[textureSize * textureSize];
            System.Array.Fill<Color>(whiteBackground, new Color(1f, 1f, 1f));
            texture.SetPixels(0, 0, textureSize, textureSize, whiteBackground);
        }

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


    public void DrawDiagramCells(Vector2Int[, ] rootPoints, int textureSize, ref Texture2D texture)
    {
        int gridSize = rootPoints.GetLength(0);
        int pixelsPerGridSqr = textureSize / gridSize;
        Color[] colors = GenerateRandomColors(gridSize * gridSize);
        // Iterate over each pixel in the texture
        for (int i = 0; i < textureSize; i++)
        {
            for (int j = 0; j < textureSize; j++)
            {
                // Get grid square indexes of current point
                int cgsqrX = i / pixelsPerGridSqr;
                int cgsqrY = j / pixelsPerGridSqr;
                float smallestDistance = Mathf.Infinity;
                Vector2Int closestRootPointGridSqrIdx = new Vector2Int();

                // We only need to check neighboring grid squares for closest root point
                for (int a = -1; a < 2; a++)
                {
                    for (int b = -1; b < 2; b++)
                    {
                        // Neighboring grid square
                        int neighGridIdxX = cgsqrX + a;
                        int neighGridIdxY = cgsqrY + b;
                        if(neighGridIdxX >= 0 && neighGridIdxY >= 0 && neighGridIdxX < gridSize && neighGridIdxY < gridSize)
                        {
                            // Point belonging to that neighboring grid square
                            Vector2Int rootPoint = rootPoints[neighGridIdxX, neighGridIdxY];
                            float distance = Vector2Int.Distance(new Vector2Int(i, j), rootPoint);
                            if(distance < smallestDistance)
                            {
                                smallestDistance = distance;
                                closestRootPointGridSqrIdx = new Vector2Int(neighGridIdxX, neighGridIdxY);
                            }
                        }
                    }
                }
                Color clr = colors[closestRootPointGridSqrIdx.y * gridSize + closestRootPointGridSqrIdx.x];
                texture.SetPixel(i, j, clr);
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