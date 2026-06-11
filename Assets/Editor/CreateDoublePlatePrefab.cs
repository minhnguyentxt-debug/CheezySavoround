using UnityEngine;
using UnityEditor;

public class CreateDoublePlatePrefab : EditorWindow
{
    [MenuItem("Tools/CheezySavoround/Create Double Plate Prefab")]
    public static void CreatePrefab()
    {
        string singlePlatePath = "Assets/Prefab/Plate_Base_Prefab.prefab";
        string doublePlateModelPath = "Assets/Art/Object/Models/DoublePlate.fbx";
        string outputPrefabPath = "Assets/Prefab/DoublePlate_Prefab.prefab";

        GameObject singlePlatePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(singlePlatePath);
        GameObject doublePlateModel = AssetDatabase.LoadAssetAtPath<GameObject>(doublePlateModelPath);

        if (singlePlatePrefab == null)
        {
            Debug.LogError($"[CreateDoublePlatePrefab] Không tìm thấy Single Plate Prefab tại: {singlePlatePath}");
            return;
        }

        if (doublePlateModel == null)
        {
            Debug.LogError($"[CreateDoublePlatePrefab] Không tìm thấy Double Plate FBX tại: {doublePlateModelPath}");
            return;
        }

        // Tạo GameObject cha tạm thời
        GameObject rootObj = new GameObject("DoublePlate_Prefab");
        DoublePlate doublePlateScript = rootObj.AddComponent<DoublePlate>();

        // Thêm BoxCollider cho đối tượng cha bao phủ toàn bộ diện tích đĩa đôi dọc (Z axis)
        BoxCollider parentCollider = rootObj.AddComponent<BoxCollider>();
        parentCollider.center = Vector3.zero;
        parentCollider.size = new Vector3(3.2f, 0.5f, 7.0f);

        // Instatiate Model đĩa đôi dài làm con để hiển thị
        GameObject modelInstance = (GameObject)PrefabUtility.InstantiatePrefab(doublePlateModel);
        modelInstance.transform.SetParent(rootObj.transform);
        modelInstance.transform.localPosition = Vector3.zero;
        modelInstance.transform.localRotation = Quaternion.identity;
        modelInstance.transform.localScale = Vector3.one;
        modelInstance.name = "DoublePlate_Visual";

        // Instatiate 2 đĩa con làm con của đĩa đôi (dọc theo trục Z vì model đĩa dài nằm dọc mặc định)
        GameObject plate1Obj = (GameObject)PrefabUtility.InstantiatePrefab(singlePlatePrefab);
        plate1Obj.transform.SetParent(rootObj.transform);
        plate1Obj.transform.localPosition = new Vector3(0f, 0f, -1.75f); // Đối xứng dọc theo trục Z
        plate1Obj.transform.localRotation = Quaternion.identity;
        plate1Obj.name = "PizzaPlate_1";

        GameObject plate2Obj = (GameObject)PrefabUtility.InstantiatePrefab(singlePlatePrefab);
        plate2Obj.transform.SetParent(rootObj.transform);
        plate2Obj.transform.localPosition = new Vector3(0f, 0f, 1.75f); // Đối xứng dọc theo trục Z, sẽ được cập nhật lại theo Grid ở runtime
        plate2Obj.transform.localRotation = Quaternion.identity;
        plate2Obj.name = "PizzaPlate_2";

        doublePlateScript.plate1 = plate1Obj.GetComponent<PizzaPlate>();
        doublePlateScript.plate2 = plate2Obj.GetComponent<PizzaPlate>();

        // Lưu thành Prefab
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(rootObj, outputPrefabPath);
        if (prefabAsset != null)
        {
            Debug.Log($"<color=green>[CreateDoublePlatePrefab] Đã tạo thành công Prefab đĩa đôi tại: {outputPrefabPath}</color>");
            
            // Tự động tìm DockManager trong Scene hiện tại để gán tham chiếu
            DockManager dockManager = Object.FindAnyObjectByType<DockManager>();
            if (dockManager != null)
            {
                SerializedObject so = new SerializedObject(dockManager);
                SerializedProperty prop = so.FindProperty("doublePlatePrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = prefabAsset;
                    so.ApplyModifiedProperties();
                    EditorUtility.SetDirty(dockManager); // Đánh dấu scene có thay đổi để Save
                    Debug.Log("<color=green>[CreateDoublePlatePrefab] Đã tự động gán Prefab đĩa đôi vào DockManager trong Scene!</color>");
                }
            }

            // Highlight asset vừa tạo trong Project Window
            EditorGUIUtility.PingObject(prefabAsset);
        }
        else
        {
            Debug.LogError($"[CreateDoublePlatePrefab] Không thể lưu Prefab tại: {outputPrefabPath}");
        }

        // Dọn dẹp đối tượng tạm trong Scene
        DestroyImmediate(rootObj);
    }
}
