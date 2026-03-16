#if UNITY_EDITOR

using System.Linq;
using _Scripts.Core;
using _Scripts.Views;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    [CustomEditor(typeof(GraphRuntimeBridge))]
    public class GraphRuntimeBridgeEditor : UnityEditor.Editor
    {
        private GraphRuntimeBridge _bridge;

        private bool _showBlocks = true;
        private bool _showConnections = true;

        private bool _showAddConn = false;
        private int _fromIdx = 0;
        private string _fromPort = "FlowOut";
        private int _toIdx = 0;
        private string _toPort = "FlowIn";

        private static readonly string[] BlockTypes =
        {
            "SpawnObject", "MoveObject", "RotateObject", "ScaleObject",
            "Variable", "Compare", "Branch", "Log"
        };

        private void OnEnable()
        {
            _bridge = (GraphRuntimeBridge)target;
            EditorApplication.update += RepaintIfPlaying;
        }

        private void OnDisable()
        {
            EditorApplication.update -= RepaintIfPlaying;
        }

        private void RepaintIfPlaying()
        {
            if (Application.isPlaying) Repaint();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space(4);

            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
                { fontSize = 14, alignment = TextAnchor.MiddleCenter };
            EditorGUILayout.LabelField("⬡  Node Graph  ⬡", titleStyle);
            EditorGUILayout.Space(6);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter Play Mode to interact with the graph.\n" +
                    "Blocks, connections and controls appear here at runtime.",
                    MessageType.Info);
                return;
            }

            DrawStatusBar();
            EditorGUILayout.Space(4);

            DrawActionButtons();
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawBlocksPanel();
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            DrawConnectionsPanel();
        }


        private void DrawStatusBar()
        {
            var bgColor = _bridge.IsRunning
                ? new Color(1f, 0.85f, 0.3f) // yellow while running
                : new Color(0.3f, 0.85f, 0.3f); // green when idle

            var statusStyle = new GUIStyle(EditorStyles.helpBox)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 11,
                fontStyle = FontStyle.Bold
            };

            var prevBg = GUI.backgroundColor;
            GUI.backgroundColor = bgColor;

            string label = _bridge.IsRunning
                ? $"⏳  {_bridge.StatusMessage}"
                : $"✔  {_bridge.StatusMessage}";

            EditorGUILayout.LabelField(label, statusStyle, GUILayout.Height(24));
            GUI.backgroundColor = prevBg;

            EditorGUILayout.BeginHorizontal();
            DrawPill($"Blocks: {_bridge.BlockCount}", new Color(0.5f, 0.7f, 1f));
            DrawPill($"Connections: {_bridge.ConnectionCount}", new Color(0.7f, 0.5f, 1f));
            DrawPill($"Objects: {_bridge.RuntimeObjectCount}", new Color(0.5f, 1f, 0.7f));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawPill(string text, Color color)
        {
            var style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUILayout.Box(text, style, GUILayout.ExpandWidth(true), GUILayout.Height(18));
            GUI.backgroundColor = prev;
        }

        private void DrawActionButtons()
        {
            var view = FindGraphView();
            if (view == null)
            {
                EditorGUILayout.HelpBox("GraphView not found in scene.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = _bridge.IsRunning
                ? Color.grey
                : new Color(0.4f, 0.9f, 0.4f);
            if (GUILayout.Button("▶  Run Graph", GUILayout.Height(30)) && !_bridge.IsRunning)
                view.RequestRun();

            GUI.backgroundColor = new Color(1f, 0.45f, 0.45f);
            if (GUILayout.Button("🗑  Clear Scene", GUILayout.Height(30)))
                view.RequestClear();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            GUI.backgroundColor = new Color(0.45f, 0.75f, 1f);
            if (GUILayout.Button("⬇  Export JSON", GUILayout.Height(24)))
                view.RequestExport();

            GUI.backgroundColor = new Color(1f, 0.85f, 0.4f);
            if (GUILayout.Button("⬆  Import JSON", GUILayout.Height(24)))
                view.RequestImport();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Add Block:", EditorStyles.boldLabel);

            int cols = 4;
            for (int i = 0; i < BlockTypes.Length; i += cols)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = i; j < Mathf.Min(i + cols, BlockTypes.Length); j++)
                    if (GUILayout.Button(BlockTypes[j]))
                        view.RequestAddBlock(BlockTypes[j]);
                EditorGUILayout.EndHorizontal();
            }
        }

        private void DrawBlocksPanel()
        {
            var snapshots = _bridge.BlockSnapshots;

            _showBlocks = EditorGUILayout.Foldout(
                _showBlocks,
                $"Blocks  ({snapshots.Count})",
                true, EditorStyles.foldoutHeader);

            if (!_showBlocks) return;

            var view = FindGraphView();

            if (snapshots.Count == 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("No blocks yet.", EditorStyles.centeredGreyMiniLabel);
                EditorGUI.indentLevel--;
                return;
            }

            EditorGUI.indentLevel++;
            foreach (var block in snapshots)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                // Type pill
                var typeStyle = new GUIStyle(EditorStyles.miniLabel)
                    { fontStyle = FontStyle.Bold };
                var prevColor = GUI.contentColor;
                GUI.contentColor = GetBlockColor(block.type);
                GUILayout.Label($"[{block.type}]", typeStyle, GUILayout.Width(100));
                GUI.contentColor = prevColor;

                // Name
                EditorGUILayout.LabelField(block.gameObjectName, GUILayout.ExpandWidth(true));

                // ID (mini)
                EditorGUILayout.LabelField($"#{block.id}", EditorStyles.miniLabel, GUILayout.Width(64));

                // Remove button
                if (view != null)
                {
                    GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                    if (GUILayout.Button("✕", GUILayout.Width(22)))
                        view.RequestRemoveBlock(block.id);
                    GUI.backgroundColor = Color.white;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel--;
        }

        private void DrawConnectionsPanel()
        {
            var snapshots = _bridge.ConnectionSnapshots;

            _showConnections = EditorGUILayout.Foldout(
                _showConnections,
                $"Connections  ({snapshots.Count})",
                true, EditorStyles.foldoutHeader);

            if (!_showConnections) return;

            var view = FindGraphView();

            if (snapshots.Count == 0)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("No connections yet.", EditorStyles.centeredGreyMiniLabel);
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUI.indentLevel++;
                for (int i = 0; i < snapshots.Count; i++)
                {
                    var conn = snapshots[i];
                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

                    // FROM side
                    var fromStyle = new GUIStyle(EditorStyles.miniLabel) { fontStyle = FontStyle.Bold };
                    GUI.contentColor = new Color(0.5f, 1f, 0.7f);
                    GUILayout.Label(conn.from, fromStyle, GUILayout.ExpandWidth(true));
                    GUI.contentColor = Color.white;

                    // Arrow
                    GUILayout.Label("→", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(18));

                    // TO side
                    GUI.contentColor = new Color(0.5f, 0.8f, 1f);
                    GUILayout.Label(conn.to, fromStyle, GUILayout.ExpandWidth(true));
                    GUI.contentColor = Color.white;

                    // Remove button
                    if (view != null)
                    {
                        var idx = i; // capture for lambda
                        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
                        if (GUILayout.Button("✕", GUILayout.Width(22)))
                            view.RequestRemoveConnection(idx);
                        GUI.backgroundColor = Color.white;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4);
            _showAddConn = EditorGUILayout.Foldout(_showAddConn, "➕  Add Connection", true);
            if (_showAddConn) DrawAddConnectionPanel();
        }


        private void DrawAddConnectionPanel()
        {
            var blocks = _bridge.BlockSnapshots;
            if (blocks.Count == 0)
            {
                EditorGUILayout.HelpBox("Add blocks first.", MessageType.Info);
                return;
            }

            var names = blocks.Select(b => $"{b.gameObjectName}  [{b.type}]").ToArray();
            var sceneBlocks = GetSceneBlocks();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("FROM", EditorStyles.boldLabel);
            _fromIdx = EditorGUILayout.Popup("Block", Mathf.Clamp(_fromIdx, 0, names.Length - 1), names);
            _fromPort = EditorGUILayout.TextField("Port", _fromPort);
            DrawPortHints(blocks, sceneBlocks, _fromIdx, isOutput: true);

            EditorGUILayout.Space(4);

            EditorGUILayout.LabelField("TO", EditorStyles.boldLabel);
            _toIdx = EditorGUILayout.Popup("Block", Mathf.Clamp(_toIdx, 0, names.Length - 1), names);
            _toPort = EditorGUILayout.TextField("Port", _toPort);
            DrawPortHints(blocks, sceneBlocks, _toIdx, isOutput: false);

            EditorGUILayout.Space(4);

            GUI.backgroundColor = new Color(0.4f, 0.9f, 0.4f);
            if (GUILayout.Button("Connect"))
            {
                var view = FindGraphView();
                if (view != null && _fromIdx < blocks.Count && _toIdx < blocks.Count)
                {
                    view.RequestAddConnection(
                        blocks[_fromIdx].id, _fromPort,
                        blocks[_toIdx].id, _toPort);
                }
            }

            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndVertical();
        }

        private void DrawPortHints(
            System.Collections.Generic.IReadOnlyList<GraphRuntimeBridge.BlockSnapshot> snapshots,
            BaseBlock[] sceneBlocks,
            int idx,
            bool isOutput)
        {
            if (idx >= snapshots.Count) return;
            var id = snapshots[idx].id;
            var block = sceneBlocks.FirstOrDefault(b => b.blockId == id);
            if (block == null) return;

            var portNames = string.Join("   ", block.GetPortDefinitions()
                .Where(p => p.isOutput == isOutput)
                .Select(p => p.name));

            var label = isOutput ? $"out: {portNames}" : $"in:  {portNames}";
            EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
        }


        private GraphView FindGraphView() =>
            Object.FindObjectOfType<GraphView>();

        private BaseBlock[] GetSceneBlocks() =>
            Object.FindObjectsOfType<BaseBlock>();

        private Color GetBlockColor(string blockType) => blockType switch
        {
            "SpawnObject" => new Color(1f, 0.8f, 0.4f),
            "MoveObject" => new Color(0.4f, 1f, 0.6f),
            "RotateObject" => new Color(0.4f, 0.8f, 1f),
            "ScaleObject" => new Color(0.8f, 0.4f, 1f),
            "Variable" => new Color(1f, 1f, 0.4f),
            "Compare" => new Color(1f, 0.6f, 0.4f),
            "Branch" => new Color(0.4f, 1f, 1f),
            "Log" => new Color(0.7f, 0.7f, 0.7f),
            _ => Color.white
        };
    }
}
#endif