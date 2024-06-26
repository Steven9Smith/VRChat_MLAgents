using UnityEngine;
using Unity.MLAgents;

public class GridSettings : MonoBehaviour
{
    public Camera MainCamera;

    [SerializeField] EnvironmentParameterGridSize gridSize;

    public void Awake()
    {
        gridSize.MainCamera = MainCamera;
        Academy.Instance.EnvironmentParameters.RegisterCallback("gridSize",gridSize);

    }
}
