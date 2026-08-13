using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public static class MobileShopSetup
{
    private const string PrototypeScenePath = "Assets/_Project/Scenes/Prototype_Block01.unity";
    private const string ShopScenePath = "Assets/_Project/Scenes/Interior_MobileShop01.unity";
    private const string DialogueAssetPath = "Assets/_Project/Data/ScriptableObjects/Dialogue/InterviewDialogue.asset";

    [MenuItem("TehranCity/Setup/12 Mobile Shop + Interview")]
    public static void SetupAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EnsureSceneInBuild(PrototypeScenePath);
        EnsureSceneInBuild(ShopScenePath);

        var dialogue = EnsureDialogueAsset();

        var prototypeScene = OpenSceneAdditive(PrototypeScenePath);
        if (prototypeScene.IsValid())
        {
            EditorSceneManager.SetActiveScene(prototypeScene);
            LinkAlleyDoor();
            EditorSceneManager.MarkSceneDirty(prototypeScene);
        }

        var shopScene = OpenSceneAdditive(ShopScenePath);
        if (shopScene.IsValid())
        {
            EditorSceneManager.SetActiveScene(shopScene);
            BuildShop(dialogue);
            EditorSceneManager.MarkSceneDirty(shopScene);
            EditorSceneManager.SetActiveScene(shopScene);
        }

        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[TehranCity] Mobile Shop + Interview setup finished.");
    }

    private static InterviewDialogueSO EnsureDialogueAsset()
    {
        var asset = AssetDatabase.LoadAssetAtPath<InterviewDialogueSO>(DialogueAssetPath);

        if (asset == null)
        {
            EnsureFolder(Path.GetDirectoryName(DialogueAssetPath));
            asset = ScriptableObject.CreateInstance<InterviewDialogueSO>();
            AssetDatabase.CreateAsset(asset, DialogueAssetPath);
        }

        asset.employerName = "کارفرما";
        asset.employerLine = "قبلاً کار کردی؟ اینجا کار شوخی نیست. یه شیفت آزمایشی، اگه خراب نکنی ادامه میدیم.";
        asset.acceptLabel = "قبول می‌کنم";
        asset.rejectLabel = "بعداً فکر می‌کنم";
        asset.acceptedLine = "خوبه. فعلاً از همین امروز شروع کن. صندوق و قفسه رو تمیز نگه دار.";
        asset.rejectedLine = "باشه. اگه پشیمون شدی، تا آخر هفته وقت داری.";
        asset.jobAcceptedFlag = GameFlags.JobAccepted;

        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

        return asset;
    }

    private static void LinkAlleyDoor()
    {
        var door = FindInActiveScene("Door_Passage");
        if (door == null)
            door = FindInActiveSceneContains("Door_Passage");
        if (door == null)
            door = FindInActiveSceneContains("Passage");

        if (door == null)
        {
            Debug.LogError("[TehranCity] Door_Passage not found in Prototype_Block01.");
            return;
        }

        var spawn = FindInActiveScene("Spawn_MobileShop_Outside");
        if (spawn == null)
        {
            spawn = new GameObject("Spawn_MobileShop_Outside");
            Undo.RegisterCreatedObjectUndo(spawn, "Create mobile shop outside spawn");
            spawn.transform.position = door.transform.position - door.transform.forward * 2f;
        }

        // نشاندن Spawn روی زمین (تعمیر نمونه‌های شناور قبلی)
        if (Physics.Raycast(spawn.transform.position + Vector3.up * 5f, Vector3.down, out var groundHit, 20f, ~0, QueryTriggerInteraction.Ignore))
        {
            spawn.transform.position = new Vector3(spawn.transform.position.x, groundHit.point.y + 0.05f, spawn.transform.position.z);
        }

        var sceneDoor = EnsureSceneDoor(door);
        ConfigureSceneDoor(sceneDoor, "Interior_MobileShop01", "Spawn_MobileShop_Inside", "ورود به پاساژ");
    }

    private static void BuildShop(InterviewDialogueSO dialogue)
    {
        var existing = GetRootObject("MobileShopRoot");
        if (existing != null)
            Undo.DestroyObjectImmediate(existing);

        var root = new GameObject("MobileShopRoot");
        Undo.RegisterCreatedObjectUndo(root, "Build mobile shop");

        // Floor
        var floor = CreateCube(root.transform, "Floor", new Vector3(0f, -0.5f, 0f), new Vector3(14f, 1f, 10f));
        if (floor.GetComponent<Collider>() == null)
            floor.AddComponent<BoxCollider>();

        // Walls
        CreateCube(root.transform, "Wall_Back", new Vector3(0f, 1.5f, -5f), new Vector3(14f, 3f, 0.2f));
        CreateCube(root.transform, "Wall_Left", new Vector3(-7f, 1.5f, 0f), new Vector3(0.2f, 3f, 10f));
        CreateCube(root.transform, "Wall_Right", new Vector3(7f, 1.5f, 0f), new Vector3(0.2f, 3f, 10f));

        CreateCube(root.transform, "Wall_Front_Left", new Vector3(-4f, 1.5f, 5f), new Vector3(6f, 3f, 0.2f));
        CreateCube(root.transform, "Wall_Front_Right", new Vector3(4f, 1.5f, 5f), new Vector3(6f, 3f, 0.2f));
        CreateCube(root.transform, "Wall_Front_Top", new Vector3(0f, 2.5f, 5f), new Vector3(2f, 1f, 0.2f));

        // Counter
        CreateCube(root.transform, "Counter", new Vector3(0f, 0.5f, -1.2f), new Vector3(3.2f, 1f, 0.7f));

        // Shelves
        CreateCube(root.transform, "Shelf_Left", new Vector3(-4f, 0.75f, -2f), new Vector3(2f, 1.5f, 0.7f));
        CreateCube(root.transform, "Shelf_Right", new Vector3(4f, 0.75f, -2f), new Vector3(2f, 1.5f, 0.7f));
        CreateCube(root.transform, "Shelf_Back", new Vector3(0f, 0.75f, -4.2f), new Vector3(4f, 1.5f, 0.6f));

        // Spawn inside
        var spawnInside = new GameObject("Spawn_MobileShop_Inside");
        spawnInside.transform.SetParent(root.transform, false);
        spawnInside.transform.localPosition = new Vector3(0f, 0f, 3.6f);

        // Door to alley
        // در خروج به کوچه + متریال در + تابلوی راهنما
        var door = CreateCube(root.transform, "Door_ToAlley", new Vector3(0f, 1f, 5f), new Vector3(2f, 2f, 0.2f), true);

        var doorMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Environment/Narmak/Materials/MAT_Alley_Door.mat");
        var doorRenderer = door.GetComponent<Renderer>();
        if (doorRenderer != null && doorMat != null)
            doorRenderer.sharedMaterial = doorMat;

        // تابلوی خروج (قانون سه‌سطحی: RTLTextMeshPro3D + fontSize 4.2 + والد بدون Scale + چرخش صفر)
        if (root.transform.Find("Sign_ShopExit_Text") == null)
        {
            var tgo = new GameObject("Sign_ShopExit_Text");
            tgo.transform.SetParent(root.transform, false);
            tgo.transform.localPosition = new Vector3(0f, 2.55f, 4.8f); // بالای در، سمت داخل
            tgo.transform.localRotation = Quaternion.identity;
            tgo.transform.localScale = Vector3.one;

            var tmp = tgo.AddComponent<RTLTMPro.RTLTextMeshPro3D>();
            tmp.font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>("Assets/_Project/Art/UI/Fonts/TMP_Tahoma_Persian.asset");
            tmp.fontSize = 4.2f;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.text = "خروج به کوچه";
        }

        var doorSceneDoor = EnsureSceneDoor(door);
        ConfigureSceneDoor(doorSceneDoor, "Prototype_Block01", "Spawn_MobileShop_Outside", "خروج به کوچه");

        // Employer
        var employer = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Undo.RegisterCreatedObjectUndo(employer, "Create employer");
        employer.name = "Employer";
        employer.transform.SetParent(root.transform, false);
        employer.transform.localPosition = new Vector3(0f, 1f, -2.3f);

        var employerCollider = employer.GetComponent<Collider>();
        if (employerCollider != null)
            employerCollider.isTrigger = true;

        // Dialogue camera
        var cameraController = CreateDialogueCamera(root.transform, new Vector3(0f, 1.5f, -2.2f), new Vector3(0f, 0.55f, 3.4f));

        var employerController = employer.AddComponent<EmployerDialogueController>();
        employerController.dialogue = dialogue;
        employerController.dialogueCamera = cameraController;

        EnsurePlayer();
        EnsureLight();
    }

    private static void EnsurePlayer()
    {
        var player = FindInActiveScene("Player");

        if (player == null)
        {
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            Undo.RegisterCreatedObjectUndo(player, "Create shop player");
            player.name = "Player";

            var spawn = FindInActiveScene("Spawn_MobileShop_Inside");
            if (spawn != null)
                player.transform.position = spawn.transform.position + Vector3.up * 1f;
        }

        var strayRb = player.GetComponent<Rigidbody>();
        if (strayRb != null) Undo.DestroyObjectImmediate(strayRb);

        var strayCol = player.GetComponent<CapsuleCollider>();
        if (strayCol != null) Undo.DestroyObjectImmediate(strayCol);

        var insideSpawn = FindInActiveScene("Spawn_MobileShop_Inside");
        if (insideSpawn != null)
            player.transform.position = insideSpawn.transform.position + Vector3.up * 1f;

        // 1) تگ — دوربین با FindWithTag("Player") بازیکن را پیدا می‌کند
        player.tag = "Player";

        // 2) کولایدر — CharacterController جای CapsuleCollider/Rigidbody می‌نشیند
        var capsuleCollider = player.GetComponent<CapsuleCollider>();
        if (capsuleCollider != null)
            Undo.DestroyObjectImmediate(capsuleCollider);

        var rigidbody = player.GetComponent<Rigidbody>();
        if (rigidbody != null)
            Undo.DestroyObjectImmediate(rigidbody);

        var cc = player.GetComponent<CharacterController>();
        if (cc == null)
            cc = Undo.AddComponent<CharacterController>(player);

        cc.center = Vector3.zero;
        cc.height = 2f;
        cc.radius = 0.4f;
        cc.stepOffset = 0.3f;

        // 3) کامپوننت‌های منطقی — معادل Player واقعی صحنه‌های دیگر
        if (player.GetComponent<PlayerController>() == null)
            Undo.AddComponent<PlayerController>(player);

        if (player.GetComponent<Interactor>() == null)
            Undo.AddComponent<Interactor>(player);

        // ظاهر یکسان با Player اتاق/کوچه: متریال نارنجی + بین
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Environment/Narmak/Materials/MAT_Greybox_Player.mat");

        var renderer = player.GetComponent<Renderer>();
        if (renderer != null && mat != null)
            renderer.sharedMaterial = mat;

        if (player.transform.Find("FacingNose") == null)
        {
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(nose, "Create facing nose");
            nose.name = "FacingNose";

            var noseCol = nose.GetComponent<Collider>();
            if (noseCol != null)
                Undo.DestroyObjectImmediate(noseCol);

            nose.transform.SetParent(player.transform);
            nose.transform.localPosition = new Vector3(0f, 0.35f, 0.45f);
            nose.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);

            var noseRenderer = nose.GetComponent<Renderer>();
            if (noseRenderer != null && mat != null)
                noseRenderer.sharedMaterial = mat;
        }

        Debug.Log("[TehranCity] Shop Player ensured: tag=Player + PlayerController + Interactor + CharacterController.");
    }

    private static void EnsureLight()
    {
        if (ActiveSceneHasComponent<Light>())
            return;

        var lightGO = new GameObject("Directional Light");
        Undo.RegisterCreatedObjectUndo(lightGO, "Create shop light");

        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 30f);
    }

    private static bool ActiveSceneHasComponent<T>() where T : Component
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.GetComponentInChildren<T>(true) != null)
                return true;
        }

        return false;
    }

    private static DialogueCameraController CreateDialogueCamera(Transform parent, Vector3 pivotLocalPos, Vector3 offset)
    {
        var pivot = new GameObject("DialogueCameraPivot");
        Undo.RegisterCreatedObjectUndo(pivot, "Create dialogue camera pivot");
        pivot.transform.SetParent(parent, false);
        pivot.transform.localPosition = pivotLocalPos;

        var camGO = new GameObject("DialogueCamera");
        Undo.RegisterCreatedObjectUndo(camGO, "Create dialogue camera");
        camGO.transform.SetParent(pivot.transform, false);
        camGO.transform.localPosition = offset;
        camGO.transform.localRotation = Quaternion.LookRotation(pivot.transform.position - camGO.transform.position, Vector3.up);

        var cam = Undo.AddComponent<CinemachineCamera>(camGO);
        cam.Priority = -1000;

        var follow = Undo.AddComponent<CinemachineFollow>(camGO);
        SetMember(follow, new[] { "FollowTarget", "Target", "Follow" }, pivot.transform);
        SetMember(follow, new[] { "Offset", "FollowOffset", "TrackingOffset" }, offset);

        TryAddAim(camGO, pivot.transform);

        var controller = Undo.AddComponent<DialogueCameraController>(pivot);
        controller.dialogueCamera = cam;

        camGO.SetActive(false);
        return controller;
    }

    private static void TryAddAim(GameObject go, Transform target)
    {
        var aimType = FindTypeByName("CinemachineHardLookToTarget")
                      ?? FindTypeByName("CinemachineHardLookAtTarget")
                      ?? FindTypeByName("CinemachineRotationComposer");

        if (aimType == null)
            return;

        var comp = Undo.AddComponent(go, aimType);
        SetMember(comp, new[] { "LookAtTarget", "Target", "LookAt" }, target);
    }

    private static GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 scale, bool trigger = false)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Undo.RegisterCreatedObjectUndo(go, "Create greybox cube");

        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPosition;
        go.transform.localScale = scale;

        var collider = go.GetComponent<Collider>();
        if (collider != null)
            collider.isTrigger = trigger;

        return go;
    }

    private static Component EnsureSceneDoor(GameObject go)
    {
        var type = FindTypeByName("SceneDoor");
        if (type == null)
        {
            Debug.LogError("[TehranCity] SceneDoor type not found.");
            return null;
        }

        var comp = go.GetComponent(type);
        if (comp == null)
            comp = Undo.AddComponent(go, type);

        foreach (var mb in go.GetComponents<MonoBehaviour>())
        {
            if (mb != null && mb.GetType().Name.Contains("SimpleInteractable"))
                Undo.DestroyObjectImmediate(mb);
        }

        return comp;
    }

    private static void ConfigureSceneDoor(Component door, string sceneName, string spawnName, string prompt)
    {
        if (door == null)
            return;

        TryInvoke(door, new[] { "Configure", "Setup", "Initialize", "SetDestination" }, sceneName, spawnName);

        var so = new SerializedObject(door);

        SetSerializedString(so, new[]
        {
            "sceneToLoad", "sceneName", "targetScene", "destinationScene", "loadScene"
        }, sceneName);

        SetSerializedString(so, new[]
        {
            "spawnPointName", "spawnPoint", "destinationSpawn", "targetSpawn", "spawnName"
        }, spawnName);

        SetSerializedString(so, new[]
        {
            "prompt", "interactionPrompt", "Prompt", "promptText", "PromptText"
        }, prompt);

        so.ApplyModifiedProperties();

        SetMember(door, new[]
        {
            "sceneToLoad", "sceneName", "targetScene", "destinationScene", "loadScene"
        }, sceneName);

        SetMember(door, new[]
        {
            "spawnPointName", "spawnPoint", "destinationSpawn", "targetSpawn", "spawnName"
        }, spawnName);

        SetMember(door, new[]
        {
            "prompt", "interactionPrompt", "Prompt", "promptText", "PromptText"
        }, prompt);
    }

    private static bool SetSerializedString(SerializedObject so, string[] propertyNames, string value)
    {
        foreach (var propertyName in propertyNames)
        {
            var prop = so.FindProperty(propertyName);
            if (prop != null && prop.propertyType == SerializedPropertyType.String)
            {
                prop.stringValue = value;
                return true;
            }
        }

        return false;
    }

    private static void SetMember(object target, string[] memberNames, object value)
    {
        if (target == null)
            return;

        var type = target.GetType();

        foreach (var memberName in memberNames)
        {
            var prop = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null && prop.CanWrite)
            {
                try
                {
                    if (prop.PropertyType.IsInstanceOfType(value) || prop.PropertyType == typeof(object))
                    {
                        prop.SetValue(target, value);
                        return;
                    }

                    if (prop.PropertyType == typeof(string) && value != null)
                    {
                        prop.SetValue(target, value.ToString());
                        return;
                    }
                }
                catch
                {
                    // ignore
                }
            }

            var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                try
                {
                    if (field.FieldType.IsInstanceOfType(value) || field.FieldType == typeof(object))
                    {
                        field.SetValue(target, value);
                        return;
                    }

                    if (field.FieldType == typeof(string) && value != null)
                    {
                        field.SetValue(target, value.ToString());
                        return;
                    }
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    private static bool TryInvoke(object target, string[] methodNames, params object[] args)
    {
        if (target == null)
            return false;

        var methods = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => methodNames.Contains(m.Name) && m.GetParameters().Length == args.Length && !m.IsGenericMethod);

        foreach (var method in methods)
        {
            var parameters = method.GetParameters();
            var converted = new object[args.Length];
            var ok = true;

            for (int i = 0; i < parameters.Length; i++)
            {
                var arg = args[i];
                var parameterType = parameters[i].ParameterType;

                if (arg == null)
                {
                    if (parameterType.IsValueType && Nullable.GetUnderlyingType(parameterType) == null)
                    {
                        ok = false;
                        break;
                    }

                    converted[i] = null;
                    continue;
                }

                if (parameterType.IsInstanceOfType(arg) || parameterType == typeof(object))
                {
                    converted[i] = arg;
                    continue;
                }

                if (parameterType == typeof(string))
                {
                    converted[i] = arg.ToString();
                    continue;
                }

                ok = false;
                break;
            }

            if (!ok)
                continue;

            try
            {
                method.Invoke(target, converted);
                return true;
            }
            catch
            {
                // ignore
            }
        }

        return false;
    }

    private static GameObject GetRootObject(string name)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == name)
                return root;
        }

        return null;
    }

    private static GameObject FindInActiveScene(string name)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var result = FindInChildren(root.transform, name, false);
            if (result != null)
                return result.gameObject;
        }

        return null;
    }

    private static GameObject FindInActiveSceneContains(string namePart)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var result = FindInChildren(root.transform, namePart, true);
            if (result != null)
                return result.gameObject;
        }

        return null;
    }

    private static Transform FindInChildren(Transform transform, string name, bool contains)
    {
        if (contains ? transform.name.Contains(name) : transform.name == name)
            return transform;

        foreach (Transform child in transform)
        {
            var result = FindInChildren(child, name, contains);
            if (result != null)
                return result;
        }

        return null;
    }

    private static Scene OpenSceneAdditive(string path)
    {
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (scene.path == path)
                return scene;
        }

        if (!File.Exists(path))
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);
            EditorSceneManager.SaveScene(scene, path);
            return scene;
        }

        return EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
    }

    private static void EnsureSceneInBuild(string path)
    {
        if (!File.Exists(path))
            return;

        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Any(s => s.path == path && s.enabled))
        {
            scenes.Add(new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }

    private static void EnsureFolder(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath))
            return;

        folderPath = folderPath.Replace("\\", "/");

        if (folderPath == "Assets" || AssetDatabase.IsValidFolder(folderPath))
            return;

        var parent = Path.GetDirectoryName(folderPath);
        if (!string.IsNullOrEmpty(parent))
        {
            parent = parent.Replace("\\", "/");
            EnsureFolder(parent);
        }

        var folderName = Path.GetFileName(folderPath);
        if (!string.IsNullOrEmpty(folderName))
            AssetDatabase.CreateFolder(parent, folderName);
    }

    private static Type FindTypeByName(string typeName)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                var type = asm.GetTypes().FirstOrDefault(t => t != null && t.Name == typeName);
                if (type != null)
                    return type;
            }
            catch (ReflectionTypeLoadException e)
            {
                var type = e.Types.FirstOrDefault(t => t != null && t.Name == typeName);
                if (type != null)
                    return type;
            }
            catch
            {
                // ignore
            }
        }

        return null;
    }
}