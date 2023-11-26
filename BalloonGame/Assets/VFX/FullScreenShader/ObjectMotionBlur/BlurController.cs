using UnityEngine;

public class BlurControl : MonoBehaviour
{
    public Material shaderMaterial; //Shader��K�p�������}�e���A��
    public float blurScaleMultiplier = 2f; //���x�ɏ�Z����l
    private Vector3 lastPosition; //�O�t���[���̈ʒu

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
        //���݂̈ʒu
        Vector3 currentPosition = transform.position;

        //���x�̌v�Z
        Vector3 velocity = (currentPosition - lastPosition) / Time.deltaTime;

        //���x�x�N�g���𐳋K�����Đi�s�������擾
        Vector3 blurDirection = velocity.normalized;

        //BlurScale�̌v�Z�i���x�̑傫���ɏ�Z�j
        float blurScale = Mathf.Clamp01(velocity.magnitude ) * blurScaleMultiplier;

        //�}�e���A���Ƀp�����[�^��ݒ�
        shaderMaterial.SetVector("_BlurDiretcion", new Vector4(blurDirection.x, blurDirection.y, blurDirection.z, 0.0f));
        shaderMaterial.SetFloat("_BlurScale", blurScale);

        //�O�t���[���̈ʒu���X�V
        lastPosition = currentPosition;
    }
}
