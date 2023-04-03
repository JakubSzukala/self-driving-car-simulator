using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoronoiDiagramScript : MonoBehaviour
{
    private RawImage img;
    private int imgSize;


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
        Texture2D texture = new Texture2D(imgSize, imgSize);
        texture.filterMode = FilterMode.Point;
        for (int i = 0; i < imgSize; i++)
        {
            for (int j = 0; j < imgSize; j++)
            {
                float clr = Random.Range(0, 1f);
                texture.SetPixel(i, j, new Color(clr, clr, clr));
            }
        }
        texture.Apply(); // Copy from CPU to GPU memory to render
        img.texture = texture;
    }


    private void GenerateRootPoints()
    {

    }


    private void GenerateUniformRootPoints()
    {
        throw new System.NotImplementedException();
    }
}
