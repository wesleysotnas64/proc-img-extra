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

    private RectTransform imageRectTransform;

    private void Start()
    {
        // Obtém o RectTransform da Image
        imageRectTransform = targetImage.GetComponent<RectTransform>();
        RenderImage();
    }

    private void Update()
    {
        MousePosition();

        // Verifica se o botão do mouse foi pressionado
        if (Input.GetMouseButtonDown(0))
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

            // Obtém a cor do pixel clicado
            Color targetColor = ((Texture2D)targetImage.mainTexture).GetPixelBilinear(textureCoord.x, textureCoord.y);

            // Preenche a área com cores semelhantes adjacentes para branco
            FloodFill(textureCoord, targetColor, Color.white);
        }
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

        // Exibe as informações
        txtMouseXPosition.text = "X: " + ((int)(textureCoord.x * baseTexture.width)).ToString();
        txtMouseYPosition.text = "Y: " + ((int)(textureCoord.y * baseTexture.height)).ToString();
        txtPixelColor.text = "Pixel Color: " + $"R: {pixelColor.r:F3}, G: {pixelColor.g:F3}, B: {pixelColor.b:F3}";
    }

    private void FloodFill(Vector2 textureCoord, Color targetColor, Color fillColor)
    {
        Texture2D texture = (Texture2D)targetImage.mainTexture;
        int width = texture.width;
        int height = texture.height;

        // Converte as coordenadas da textura para pixels
        int startX = Mathf.RoundToInt(textureCoord.x * width);
        int startY = Mathf.RoundToInt(textureCoord.y * height);

        // Obtém a cor do pixel inicial
        Color startColor = texture.GetPixel(startX, startY);

        // Se a cor inicial for a mesma que a cor de destino, não há necessidade de preenchimento
        if (startColor == fillColor || startColor != targetColor)
        {
            return;
        }

        // Usado para armazenar os pixels a serem verificados
        Queue<Vector2Int> pixelsToCheck = new Queue<Vector2Int>();

        // Adiciona o pixel inicial à fila
        pixelsToCheck.Enqueue(new Vector2Int(startX, startY));

        while (pixelsToCheck.Count > 0)
        {
            // Retira o próximo pixel da fila
            Vector2Int currentPixel = pixelsToCheck.Dequeue();

            // Obtém as coordenadas do pixel
            int x = currentPixel.x;
            int y = currentPixel.y;

            // Verifica se o pixel está dentro dos limites da textura
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                // Obtém a cor do pixel
                Color currentColor = texture.GetPixel(x, y);

                // Se a cor do pixel for a mesma que a cor inicial, preenche com a cor de destino
                if (currentColor == targetColor)
                {
                    texture.SetPixel(x, y, fillColor);

                    // Adiciona os pixels adjacentes à fila
                    pixelsToCheck.Enqueue(new Vector2Int(x + 1, y));
                    pixelsToCheck.Enqueue(new Vector2Int(x - 1, y));
                    pixelsToCheck.Enqueue(new Vector2Int(x, y + 1));
                    pixelsToCheck.Enqueue(new Vector2Int(x, y - 1));
                }
            }
        }

        // Aplica as alterações na textura
        texture.Apply();
    }
}
