using UnityEngine;
using UnityEditor;
using System.ComponentModel;

namespace LogansPathSystem
{
    /// <summary>
    /// Utility clalss that has some helpful features for creating paths
    /// </summary>
    public class LPS_PathHelper : MonoBehaviour
    {
        [SerializeField] private LPS_Manager _manager;

        [Header("OPTIONS")]
        [SerializeField, Range(0,2), Tooltip("0 = none, 1 = meters, 2 = path points")] 
        private int snapMode = 0;

        [Header("DISTANCE CALCULATOR")]
        [SerializeField] private Transform distObject;

        [Header("ANGLE CALCULATOR")]
        [SerializeField] private Transform angObjectA;
        [SerializeField] private Transform angObjectB;

        [Header("FORCE ANGLE")]
        [SerializeField] private float forceAngleValue = 0f;

        [Header("FORCE DISTANCE")]
        [SerializeField] private float forceDistanceValue = 0f;

        [Header("SETTINGS")]
        [SerializeField, Range(0f, 2f)] private float snapThreshold = 0.1f;

        #region HELPERS ========================================
        [ContextMenu("z call ForceAngle()")]
        public void ForceAngle()
        {
            Vector3 v_toA = angObjectA.position - transform.position;
            Vector3 v_toB = angObjectB.position - transform.position;

            Vector3 v_mid = (v_toA + v_toB) / 2f;
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            
            if( Selection.activeGameObject == this.gameObject )
            {
                if( _manager != null && snapThreshold > 0f )
                {
                    //Debug.Log("a");
                    if( snapMode == 1 )
                    {
                        Vector3 crntPos = transform.position;
                        Vector3 newPos = new Vector3(
                            (int)crntPos.x,
                            (int)crntPos.y,
                            (int)crntPos.z
                        );

                        if( Vector3.Distance(crntPos, newPos) < snapThreshold )
                        {
                            transform.position = new Vector3(
                                (int)crntPos.x,
                                (int)crntPos.y,
                                (int)crntPos.z
                            );
                        }
                    }
                    else if( snapMode == 2 )
                    {
                        LPS_PathPoint runningbestPt = null;
                        float runningBestDist = float.MaxValue;

                        //Debug.Log($"checking manager through '{_manager._Paths.Count}' paths...");
                        for ( int i_paths = 0; i_paths < _manager._Paths.Count; i_paths++ )
                        {
                            if( _manager._Paths[i_paths].PathPoints != null && _manager._Paths[i_paths].PathPoints.Count > 0 )
                            {
                                LPS_PathPoint pt = _manager._Paths[i_paths].GetClosestPointToPosition( transform.position );
                                float dist = Vector3.Distance( transform.position, pt.Position );
                                if( dist < snapThreshold && dist < runningBestDist )
                                {
                                    //Debug.Log($"found new best point: '{pt}', at dist: '{dist}'");
                                    runningBestDist = dist;
                                    runningbestPt = pt;
                                }
                            }
                        }

                        if( runningbestPt != null )
                        {
                            transform.position = runningbestPt.Position;
                        }
                    }
                }
            }
            else
            {
                //return;
            }


            if (distObject != null)
            {
                Handles.DrawDottedLine(transform.position, distObject.position, 0.1f);
                Vector3 vTo = distObject.position - transform.position;
                Handles.Label(
                    transform.position + (vTo / 2f), 
                    $"dist\n{Vector3.Distance(transform.position, distObject.position)}"
                );
            }

            if ( angObjectA != null && angObjectB != null )
            {
                Gizmos.DrawLine(transform.position, angObjectA.position);
                Gizmos.DrawLine(transform.position, angObjectB.position);

                Vector3 v_toA = angObjectA.position - transform.position;
                Vector3 v_toB = angObjectB.position - transform.position;

                Vector3 v_mid = (v_toA + v_toB) / 2f;
                Handles.Label(
                    transform.position + (v_mid.normalized * 2f), 
                    $"ang\n{Vector3.Angle(v_toA, v_toB).ToString()}"
                );
            }
        }
#endif
    }
}
