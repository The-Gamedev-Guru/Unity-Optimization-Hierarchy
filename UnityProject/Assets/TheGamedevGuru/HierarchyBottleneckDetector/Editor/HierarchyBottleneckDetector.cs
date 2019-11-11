// Copyright 2019 The Gamedev Guru (http://thegamedev.guru)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
// ReSharper disable PossibleInvalidOperationException

namespace TheGamedevGuru
{
public class HierarchyBottleneckDetector : EditorWindow
{
    private int _score;
    private Transform _maxDepthChild;
    private int _maxDepthCount;
    
    private GameObject _maxObjectRoot;
    private int _maxObjectCount;
    
    [MenuItem("TheGamedevGuru/FAP Hierarchy Tool")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        var window = (HierarchyBottleneckDetector)EditorWindow.GetWindow(typeof(HierarchyBottleneckDetector), true, "Gamedev Guru's Hierarchy Bottleneck Detector");
        window.Show();
    }

    void OnGUI()
    {
        EditorGUILayout.Separator();
        GUILayout.Label("Find Bottlenecks in your Hierarchy", EditorStyles.boldLabel);
        EditorGUILayout.Separator();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel($"Longest path child:");
        EditorGUILayout.ObjectField(_maxDepthChild, typeof(Transform), true);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField($"Longest path depth: " + _maxDepthCount);
        
        EditorGUILayout.Separator();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel($"Max object count root:");
        EditorGUILayout.ObjectField(_maxObjectRoot, typeof(GameObject), true);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField($"Max object child count: " + _maxObjectCount);

        EditorGUILayout.Separator();

        if (GUILayout.Button("Calculate your Score"))
        {
            var maxObjectCountRoot =
                Enumerable
                    .Range(0, SceneManager.sceneCount)
                    .Select(SceneManager.GetSceneAt)
                    .SelectMany(scene => scene.GetRootGameObjects())
                    .OrderByDescending(go => GetTotalObjectCount(go.transform))
                    .First();

            _maxObjectCount = GetTotalObjectCount(maxObjectCountRoot.transform);
        
            _maxObjectRoot =
                Enumerable
                    .Range(0, SceneManager.sceneCount)
                    .Select(SceneManager.GetSceneAt)
                    .SelectMany(scene => scene.GetRootGameObjects())
                    .OrderByDescending(go => GetLongestPath(go.transform, 0).Depth)
                    .First();

            var maxDepthPath = GetLongestPath(_maxObjectRoot.transform, 0);
            _maxDepthChild = maxDepthPath.Transform;
            _maxDepthCount = maxDepthPath.Depth;
            EditorGUIUtility.PingObject(_maxDepthChild.gameObject);
            Selection.activeGameObject = _maxDepthChild.gameObject;
            _score = GetScore(_maxDepthCount, _maxObjectCount);
        }
        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Score: " + _score, new GUIStyle { fontSize = 35});
    }

    
    private int GetScore(int longestPathDepth, int maxObjectCount)
    {
        const int worstScoreDepth = 6;
        const int worstScoreObjectCount = 50;
        var penaltyAccumulatedObjectCount = Mathf.Lerp(0, 60, Mathf.InverseLerp(1, worstScoreObjectCount, maxObjectCount));
        var penaltyDepth = Mathf.Lerp(0, 40, Mathf.InverseLerp(1, worstScoreDepth, longestPathDepth));
        var score = 100 - penaltyAccumulatedObjectCount - penaltyDepth;
        return (int)score;
    }
    
    private int GetTotalObjectCount(Transform transform)
    {
        if (transform.childCount == 0) return 1;
        var objectCount = 1;
        foreach (Transform child in transform)
        {
            objectCount += GetTotalObjectCount(child);
        }

        return objectCount;
    }

    private Path GetLongestPath(Transform transform, int currentCount)
    {
        if (transform.childCount == 0)
        {
            return new Path(transform, currentCount + 1);
        }

        Path? longestPath = null;
        foreach (Transform child in transform)
        {
            var thisLongestPath = GetLongestPath(child, currentCount + 1);
            if (longestPath.HasValue == false || thisLongestPath.Depth > longestPath.Value.Depth)
                longestPath = thisLongestPath;
        }
        return longestPath.Value;
    }

    private struct Path
    {
        public readonly Transform Transform;
        public readonly int Depth;

        public Path(Transform transform, int depth)
        {
            this.Transform = transform;
            this.Depth = depth;
        }
    }
}
}
