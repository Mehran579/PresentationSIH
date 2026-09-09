using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.Collections.Generic;

public class NumberSequenceSetup : EditorWindow
{
    [MenuItem("SmritiCare/Setup Number Sequence Game")]
    public static void SetupGame()
    {
        Debug.Log("Starting Number Sequence Game Setup...");

        // 1. Root Object
        GameObject root = new GameObject("NumberSeq_Game");

        // 2. Canvas Setup
        GameObject canvasObj = new GameObject("Canvas");
        canvasObj.transform.SetParent(root.transform);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        canvasObj.AddComponent<GraphicRaycaster>();

        // 3. Sequence Row
        GameObject seqRow = new GameObject("SequenceRow");
        seqRow.transform.SetParent(canvasObj.transform);

        RectTransform seqRect = seqRow.GetComponent<RectTransform>();
        if (seqRect == null) seqRect = seqRow.AddComponent<RectTransform>();
        seqRect.sizeDelta = new Vector2(1080, 300);
        seqRect.anchoredPosition = new Vector2(0, 500);

        HorizontalLayoutGroup seqLayout = seqRow.AddComponent<HorizontalLayoutGroup>();
        seqLayout.childControlWidth = true;
        seqLayout.childControlHeight = true;
        seqLayout.childForceExpandWidth = true;
        seqLayout.childForceExpandHeight = true;

        NumberDigitDisplay[] seqDisplays = new NumberDigitDisplay[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject tile = new GameObject("NumberTile_" + i);
            tile.transform.SetParent(seqRow.transform);

            RectTransform tileRect = tile.GetComponent<RectTransform>() ?? tile.AddComponent<RectTransform>();

            GameObject row = new GameObject("DigitRow");
            row.transform.SetParent(tile.transform);

            RectTransform rowRect = row.GetComponent<RectTransform>() ?? row.AddComponent<RectTransform>();
            HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;

            NumberDigitDisplay display = tile.AddComponent<NumberDigitDisplay>();

            var field = typeof(NumberDigitDisplay).GetField("digitParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(display, row.transform);

            seqDisplays[i] = display;
        }

        // 4. Answer Row
        GameObject ansRow = new GameObject("AnswerRow");
        ansRow.transform.SetParent(canvasObj.transform);

        RectTransform ansRect = ansRow.GetComponent<RectTransform>();
        if (ansRect == null) ansRect = ansRow.AddComponent<RectTransform>();
        ansRect.sizeDelta = new Vector2(1080, 300);
        ansRect.anchoredPosition = new Vector2(0, -500);

        HorizontalLayoutGroup ansLayout = ansRow.AddComponent<HorizontalLayoutGroup>();
        ansLayout.childControlWidth = true;
        ansLayout.childControlHeight = true;
        ansLayout.childForceExpandWidth = true;
        ansLayout.childForceExpandHeight = true;

        AnswerTile[] ansTiles = new AnswerTile[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject btn = new GameObject("AnswerButton_" + i);
            btn.transform.SetParent(ansRow.transform);
            btn.AddComponent<Image>();

            RectTransform btnRect = btn.GetComponent<RectTransform>() ?? btn.AddComponent<RectTransform>();

            GameObject row = new GameObject("DigitRow");
            row.transform.SetParent(btn.transform);

            RectTransform rowRect = row.GetComponent<RectTransform>() ?? row.AddComponent<RectTransform>();
            HorizontalLayoutGroup rowLayout = row.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childAlignment = TextAnchor.MiddleCenter;

            NumberDigitDisplay display = btn.AddComponent<NumberDigitDisplay>();
            var field = typeof(NumberDigitDisplay).GetField("digitParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null) field.SetValue(display, row.transform);

            AnswerTile tile = btn.AddComponent<AnswerTile>();
            var tileField = typeof(AnswerTile).GetField("display", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (tileField != null) tileField.SetValue(tile, display);

            ansTiles[i] = tile;
        }

        // 5. Controller
        NumberSequenceController controller = root.AddComponent<NumberSequenceController>();

        var fieldSeq = typeof(NumberSequenceController).GetField("sequenceTiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fieldSeq != null) fieldSeq.SetValue(controller, seqDisplays);

        var fieldAns = typeof(NumberSequenceController).GetField("answerTiles", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (fieldAns != null) fieldAns.SetValue(controller, ansTiles);

        Debug.Log("Number Sequence Game successfully built via Editor script!");
    }
}