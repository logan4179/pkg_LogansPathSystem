using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LogansPathSystem
{
    public class LPS_Path : MonoBehaviour
    {
        public List<LPS_PathPoint> PathPoints;

        [Header("DEBUG")]
        [SerializeField] private bool dbgCross = false;
        [SerializeField] private bool dbgWide = false;

        public Vector3 Destination
        {
            get
            {
                return PathPoints[PathPoints.Count - 1].Position;
            }
        }

        [Header("DEBUG")]
        [SerializeField] private Color color_debugPath;

        private void Awake()
        {
            if (PathPoints == null || PathPoints.Count <= 0)
            {
                return;
            }

            InitializePath();
        }

        [ContextMenu("z call InitializePath()")]
        public void InitializePath()
        {
            for (int i = 0; i < PathPoints.Count; i++)
            {
                if (i == 0)
                {
                    PathPoints[i].SetPrevAndNext(null, PathPoints[i + 1]);

                }
                else if (i == PathPoints.Count - 1)
                {
                    PathPoints[i].SetPrevAndNext(PathPoints[i - 1], null);

                }
                else
                {
                    PathPoints[i].SetPrevAndNext(PathPoints[i - 1], PathPoints[i + 1]);
                }
            }
        }


        [ContextMenu("z call GrabPointsFromChildren()")]
        public void GrabPointsFromChildren()
        {
            PathPoints = GetComponentsInChildren<LPS_PathPoint>().ToList();

            Debug.Log($"Got '{PathPoints.Count}' children transforms");
        }


#if UNITY_EDITOR
        public void DrawMyGizmos(float radius, Vector3 lblOffset)
        {
            if (PathPoints == null || PathPoints.Count <= 0)
            {
                return;
            }

            Color oldColor = Gizmos.color;

            Gizmos.color = color_debugPath;
            for (int i = 0; i < PathPoints.Count; i++)
            {
                PathPoints[i].DrawMyGizmos( radius, $"p{i}", lblOffset, dbgCross, dbgWide );

                if (i < PathPoints.Count - 1)
                {
                    Gizmos.DrawLine
                    (
                        PathPoints[i].transform.position,
                        PathPoints[i + 1].transform.position
                    );
                }

                if ( !Application.isPlaying && i > 0 && i < PathPoints.Count - 1 )
                {
                    if ( i <= 0 || i >= PathPoints.Count - 1 )
                    {
                        continue;
                    }

                    Vector3 pt = (
                        PathPoints[i - 1].Position +
                        PathPoints[i].Position +
                        PathPoints[i + 1].Position
                    ) / 3f;

                    Gizmos.DrawCube(pt, Vector3.one * radius * 0.5f);
                    Gizmos.DrawLine(PathPoints[i].Position, pt);
                    Handles.Label( pt, "smoothed" );

                }
            }

            Gizmos.color = oldColor;
        }
#endif
    }
}