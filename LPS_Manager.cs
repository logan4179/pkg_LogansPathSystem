using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace LogansPathSystem
{
    /// <summary>
    /// Logans Path System Manager
    /// </summary>
    public class LPS_Manager : MonoBehaviour
    {
        [Header("REFERENCE (INTERNAL)")]
        public List<LPS_Path> _Paths;
        public List<LPS_Entity> _Entities;


        //[Header("REFERENCE (EXTERNAL)")]

        [Header("OPTIONS")]
        [SerializeField] private bool turnOnAllPathObjectsAtStart = false;

        [Header("DEBUG")]
        [SerializeField] private bool amDebugging = false;
        [SerializeField] private bool debugOnlyWhenSelected = false;
        [SerializeField] private bool amDrawingPaths = true;
        [SerializeField, UnityEngine.Range(0f, 3f)] private float radius_pathPoints = 0.2f;

        [SerializeField] private Vector3 v_labelOffset;

        [ContextMenu("z call GrabPathsFromChildren()")]
        public void GrabPathsFromChildren()
        {
            _Paths = GetComponentsInChildren<LPS_Path>().ToList();

            Debug.Log($"Got '{_Paths.Count}' children transforms");
        }

        void Start()
        {
            if( _Paths == null || _Paths.Count == 0 )
            {
                Debug.LogWarning( "LogansPathSystem Warning! LPS_Manager had no paths assigned at Start(). Was this intentional?");
            }

            if (turnOnAllPathObjectsAtStart)
            {
                for (int i_paths = 0; i_paths < _Paths.Count; i_paths++)
                {
                    if (!_Paths[i_paths].gameObject.activeSelf)
                    {
                        _Paths[i_paths].gameObject.SetActive(true);
                    }
                }
            }
        }

        [UnityEngine.Range(0f,1f)] public float TryInt = 0;
        public float SinRslt;

    }
}