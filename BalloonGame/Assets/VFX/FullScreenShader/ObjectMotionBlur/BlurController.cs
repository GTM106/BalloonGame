using UnityEngine;

public class BlurControl : MonoBehaviour
{
    public Material shaderMaterial; //Shaderを適用したいマテリアル
    public float blurScaleMultiplier = 2f; //速度に乗算する値
    private Vector3 lastPosition; //前フレームの位置

    void Start()
    {
        if (shaderMaterial == null)
        {
            Debug.LogError("Shader Material is not assigned.");
            return;
        }

        lastPosition = transform.position;
    }

    void Update()
    {
        //現在の位置
        Vector3 currentPosition = transform.position;

        //速度の計算
        Vector3 velocity = (currentPosition - lastPosition) / Time.deltaTime;

        //速度ベクトルを正規化して進行方向を取得
        Vector3 blurDirection = velocity.normalized;

        //BlurScaleの計算（速度の大きさに乗算）
        float blurScale = Mathf.Clamp01(velocity.magnitude ) * blurScaleMultiplier;

        //マテリアルにパラメータを設定
        shaderMaterial.SetVector("_BlurDiretcion", new Vector4(blurDirection.x, blurDirection.y, blurDirection.z, 0.0f));
        shaderMaterial.SetFloat("_BlurScale", blurScale);

        //前フレームの位置を更新
        lastPosition = currentPosition;
    }
}
