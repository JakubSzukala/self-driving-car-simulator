using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class VoronoiDiagramScript : MonoBehaviour
{
    private RawImage img;
    private int imgSize;
    [SerializeField] private int gridSize = 10;
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
}
