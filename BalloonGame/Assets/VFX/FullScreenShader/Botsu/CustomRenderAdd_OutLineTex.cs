using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomRenderAdd_OutLineTex : MonoBehaviour
{
    public Material outlineMaterial;
    private RenderTexture renderTexture;

    void Start()
    {
        Camera camera = GetComponent<Camera>();

        // カスタムレンダーテクスチャの作成
        renderTexture = new RenderTexture(Screen.width, Screen.height, 0);

        // カメラにカスタムレンダーテクスチャを設定
        camera.targetTexture = renderTexture;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // カスタムシェーダーを使用して、カスタムレンダーテクスチャに描画する
        Graphics.Blit(source, renderTexture, outlineMaterial);

        // カスタムレンダーテクスチャを画面に描画する
        Graphics.Blit(renderTexture, destination);
    }
}
