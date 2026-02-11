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

        //[Header("DEBUG")]


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

        public Vector3 GetVectorToNextPt( int indx )
        {
            return PathPoints[indx+1].Position - PathPoints[indx].Position;
        }

        public Vector3 GetVectorToPreviousPt(int indx)
        {
            return PathPoints[indx - 1].Position - PathPoints[indx].Position;
        }

        public float GetDistToPreviousPt( int indx )
        {
            return Vector3.Distance( PathPoints[indx].Position, PathPoints[indx - 1].Position );
        }

        public float GetDistToNextPt(int indx)
        {
            return Vector3.Distance(PathPoints[indx].Position, PathPoints[indx + 1].Position);
        }

        public bool AmStartOrEndOfPath(int indx)
        {
            return indx == 0 || indx == PathPoints.Count - 1;
        }

        [ContextMenu("z call GrabPointsFromChildren()")]
        public void GrabPointsFromChildren()
        {
            PathPoints = GetComponentsInChildren<LPS_PathPoint>().ToList();

            Debug.Log($"Got '{PathPoints.Count}' children transforms");
        }

        public LPS_PathPoint GetClosestPointToPosition( Vector3 pos )
        {
            int runningBestIndx = -1;
            float runningBestDist = float.MaxValue;

            for ( int i = 0; i < PathPoints.Count; i++ )
            {
                float dist = Vector3.Distance( pos, PathPoints[i].Position );
                if ( dist < runningBestDist )
                {
                    runningBestDist = dist;
                    runningBestIndx = i;
                }
            }

            return PathPoints[runningBestIndx];
        }


#if UNITY_EDITOR
        public void DrawMyGizmos(float radius, Vector3 lblOffset, bool dbgCross, 
            bool dbgWide, bool dbgSmooth ) //called from the LPS_Debugger
        {
            if (PathPoints == null || PathPoints.Count <= 0)
            {
                return;
            }

            Color oldColor = Gizmos.color;

            Gizmos.color = color_debugPath;
            for (int i = 0; i < PathPoints.Count; i++)
            {
                PathPoints[i].DrawMyGizmos( 
                    radius, $"p{i}", lblOffset, dbgCross, dbgWide, dbgSmooth 
                );

                if (i < PathPoints.Count - 1)
                {
                    Gizmos.DrawLine
                    (
                        PathPoints[i].transform.position,
                        PathPoints[i + 1].transform.position
                    );
                }

                if ( dbgSmooth && !Application.isPlaying && i > 0 && i < PathPoints.Count - 1 )
                {
                    if ( i <= 0 || i >= PathPoints.Count - 1 )
                    {
                        continue;
                    }

                    Vector3 smoothPt = (
                        PathPoints[i - 1].Position +
                        PathPoints[i].Position +
                        PathPoints[i + 1].Position
                    ) / 3f;

                    Gizmos.DrawCube(smoothPt, Vector3.one * radius * 0.5f);
                    Gizmos.DrawLine(PathPoints[i].Position, smoothPt);
                    Handles.Label( smoothPt, "s" );

                }
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(PathPoints[0].Position, PathPoints[0].Position + (lblOffset * 3f) );
            Handles.Label(PathPoints[0].Position + (lblOffset * 3f), "start");

            Gizmos.color = Color.red;
            Gizmos.DrawLine(PathPoints[PathPoints.Count - 1].Position, PathPoints[PathPoints.Count-1].Position + (lblOffset * 3f));
            Handles.Label(PathPoints[PathPoints.Count - 1].Position + (lblOffset * 3f), "end");

            Gizmos.color = oldColor;
        }
#endif
    }
}