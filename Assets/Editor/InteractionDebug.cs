using TMPro;
using UnityEditor;
using UnityEngine;

public static class InteractionDebug
{
    [MenuItem("TehranCity/Debug/Validate Interaction Setup")]
    public static void Validate()
    {
        var player = GameObject.Find("Player");
        Debug.Log(player == null ? "[Validate] Player: MISSING"
            : $"[Validate] Player.Interactor: {(player.GetComponent<Interactor>() != null ? "OK" : "MISSING")}");

        foreach (var n in new[] { "Interact_Phone", "Mirror" })
        {
            var go = GameObject.Find(n);
            Debug.Log(go == null ? $"[Validate] {n}: MISSING"
                : $"[Validate] {n}.SimpleInteractable: {(go.GetComponent<SimpleInteractable>() != null ? "OK" : "MISSING")}");
        }

        var canvas = GameObject.Find("UI_Canvas");
        Debug.Log(canvas == null ? "[Validate] UI_Canvas: MISSING" : "[Validate] UI_Canvas: OK");
        if (canvas != null)
        {
            var ui = canvas.GetComponent<InteractionUI>();
            Debug.Log(ui == null ? "[Validate] InteractionUI: MISSING" : "[Validate] InteractionUI: OK");
            if (ui != null)
            {
                var so = new SerializedObject(ui);
                Debug.Log($"[Validate] promptText assigned: {so.FindProperty("promptText").objectReferenceValue != null}");
                Debug.Log($"[Validate] toastText assigned: {so.FindProperty("toastText").objectReferenceValue != null}");
            }
        }

        Debug.Log($"[Validate] Persian Font: {(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/UI/Fonts/TMP_Tahoma_Persian.asset") != null ? "OK" : "MISSING (Setup 7 را اجرا کن)")}");
    }
}