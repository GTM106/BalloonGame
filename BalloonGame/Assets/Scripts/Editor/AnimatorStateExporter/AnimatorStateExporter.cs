using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

public class AnimatorStateExporter : EditorWindow
{
    [MenuItem("Window/Animator State Exporter")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorStateExporter>("Animator State Exporter");
    }

    AnimatorController controller;
    AnimatorStateData stateData;
    string stateNames;

    private void OnGUI()
    {
        GUILayout.Label("Animator Controller ���̃X�e�[�g�����w��̃X�N���v�^�u���I�u�W�F�N�g�Ɋi�[���܂�", EditorStyles.helpBox);

        controller = EditorGUILayout.ObjectField("Animator Controller", controller, typeof(AnimatorController), false) as AnimatorController;
        stateData = EditorGUILayout.ObjectField("Animator State Data", stateData, typeof(AnimatorStateData), false) as AnimatorStateData;

        GUILayout.Space(10);

        if (GUILayout.Button("Export States"))
        {
            ExportStates(controller, stateData);
            GanarateDataBaseTemplete(controller.name);
        }
    }

    private void ExportStates(AnimatorController controller, AnimatorStateData stateData)
    {
        if (controller == null)
        {
            Debug.LogError("Animator Controller��null�ł��B\n�X�e�[�g����ǂݎ�肽��AnimatorController���Z�b�g���Ă�������");
            return;
        }
        if (stateData == null)
        {
            const string outputPath = "Assets/StateData";

            //���łɓ����̃A�Z�b�g���������炻����㏑���ۑ�����
            //�������邱�ƂŊ��ɃA�^�b�`����Ă�����̂�Missing�ɂȂ�Ȃ�
            if (File.Exists($"{outputPath}/{controller.name}StateData.asset"))
            {
                stateData = AssetDatabase.LoadAssetAtPath<AnimatorStateData>($"{outputPath}/{controller.name}StateData.asset");
            }
            else
            {
                Debug.LogWarning($"Animator State Data��null�ł��B\n{outputPath}�Ɏ����Ő������܂�");

                //�f�B���N�g�����Ȃ���������
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                stateData = CreateInstance<AnimatorStateData>();
                AssetDatabase.CreateAsset(stateData, $"{outputPath}/{controller.name}StateData.asset");
            }
        }

        //������
        stateNames = "";
        stateData.stateNames.Clear();

        //�A�j���[�V�����X�e�[�g���擾���邽�߂̃��[�v
        foreach (AnimatorControllerLayer layer in controller.layers)
        {
            foreach (ChildAnimatorState state in layer.stateMachine.states)
            {
                AnimatorState animatorState = state.state;
                string stateName = animatorState.name;

                //�擾�������O���X�N���v�^�u���I�u�W�F�N�g�Ɋi�[
                stateData.stateNames.Add(stateName);

                //enum�����p�Ɉꊇ�Ǘ�
                //�󔒂�����Ǝ������������X�N���v�g���R���p�C���G���[���N�������߁A�󔒂������B
                //enum�Ɏg�p�ł��Ȃ������ɂ͑Ή����Ă��܂��񂪁A�g�p����Ȃ��Ƃ����O��ł��B
                stateNames += stateName.Replace(" ", "") + ", ";
            }
        }

        EditorUtility.SetDirty(stateData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Export�������܂���");
    }

    private void GanarateDataBaseTemplete(string dataClassName)
    {
        //�N���X���̍ŏ���enum������E_��ǉ�
        dataClassName = "E_" + dataClassName;

        string localPath = "Assets/Scripts/Animation/Enum";

        //�f�B���N�g�����Ȃ���������
        if (!Directory.Exists(localPath))
        {
            Directory.CreateDirectory(localPath);
        }

        //�e���v���[�g��ǂݍ���
        string templateText = File.ReadAllText("Assets/Scripts/Editor/AnimatorStateExporter/BaseEnum.txt");
        if (string.IsNullOrEmpty(templateText))
        {
            Debug.LogError("Script template not found. Make sure you have a 'DataBaseScriptTemplate.txt' file in a 'Resources' folder.");
            return;
        }

        //�e���v���[�g�ɓK�p����
        string classContent = templateText.Replace("#EnumName#", dataClassName).Replace("#VALUE#", stateNames);

        //�t�@�C���̐���
        string filePath = localPath + $"/{dataClassName}.cs";

        System.IO.File.WriteAllText(filePath, classContent);
        AssetDatabase.Refresh();

        Debug.Log($"Class '{dataClassName}' generated successfully at '{filePath}'.");
    }
}
