using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ShiftSetup
{
    private const string ShopPath = "Assets/_Project/Scenes/Interior_MobileShop01.unity";
    private const string AlleyPath = "Assets/_Project/Scenes/Prototype_Block01.unity";

    [MenuItem("TehranCity/Setup/13) Shift Minigame + Food Shop")]
    public static void Setup()
    {
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        // ---------- پاساژ: ایستگاه‌ها + کنترلر شیفت ----------
        var shop = OpenAdditive(ShopPath);
        EditorSceneManager.SetActiveScene(shop);

        var counter = FindInActiveScene("Counter");
        var shelf = FindInActiveScene("Shelf_Left");
        if (counter == null || shelf == null)
        {
            Debug.LogError("[TehranCity] Counter/Shelf پیدا نشد؛ اول Setup 12 را اجرا کن.");
            return;
        }

        var reg = counter.GetComponent<ShiftStation>();
        if (reg == null) reg = counter.AddComponent<ShiftStation>();
        reg.kind = ShiftStationKind.Register;
        reg.promptText = "صندوق: ثبت فروش";
        reg.active = false;

        var shf = shelf.GetComponent<ShiftStation>();
        if (shf == null) shf = shelf.AddComponent<ShiftStation>();
        shf.kind = ShiftStationKind.Shelf;
        shf.promptText = "قفسه: چیدن جنس";
        shf.active = false;

        var spot = FindInActiveScene("CustomerSpot");
        if (spot == null)
        {
            spot = new GameObject("CustomerSpot");
            spot.transform.position = new Vector3(2.5f, 0f, 1.5f);
        }

        var ctrlGo = FindInActiveScene("ShiftRoot");
        if (ctrlGo == null) ctrlGo = new GameObject("ShiftRoot");
        var ctrl = ctrlGo.GetComponent<ShiftController>();
        if (ctrl == null) ctrl = ctrlGo.AddComponent<ShiftController>();
        ctrl.stationRegister = reg;
        ctrl.stationShelf = shf;
        ctrl.customerSpot = spot.transform;
        var doorGo = FindInActiveScene("Door_ToAlley");
        ctrl.door = doorGo != null ? doorGo.GetComponent<SceneDoor>() : null;

        var employer = FindInActiveScene("Employer");
        if (employer != null)
        {
            var ec = employer.GetComponent<EmployerDialogueController>();
            if (ec != null) ec.shiftController = ctrl;
        }

        EditorSceneManager.MarkSceneDirty(shop);

        // ---------- کوچه: رفاه‌مارکت قابل خرید ----------
        var alley = OpenAdditive(AlleyPath);
        EditorSceneManager.SetActiveScene(alley);

        var sup = FindInActiveScene("Door_Supermarket");
        if (sup != null)
        {
            foreach (var mb in sup.GetComponents<MonoBehaviour>())
            {
                if (mb != null && mb.GetType().Name.Contains("SimpleInteractable"))
                    Undo.DestroyObjectImmediate(mb);
            }

            if (sup.GetComponent<FoodShopInteractable>() == null)
                sup.AddComponent<FoodShopInteractable>();
        }

        EditorSceneManager.MarkSceneDirty(alley);
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[TehranCity] Shift minigame + food shop ready. Play from Bootstrap!");
    }

    private static Scene OpenAdditive(string path)
    {
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var s = EditorSceneManager.GetSceneAt(i);
            if (s.path == path) return s;
        }
        return EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
    }

    private static GameObject FindInActiveScene(string name)
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            var t = FindInChildren(root.transform, name);
            if (t != null) return t.gameObject;
        }
        return null;
    }

    private static Transform FindInChildren(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform child in t)
        {
            var r = FindInChildren(child, name);
            if (r != null) return r;
        }
        return null;
    }
}