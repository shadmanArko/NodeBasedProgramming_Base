using UnityEngine;

namespace _Scripts.Config
{
    [CreateAssetMenu(fileName = "NodeGraphConfig", menuName = "NodeGraph/Config", order = 0)]
    public class NodeGraphConfig : ScriptableObject
    {
        [Header("Serialization")] [Tooltip("Default path for JSON export/import.")]
        public string defaultJsonPath = "Assets/graph_export.json";

        [Header("Execution")] [Tooltip("Destroy previously spawned objects before each run.")]
        public bool clearOnRun = true;

        [Tooltip("Reset VariableBlock values to their initial values before each run.")]
        public bool resetVariablesOnRun = true;

        [Header("Block Spawning")] [Tooltip("Local-space Y offset when a new block GameObject is created.")]
        public float blockSpawnOffsetY = 0f;

        [Tooltip("Maximum allowed graph execution depth to prevent infinite loops.")]
        public int maxExecutionDepth = 256;

        [Header("Logging")] [Tooltip("Log each block execution to the Unity Console.")]
        public bool verboseLogging = true;
    }
}