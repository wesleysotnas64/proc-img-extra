using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [Header("Layout")]
    public TMP_Text txtMouseXPosition;
    public TMP_Text txtMouseYPosition;
    public TMP_Text txtPixelColor;

    [Header("Image Area")]
    public Texture2D baseTexture;
    public Image targetImage;

    [Header("Coordenates")]
    public int pixelPositionX;
    public int pixelPositionY;

    [Header("Brush")]
    public int brushSize;
    public Color brushColor;

    public Slider sldrBrushSize;
    public Slider sldrBrushColorRed;
    public Slider sldrBrushColorGreen;
    public Slider sldrBrushColorBlue;
    public TMP_Text txtBrsuhSize;  
    public TMP_Text txtBrsuhColorRed;
    public TMP_Text txtBrsuhColorGreen;
    public TMP_Text txtBrushColorBlue;

    private RectTransform imageRectTransform;

    private void Start()
    {
        // Obtém o RectTransform da Image
        imageRectTransform = targetImage.GetComponent<RectTransform>();
        baseTexture = CreateWhiteTexture(500, 500);
        brushColor = new Color(0, 0, 0, 1);
        UpdateInterface();
        RenderImage();
    }

    private void Update()
    {
        MousePosition();
        Draw();
        
    }

    public void UpdateInterface()
    {
        brushSize = (int) sldrBrushSize.value;
        txtBrsuhSize.text = brushSize.ToString();

        float redValeu = sldrBrushColorRed.value;
        float greenValeu = sldrBrushColorGreen.value;
        float blueValeu = sldrBrushColorBlue.value;

        brushColor = new Color(redValeu, greenValeu, blueValeu, 1);

        txtBrsuhColorRed.text = brushColor.r.ToString();
        txtBrsuhColorGreen.text = brushColor.g.ToString();
        txtBrushColorBlue.text = brushColor.b.ToString();
    }

    public void Draw()
    {
        if(Input.GetMouseButton(0))
        {
            SetPixelColor(pixelPositionX, pixelPositionY, brushColor, brushSize);   
        }
    }

    void SetPixelColor(int x, int y, Color color, int bSize)
    {
        // Verifique se as coordenadas estão dentro dos limites da textura
        if (x >= 0 && x <= baseTexture.width && y >= 0 && y <= baseTexture.height)
        {
            // Calcule os limites da matriz com base no tamanho do pincel
            int minX = Mathf.Max(0, x - bSize / 2);
            int minY = Mathf.Max(0, y - bSize / 2);
            int maxX = Mathf.Min(baseTexture.width - 1, x + bSize / 2);
            int maxY = Mathf.Min(baseTexture.height - 1, y + bSize / 2);

            // Pinte a matriz ao redor do pixel especificado
            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    baseTexture.SetPixel(i, j, color);
                }
            }

            // Aplique as mudanças
            baseTexture.Apply();
        }
        else
        {
            Debug.LogError("Coordenadas fora dos limites da textura.");
        }
    }

    Texture2D CreateWhiteTexture(int width, int height)
    {
        // Crie uma nova instância de Texture2D
        Texture2D texture = new Texture2D(width, height);

        // Preencha a textura com a cor branca
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }

        // Atribua os pixels à textura
        texture.SetPixels(pixels);

        // Aplique as mudanças
        texture.Apply();

        return texture;
    }

    public void RenderImage()
    {
        if (targetImage != null)
        {
            targetImage.sprite = Sprite.Create(
                    baseTexture,
                    new Rect(0, 0, baseTexture.width, baseTexture.height),
                    Vector2.zero
            );
        }
    }

    private void MousePosition()
    {
        // Obtém a posição do mouse na tela
        Vector3 mousePos = Input.mousePosition;

        // Converte para coordenadas locais da Image
        RectTransformUtility.ScreenPointToLocalPointInRectangle(imageRectTransform, mousePos, Camera.main, out Vector2 localPos);

        // Converte para coordenadas da textura
        Vector2 textureCoord = new Vector2(
            (localPos.x + imageRectTransform.rect.width / 2) / imageRectTransform.rect.width,
            (localPos.y + imageRectTransform.rect.height / 2) / imageRectTransform.rect.height
        );

        // Garante que as coordenadas estejam dentro dos limites da textura
        textureCoord.x = Mathf.Clamp01(textureCoord.x);
        textureCoord.y = Mathf.Clamp01(textureCoord.y);
        Color pixelColor = ((Texture2D)targetImage.mainTexture).GetPixelBilinear(textureCoord.x, textureCoord.y);

        //Atualiza posição do pixel (X e Y)
        pixelPositionX = (int)(textureCoord.x * baseTexture.width);
        pixelPositionY = (int)(textureCoord.y * baseTexture.height);

        // Exibe as informações
        txtMouseXPosition.text = "X: " + pixelPositionX.ToString();
        txtMouseYPosition.text = "Y: " + pixelPositionY.ToString();
        txtPixelColor.text = "Pixel Color: " + $"R: {pixelColor.r:F3}, G: {pixelColor.g:F3}, B: {pixelColor.b:F3}";
    }

}
