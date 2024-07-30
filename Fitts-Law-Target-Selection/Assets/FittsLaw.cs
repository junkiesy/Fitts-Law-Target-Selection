using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class FittsLaw : MonoBehaviour
{
    public Image targetPrefab;
    public Color activeColor = Color.red;
    public Color inactiveColor = Color.gray;
    public Canvas canvas;

    private float startTime;
    private Vector2 startPosition;
    private List<Image> targets = new List<Image>();
    private int currentTargetIndex;
    private int currentConditionIndex;
    private int selectionsCount;
    private List<string> logData = new List<string>();

    private float[] distances = { 0.05f, 0.1f, 0.15f };
    private float[] widths = { 0.005f, 0.01f, 0.015f };
    private List<Vector2> conditions = new List<Vector2>();
    private System.Random random = new System.Random();

    void Start()
    {
        foreach (var distance in distances)
        {
            foreach (var width in widths)
            {
                conditions.Add(new Vector2(distance, width));
            }
        }

        currentConditionIndex = 0;
        selectionsCount = 0;

        CreateTargets();
        ActivateNextTarget();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            if (RectTransformUtility.RectangleContainsScreenPoint(targets[currentTargetIndex].rectTransform, mousePosition, canvas.worldCamera))
            {
                OnTargetClick(true);
            }
            else
            {
                OnTargetClick(false);
            }
        }
    }

    void OnTargetClick(bool isCorrect)
    {
        float endTime = Time.time;
        float movementTime = endTime - startTime;

        var condition = conditions[currentConditionIndex];
        string logEntry = $"Mouse, {condition.y * 2}, {condition.x * 2}, {movementTime}, {(isCorrect ? 1 : 0)}";
        logData.Add(logEntry);
        Debug.Log(logEntry);

        selectionsCount++;
        if (selectionsCount >= 9)
        {
            selectionsCount = 0;
            currentConditionIndex = (currentConditionIndex + 1) % conditions.Count;
        }

        ActivateNextTarget();
    }

    void CreateTargets()
    {
        foreach (var target in targets)
        {
            Destroy(target.gameObject);
        }
        targets.Clear();

        Vector2 condition = conditions[currentConditionIndex];
        float distance = condition.x;
        float width = condition.y;

        for (int i = 0; i < 9; i++)
        {
            Image target = Instantiate(targetPrefab, canvas.transform);
            target.rectTransform.sizeDelta = new Vector2(width * 1000, width * 1000);
            target.color = inactiveColor;

            float angle = i * 40f;
            Vector3 position = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * distance * 1000, Mathf.Sin(angle * Mathf.Deg2Rad) * distance * 1000, 0);
            target.rectTransform.localPosition = position;

            targets.Add(target);
        }
    }

    void ActivateNextTarget()
    {
        if (currentTargetIndex >= 0)
        {
            targets[currentTargetIndex].color = inactiveColor;
        }

        currentTargetIndex = (currentTargetIndex + 4) % targets.Count;
        targets[currentTargetIndex].color = activeColor;

        startTime = Time.time;
        startPosition = Input.mousePosition;
    }

    void OnDestroy()
    {
        string dateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string filePath = Path.Combine(Application.dataPath, "FittsLawExperimentLog.txt");

        logData.Insert(0, $"Experiment run at {dateTime}");

        File.AppendAllLines(filePath, logData);
        Debug.Log("Log saved to: " + filePath);
    }
}
