using System.Collections;
using System.Collections.Generic;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;

namespace TheGamedevGuru
{
public class GenerateHierarchySpam : MonoBehaviour
{
    public int numElements = 10000;
    public bool deepMode = false;
    public bool doTransform = true;
    public GameObject prefab;
    
    private List<Transform> allTransforms = new List<Transform>();
    
    void Start()
    {
        // The following line sets the number of job threads to create. It's an interesting toy you can play with if you have Unity 2019.3+
        // JobsUtility.JobWorkerCount = 31;

        for (var i = 0; i < numElements; i++)
        {
            var thisGameObject = Instantiate(prefab);
            if (deepMode && allTransforms.Count > 0)
            {
                thisGameObject.transform.SetParent(allTransforms[allTransforms.Count - 1], false);
            }
            allTransforms.Add(thisGameObject.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (doTransform)
        {
            var rotation = Vector3.one * .1f;
            Profiler.BeginSample("Rotation");
            foreach (var thisTransform in allTransforms)
            {
                thisTransform.Rotate(rotation);
            }
            Profiler.EndSample();
        }
    }
}
}