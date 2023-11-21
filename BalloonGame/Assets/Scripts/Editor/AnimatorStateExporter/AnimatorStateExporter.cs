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
        GUILayout.Label("Animator Controller 内のステート名を指定のスクリプタブルオブジェクトに格納します", EditorStyles.helpBox);

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
            Debug.LogError("Animator Controllerがnullです。\nステート名を読み取りたいAnimatorControllerをセットしてください");
            return;
        }
        if (stateData == null)
        {
            const string outputPath = "Assets/StateData";

            //すでに同名のアセットがあったらそれを上書き保存する
            //こうすることで既にアタッチされているものがMissingにならない
            if (File.Exists($"{outputPath}/{controller.name}StateData.asset"))
            {
                stateData = AssetDatabase.LoadAssetAtPath<AnimatorStateData>($"{outputPath}/{controller.name}StateData.asset");
            }
            else
            {
                Debug.LogWarning($"Animator State Dataがnullです。\n{outputPath}に自動で生成します");

                //ディレクトリがなかったら作る
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }

                stateData = CreateInstance<AnimatorStateData>();
                AssetDatabase.CreateAsset(stateData, $"{outputPath}/{controller.name}StateData.asset");
            }
        }

        //初期化
        stateNames = "";
        stateData.stateNames.Clear();

        //アニメーションステートを取得するためのループ
        foreach (AnimatorControllerLayer layer in controller.layers)
        {
            foreach (ChildAnimatorState state in layer.stateMachine.states)
            {
                AnimatorState animatorState = state.state;
                string stateName = animatorState.name;

                //取得した名前をスクリプタブルオブジェクトに格納
                stateData.stateNames.Add(stateName);

                //enum生成用に一括管理
                //空白があると自動生成したスクリプトがコンパイルエラーを起こすため、空白を消す。
                //enumに使用できない文字には対応していませんが、使用されないという前提です。
                stateNames += stateName.Replace(" ", "") + ", ";
            }
        }

        EditorUtility.SetDirty(stateData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Export完了しました");
    }

    private void GanarateDataBaseTemplete(string dataClassName)
    {
        //クラス名の最初にenumを示すE_を追加
        dataClassName = "E_" + dataClassName;

        string localPath = "Assets/Scripts/Animation/Enum";

        //ディレクトリがなかったら作る
        if (!Directory.Exists(localPath))
        {
            Directory.CreateDirectory(localPath);
        }

        //テンプレートを読み込む
        string templateText = File.ReadAllText("Assets/Scripts/Editor/AnimatorStateExporter/BaseEnum.txt");
        if (string.IsNullOrEmpty(templateText))
        {
            Debug.LogError("Script template not found. Make sure you have a 'DataBaseScriptTemplate.txt' file in a 'Resources' folder.");
            return;
        }

        //テンプレートに適用する
        string classContent = templateText.Replace("#EnumName#", dataClassName).Replace("#VALUE#", stateNames);

        //ファイルの生成
        string filePath = localPath + $"/{dataClassName}.cs";

        System.IO.File.WriteAllText(filePath, classContent);
        AssetDatabase.Refresh();

        Debug.Log($"Class '{dataClassName}' generated successfully at '{filePath}'.");
    }
}
