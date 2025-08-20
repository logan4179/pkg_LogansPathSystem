using System.Drawing;
using UnityEditor;
using UnityEngine;

namespace LogansPathSystem
{
    public class LPS_PathPoint : MonoBehaviour
    {
        [Tooltip("A flag directing an NPS_Entity to stop at this point and wait for outside " +
            "logic to turn movement back on.")]
        public bool Flag_WaitAt = false;

        [Tooltip("Allows the NPS_Entity speed to be overriden when traveling to this point, " +
            "only if this value is above 0")]
        public float SpeedOverride = -1;

        [SerializeField] private bool disableMyMeshRendererOnStart = false;

        public Vector3 Position => transform.position;

        // [Header("CALCULATED")] ---------------
        private Vector3 smoothedPosition;
        public Vector3 SmoothedPosition => smoothedPosition;
        public Vector3 WideTurnPosition => transform.position - V_toSmoothedPosition;
        private float dist_prevToWideTurnPosition;
        public float Dist_prevToWideTurnPosition => dist_prevToWideTurnPosition;

        public Vector3 V_toSmoothedPosition => smoothedPosition - Position;

        public bool HasNext => distToNext > -1f;
        private float distToNext = -1f;
        public float DistToNext => distToNext;
        private Vector3 v_toNext;
        public Vector3 V_ToNext => v_toNext;

        private Vector3 v_cross;
        public Vector3 V_Cross => v_cross;

        public bool HasPrev => distToPrev > -1f;

        private float distToPrev = -1f;

        public bool AmStartOrEndOfPath
        {
            get
            {
                return distToPrev <= 0f || distToNext <= 0f;
            }
        }
        public float DistToPrev => distToPrev;
        private Vector3 v_toPrev;
        public Vector3 V_ToPrev => v_toPrev;
        private float cachedAngle = 0f;
        public float CachedAngle => cachedAngle;
        //-----------------------------------------

        [Header("DEBUG")]
        [SerializeField, TextArea(1,10)] private string dbgClass;

        void Start()
        {
            if (disableMyMeshRendererOnStart)
            {
                MeshRenderer mr = null;

                if (TryGetComponent(out mr))
                {
                    mr.enabled = false;
                }
            }
        }

        public void SetPrevAndNext(LPS_PathPoint prevPt, LPS_PathPoint nextPt)
        {
            dbgClass = $"{nameof(SetPrevAndNext)}({(prevPt == null ? "null" : prevPt.Position)}, " +
                $"{(nextPt == null ? "null" : nextPt.Position)})\n";

            distToNext = -1f;
            distToPrev = -1f;
            v_toNext = Vector3.zero;
            v_toPrev = Vector3.zero;
            v_cross = Vector3.zero;
            smoothedPosition = Position;
            cachedAngle = -1f;
            dist_prevToWideTurnPosition = -1f;

            if ( nextPt != null )
            {
                distToNext = Vector3.Distance( Position, nextPt.Position );
                v_toNext = nextPt.Position - Position;
            }

            if ( prevPt != null )
            {
                distToPrev = Vector3.Distance( Position, prevPt.Position );
                v_toPrev = prevPt.Position - Position;
            }

            if ( prevPt != null && nextPt != null )
            {
                smoothedPosition = (Position + prevPt.Position + nextPt.Position) / 3f;
                cachedAngle = Vector3.Angle( v_toPrev, v_toNext );
            }


            if ( prevPt != null ) //If this is the last point, and there's no next point...
            {
                dbgClass += $"prev point NOT null. Cacluating v_cross...\n";
                v_cross = Vector3.Cross(-v_toPrev, Vector3.up).normalized;
                float dotA = Vector3.Dot(v_cross, V_toSmoothedPosition.normalized);
                float dotB = Vector3.Dot(v_cross, -V_toSmoothedPosition.normalized);

                if
                ( dotA > dotB ) //correct if wrong direction...
                {
                    dbgClass += $"dotA: '{dotA}' " +
                        $"more in line than dotB: '{dotB}'. " +
                        $"Flipping vcross ({v_cross})...\n";
                    v_cross = -v_cross;
                    dbgClass += $"vcross now: '{v_cross}'\n";
                }

                dist_prevToWideTurnPosition = Vector3.Distance( prevPt.Position, WideTurnPosition );
            }
            else
            {
                dbgClass += $"decided prev point null...\n";
                v_cross = Vector3.zero;
            }

            dbgClass += $"\n\n" +
                $"Final Values......\n" +
                $"{nameof(Position)}: '{Position}'\n" +
                $"{nameof(smoothedPosition)}: '{smoothedPosition}'\n" +
                $"{nameof(distToNext)}: '{distToNext}'\n" +
                $"{nameof(distToPrev)}: '{distToPrev}'\n" +
                $"{nameof(v_toNext)}: '{v_toNext}'\n" +
                $"{nameof(v_toPrev)}: '{v_toPrev}'\n" +
                $"{nameof(v_cross)}: '{v_cross}'\n" +
                $"{nameof(v_toPrev)}: '{v_toPrev}'\n" +
                $"{nameof(cachedAngle)}: '{cachedAngle}'\n" +
                $"{nameof(dist_prevToWideTurnPosition)}: '{dist_prevToWideTurnPosition}'\n" +

                $"";

        }

#if UNITY_EDITOR
        public void DrawMyGizmos(float radius, string lbl, Vector3 lblOfst, bool dbgCross, bool dbgWide )
        {
            Gizmos.DrawSphere(transform.position, radius);
            Gizmos.DrawLine(transform.position, transform.position + lblOfst);
            Handles.Label(transform.position + lblOfst, lbl);

            if( Application.isPlaying )
            {
                if (smoothedPosition != Vector3.zero)
                {
                    Gizmos.DrawCube(smoothedPosition, Vector3.one * radius * 0.5f);
                    Gizmos.DrawLine(Position, smoothedPosition);
                    Handles.Label( smoothedPosition, "smoothed" );
                }

                if ( dbgWide && WideTurnPosition != Vector3.zero  && WideTurnPosition != Position )
                {
                    Gizmos.DrawLine( Position, WideTurnPosition );
                    Gizmos.DrawCube(WideTurnPosition, Vector3.one * radius * 0.5f );
                    Handles.Label( WideTurnPosition, "WIDE");
                }
            }

            if( dbgCross && v_cross != Vector3.zero)
            {
                Gizmos.DrawLine( Position, Position + (v_cross * 5f) );
                Handles.Label( Position + (v_cross * 5f), "cross" );
            }


        }
#endif
    }
}