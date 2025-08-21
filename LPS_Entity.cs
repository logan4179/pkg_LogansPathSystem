using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace LogansPathSystem
{
    public class LPS_Entity : MonoBehaviour
    {
        //[Header("REFERENCE (INTERNAL}")]
        [SerializeField] private LPS_MovementMode _movementMode;

        [Header("REFERENCE (EXTERNAL}")]
        public LPS_Path _CurrentPath;
        public LPS_PathPoint _CurrentPathPoint => _CurrentPath.PathPoints[currentPathPointIndex];
        public LPS_PathPoint PreviousPathPoint => _CurrentPath.PathPoints[currentPathPointIndex - 1];
        /// <summary>The path point that will come after the current</summary>
        public LPS_PathPoint NextPathPoint => _CurrentPath.PathPoints[currentPathPointIndex + 1];

        //[Header("OPTIONS")]


        [Header("MOVING")]
        public float _MoveSpeed = 1f;
        [Tooltip("Distance at which this entity will be considered to have reached it's goal.")]
        public float Threshold_CloseEnough = 0.1f;
        [Tooltip("If greater than 0, this causes the movement to become slower the less aligned the entity is with it's target facing direction")]
        public float SpeedEasingDistance = 0f;


        [Header("TURNING")]
        public float TurnSpeed = 1f;
        [Tooltip("Threshold at which this entity is considered 'facing enough' to start " +
            "translating toward the next goal. The closer this value is towards 1, the more strict"),
            Range(0f, 1f)]
        public float Threshold_RoughlyFacing = 0.9f;

        [Tooltip("If enabled, this causes the turning to get less sharp (turning slows) as the entity gets more in line with it's target facing direction")]
        public bool UseProgressiveTurnSpeed = false;

        [Header("OPTIONS")]
        [SerializeField] private bool snapToFirstPointAtStart = false;


        [Header("OTHER")]
        private int currentPathPointIndex = 0;
        public int CurrentPathPointIndex => currentPathPointIndex;
        private bool flag_amTraveling = false;

        //[Header("TRUTH")]
        public bool AmOnLastPathPoint
        {
            get
            {
                return currentPathPointIndex >= _CurrentPath.PathPoints.Count - 1;
            }
        }

        public bool AmPointDriven
        {
            get
            {
                return _movementMode == LPS_MovementMode.PointDriven_straight || _movementMode == LPS_MovementMode.PointDriven_smooth;
            }
        }
        public bool HavePathPointAfterCurrent => currentPathPointIndex < _CurrentPath.PathPoints.Count - 1;
        public bool AmOnStartOrEnd => currentPathPointIndex == 0 || currentPathPointIndex >= _CurrentPath.PathPoints.Count - 1;


        [Header("EVENTS")]
        public UnityEvent Event_OnPathStarted;
        public UnityEvent Event_OnPathPointWait;
        public UnityEvent Event_OnPathCompleted;

        [Header("DEBUG")]
        [SerializeField] private bool amDebugging;
        [SerializeField, TextArea(1, 10)] private string dbgClass;
        [SerializeField] private float dbgFwdLength = 1f;


        private void Start()
        {
            currentPathPointIndex = 0;
            flag_amTraveling = false;
        }

        [ContextMenu("z call BeginTravelingDownNewPath()")]
        public void BeginTravelingDownPath()
        {
            currentPathPointIndex = 0;

            if (snapToFirstPointAtStart)
            {
                transform.position = _CurrentPathPoint.Position;
            }

            flag_amTraveling = true;

            if( _movementMode == LPS_MovementMode.PointDriven_smooth )
            {
                v_chaseGoal = _CurrentPathPoint.Position;
                v_chaseGoalEnd = _CurrentPathPoint.Position;
            }

            /*(if (Event_OnPathStarted.GetPersistentEventCount() > 0) //Note: the reason
            { //I'm commenting this out and not using this if-check is because this 
                //method only checks for "persistent" listeners, and listeners added 
                //at runtime are not persistent. I think this could confuse the user, 
                //so I'm just going to force Invoke()
                Event_OnPathStarted.Invoke();
            }*/

            Event_OnPathStarted.Invoke();
            
        }

        public void BeginTravelingDownNewPath(LPS_Path path)
        {
            _CurrentPath = path;

            BeginTravelingDownPath();
        }

        /// <summary>
        /// Use if entity has been paused (either because of your outside code, or from a point flagged as 
        /// a pause point) and you want to resume traversing the path.
        /// </summary>
        public void ResumeTraveling()
        {
            flag_amTraveling = true;
        }

        public void UpdateMe()
        {
            dbgClass = $"{nameof(flag_amTraveling)}: '{flag_amTraveling}'\n" +
                $"{nameof(currentPathPointIndex)}: '{currentPathPointIndex}'\n" +
                $"";

            if
            (
                !flag_amTraveling ||
                _CurrentPath == null ||
                CurrentPathPointIndex > _CurrentPath.PathPoints.Count ||
                currentPathPointIndex < 0
            )
            {
                return; //short-circuit
            }

            if (_movementMode == LPS_MovementMode.ForwardDriven_straight )
            {
                Update_FwdDriven_straight();
            }
            else if (_movementMode == LPS_MovementMode.ForwardDriven_smooth )
            {
                Update_FwdDriven_smoothed();
            }
            else if (_movementMode == LPS_MovementMode.PointDriven_straight )
            {
                Update_PtDriven_straight();
            }
            else if (_movementMode == LPS_MovementMode.PointDriven_smooth )
            {
                Update_PtDriven_smoothed();
            }
        }

        private void IncrementPath()
        {
            if (_CurrentPathPoint.Flag_WaitAt) //needs to be done before the increment
            {
                flag_amTraveling = false;
                Event_OnPathPointWait.Invoke();
            }

            currentPathPointIndex++;

            //Debug.Log($"{nameof(currentPathPointIndex)} incremented, and now: '{currentPathPointIndex}' out of '{_CurrentPath.PathPoints.Count}' pts...");

            if (currentPathPointIndex >= _CurrentPath.PathPoints.Count)
            {
                flag_amTraveling = false;
                Debug.Log($"reached end of path.");
                Event_OnPathCompleted.Invoke();
            }
            else
            {
                if ( _movementMode == LPS_MovementMode.PointDriven_smooth )
                {
                    runningSegmentTravelTime = 0f;

                    if ( _CurrentPathPoint.HasNext )
                    {
                        v_chaseGoalEnd = _CurrentPathPoint.SmoothedPosition;
                        calculatedSegmentTravelTime = Vector3.Distance(transform.position, _CurrentPathPoint.SmoothedPosition) / _MoveSpeed;

                        //v_movingDirGoal = _CurrentPathPoint.Position;
                        v_chaseGoalStart = v_chaseGoal;

                        if ( currentPathPointIndex == 1 )
                        {
                            v_chaseGoal = _CurrentPathPoint.WideTurnPosition;
                            v_chaseGoalStart = _CurrentPathPoint.WideTurnPosition;
                        }
                        //segmentEndPos = _CurrentPathPoint.Position;
                        v_chaseGoalEnd = NextPathPoint.Position;
                    }
                    else
                    {
                        calculatedSegmentTravelTime = Vector3.Distance(transform.position, _CurrentPathPoint.Position) / _MoveSpeed;

                        v_chaseGoalEnd = _CurrentPathPoint.Position;
                    }
                }
            }
        }

        private void Update_FwdDriven_straight()
        {
            Vector3 v_entityToCurrentPt = _CurrentPathPoint.Position - transform.position;

            #region ROTATION -----------------------------------------
            Vector3 v_newFwd = Vector3.zero;
            float dot_onTrack = Vector3.Dot(transform.forward, v_entityToCurrentPt.normalized);
            float calculatedRotSpeed = TurnSpeed;

            if (UseProgressiveTurnSpeed)
            {
                calculatedRotSpeed =
                (
                    dot_onTrack > 0.5f ?
                    TurnSpeed * (0.5f + Mathf.Abs(1f - dot_onTrack)) :
                    TurnSpeed
                );
            }

            v_newFwd = Vector3.RotateTowards
            (
                transform.forward,
                v_entityToCurrentPt,
                calculatedRotSpeed * Time.deltaTime,
                0f
            );

            transform.LookAt(transform.position + v_newFwd, Vector3.up);
            #endregion

            #region MOVEMENT -----------------------------------------
            dot_onTrack = Vector3.Dot(transform.forward, v_entityToCurrentPt.normalized); //recalculate after turning...
            float calculatedMoveSpeed = _MoveSpeed;

            bool amInline = false;

            if (dot_onTrack >= Threshold_RoughlyFacing)
            {
                if ( SpeedEasingDistance > 0f ) //todo: test this, it's new
                {
                    float mvDmpnMlt = (dot_onTrack - Threshold_RoughlyFacing) / (1f - Threshold_RoughlyFacing);
                    calculatedMoveSpeed *= mvDmpnMlt;
                }

                transform.Translate(Vector3.forward * calculatedMoveSpeed * Time.deltaTime, Space.Self);

                dbgClass += $"{nameof(dot_onTrack)}: '{dot_onTrack}'\n" +
                    $"{nameof(calculatedRotSpeed)}: '{calculatedRotSpeed}'\n";

                amInline = true;
            }
            else
            {
                dbgClass += $"{nameof(dot_onTrack)}: '{dot_onTrack}'\n" +
                    $"{nameof(calculatedRotSpeed)}: '{calculatedRotSpeed}'\n";
                amInline = false;
            }
            #endregion

            #region INCREMENT CHECK ---------------------------------
            float distToCrntPt = Vector3.Distance(transform.position, _CurrentPathPoint.Position);
            dbgClass += $"{nameof(distToCrntPt)}: '{distToCrntPt}'\n" +
                $"";

            if (distToCrntPt <= Threshold_CloseEnough)
            {
                IncrementPath();
            }
            #endregion

#if UNITY_EDITOR
            if (amDebugging)
            {
                Debug.DrawLine
                (
                    transform.position,
                    transform.position + (transform.forward * dbgFwdLength),
                    amInline ? Color.green : Color.red
                );
            }
#endif
        }

        private void Update_FwdDriven_smoothed()
        {
            Vector3 v_entityToCrntPt = _CurrentPathPoint.Position - transform.position;

            //the idea: blend the actual look direction between v_prevDir and v_nextDir based on distance traveled.

            #region ROTATION -----------------------------------------
            Vector3 v_newFwd = Vector3.zero;
            Vector3 v_calculatedRotTgt = v_entityToCrntPt;
            float calculatedRotSpeed = TurnSpeed;

            if ( _CurrentPathPoint.HasPrev )
            {
                Vector3 v_entityToWideCrntPt = _CurrentPathPoint.WideTurnPosition - transform.position;
                float distRemaining = Mathf.Min
                (
                    Vector3.Distance(transform.position, _CurrentPathPoint.Position),
                    Vector3.Distance(transform.position, _CurrentPathPoint.WideTurnPosition)
                );

                //The following makes it so that as the entity gets closer to the current point,
                //the current bias grows.
                float currentPtBias = Mathf.Clamp(
                    (
                        1f - (
                        distRemaining /
                        _CurrentPathPoint.DistToPrev )
                    ) * 1.25f,
                    0f, 1f
                );

                float otherPtBias = Mathf.Clamp( 1f - currentPtBias, 0f, 1f );

                dbgClass += $"distToCrnt: '{Vector3.Distance(transform.position, _CurrentPathPoint.Position)}', " +
                    $"distToNxt: '{Vector3.Distance(transform.position, NextPathPoint.WideTurnPosition)}'\n";

                Vector3 v_entityToFuturePt = NextPathPoint.WideTurnPosition - transform.position;

                v_calculatedRotTgt = Vector3.Normalize
                (
                    (v_entityToWideCrntPt.normalized * otherPtBias) +
                    /*(v_entityToFuturePt.normalized * futurePtBias)*/
                    (v_entityToCrntPt.normalized * currentPtBias)

                );

                dbgClass += $"crntBias: '{currentPtBias}', vTgtRot: '{v_calculatedRotTgt}'\n";
            }

#if UNITY_EDITOR
            if (amDebugging)
            {
                //Debug.DrawLine(transform.position, transform.position + (v_calculatedRotTgt * 150f), Color.magenta);
                Debug.DrawLine(Vector3.zero, v_calculatedRotTgt * 150f, Color.magenta);
            }
#endif

            v_newFwd = Vector3.RotateTowards
            (
                transform.forward,
                v_calculatedRotTgt,
                calculatedRotSpeed * Time.deltaTime,
                0f
            );

            transform.LookAt(transform.position + v_newFwd, Vector3.up);
            #endregion

            #region MOVEMENT -----------------------------------------
            float dot_onTrack = Vector3.Dot(transform.forward, v_calculatedRotTgt.normalized); //recalculate after turning...
            if (dot_onTrack >= Threshold_RoughlyFacing)
            {
                transform.Translate(Vector3.forward * _MoveSpeed * Time.deltaTime, Space.Self);

                dbgClass += $"{nameof(dot_onTrack)}: '{dot_onTrack}'\n" +
                    $"{nameof(v_calculatedRotTgt)}: '{v_calculatedRotTgt}'\n";
            }
            else
            {
                dbgClass += $"{nameof(dot_onTrack)}: '{dot_onTrack}'\n" +
                    $"{nameof(v_calculatedRotTgt)}: '{v_calculatedRotTgt}'\n";
            }

            #endregion

            #region INCREMENT CHECK --------------------------------------------
            float distToCrntPt = Vector3.Distance(transform.position, _CurrentPathPoint.Position);
            if ( _CurrentPathPoint.WideTurnPosition != _CurrentPathPoint.Position)
            {
                float distToWidePt = Vector3.Distance(transform.position, _CurrentPathPoint.WideTurnPosition);
                if (distToWidePt < distToCrntPt)
                {
                    distToCrntPt = distToWidePt;
                }
            }

            dbgClass += $"{nameof(distToCrntPt)}: '{distToCrntPt}'\n" +
                $"";

            if (
                distToCrntPt <= Threshold_CloseEnough ||
                (
                    !AmOnStartOrEnd &&
                    LPS_Utils.HavePassedAlignment
                    (
                        transform,
                        PreviousPathPoint.Position, _CurrentPathPoint.Position, NextPathPoint.Position
                    )
                )
            )
            {
                IncrementPath();
            }
            #endregion
        }

        private void Update_PtDriven_straight()
        {
            Vector3 v_entityToCurrentPt = _CurrentPathPoint.Position - transform.position;

            #region ROTATION -----------------------------------------
            //Not sure if I'm even going to have rotation for this mode...

            #endregion

            #region MOVEMENT -----------------------------------------

            transform.Translate(v_entityToCurrentPt * _MoveSpeed * Time.deltaTime, Space.Self);

            #endregion

            #region INCREMENT CHECK ---------------------------------
            float distToCrntPt = Vector3.Distance(transform.position, _CurrentPathPoint.Position);
            dbgClass += $"{nameof(distToCrntPt)}: '{distToCrntPt}'\n" +
                $"";

            if (distToCrntPt <= Threshold_CloseEnough)
            {
                IncrementPath();
            }
            #endregion
        }

        float calculatedSegmentTravelTime;
        float runningSegmentTravelTime;
        Vector3 v_chaseGoalEnd;
        Vector3 v_chaseGoal;
        Vector3 v_chaseGoalStart;
        private void Update_PtDriven_smoothed()
        {
            runningSegmentTravelTime += Time.deltaTime;

            float timePassedPercentage = runningSegmentTravelTime / calculatedSegmentTravelTime;
            dbgClass += $"{nameof(runningSegmentTravelTime)}: '{runningSegmentTravelTime}' / {calculatedSegmentTravelTime}\n" +
                $"{nameof(timePassedPercentage)}: '{timePassedPercentage}'\n" +
                $"";

            if( !AmOnStartOrEnd )
            {
                v_chaseGoal = Vector3.Lerp( v_chaseGoalStart, v_chaseGoalEnd, timePassedPercentage );

                Vector3 v_toGoal = Vector3.Normalize( v_chaseGoal - transform.position );

                transform.Translate
                (
                    v_toGoal * _MoveSpeed * Time.deltaTime, Space.Self
                );

                dbgClass += $"{nameof(v_chaseGoal)}: '{v_chaseGoal}'\n" +
                    $"{nameof(v_toGoal)}: '{v_toGoal}'\n" +
                    $"";
#if UNITY_EDITOR
                if (amDebugging)
                {
                    Debug.DrawLine(transform.position, transform.position + (v_toGoal * 150f), Color.magenta);
                    Debug.DrawLine( v_chaseGoal, v_chaseGoal + (Vector3.up * 10f), Color.magenta );
                }
#endif
            }
            else
            {
                dbgClass += $"moving towards: '{Vector3.Normalize(_CurrentPathPoint.Position - transform.position)}'\n" +
                    $"";

                transform.Translate
                (
                    Vector3.Normalize(_CurrentPathPoint.Position - transform.position) * 
                    _MoveSpeed * Time.deltaTime, Space.Self
                );
            }

            #region ROTATION -----------------------------------------
            //Not sure if I'm even going to have rotation for this mode...

            #endregion

            #region DECIDE IF INCREMENT IS NEEDED -----------------------------------
            dbgClass += $"distToEndGoal: '{Vector3.Distance(transform.position, v_chaseGoalEnd)}' / {Threshold_CloseEnough}\n" +
                $"";
            if (
                Vector3.Distance(transform.position, v_chaseGoalEnd) <= Threshold_CloseEnough ||
                (
                    !AmOnStartOrEnd &&
                    LPS_Utils.HavePassedAlignment
                    (
                        transform,
                        PreviousPathPoint.Position, _CurrentPathPoint.Position, NextPathPoint.Position
                    )
                ) ||
                (
                    AmOnLastPathPoint && 
                    Vector3.Dot
                    (
                        Vector3.Normalize( transform.position - _CurrentPathPoint.Position),
                        _CurrentPathPoint.V_ToPrev.normalized
                    ) < 0
                )
            )
            {
                IncrementPath();
            }
            #endregion
        }

#if UNITY_EDITOR
        public void DrawMyGizmos(Vector3 vlblOffset)
        {
            if (!amDebugging || _CurrentPath == null || _CurrentPath.PathPoints.Count <= 0)
            {
                return;
            }

            Color oldClr = Gizmos.color;

            if ( Application.isPlaying )
            {
                for (int i = 0; i < _CurrentPath.PathPoints.Count; i++)
                {
                    Handles.DrawWireDisc(
                       _CurrentPath.PathPoints[i].Position,
                        Vector3.up,
                        Threshold_CloseEnough
                    );

                    if ( !_CurrentPath.PathPoints[i].AmStartOrEndOfPath )
                    {
                        if ( _CurrentPath.PathPoints[i].SmoothedPosition != _CurrentPath.PathPoints[i].Position )
                        {
                            Handles.DrawWireDisc(
                               _CurrentPath.PathPoints[i].SmoothedPosition,
                                Vector3.up,
                                Threshold_CloseEnough
                            );
                        }

                        if( _CurrentPath.PathPoints[i].WideTurnPosition != _CurrentPath.PathPoints[i].Position )
                        {
                            Handles.DrawWireDisc(
                               _CurrentPath.PathPoints[i].WideTurnPosition,
                                Vector3.up,
                                Threshold_CloseEnough
                            );
                        }
                    }

                }

                if (flag_amTraveling)
                {
                    if (currentPathPointIndex > -1 && currentPathPointIndex < _CurrentPath.PathPoints.Count)
                    {

                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine
                        (
                            _CurrentPath.PathPoints[currentPathPointIndex].Position,
                            _CurrentPath.PathPoints[currentPathPointIndex].Position + (vlblOffset * 3)
                        );

                        Handles.Label(_CurrentPath.PathPoints[currentPathPointIndex].Position + (vlblOffset * 3), "current");

                    }
                }

            }
            else
            {
                for (int i = 0; i < _CurrentPath.PathPoints.Count; i++)
                {
                    Handles.DrawWireDisc(_CurrentPath.PathPoints[i].Position, Vector3.up, Threshold_CloseEnough);
                }

                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + (transform.forward * dbgFwdLength));
                Handles.Label(transform.position + (transform.forward * dbgFwdLength), "eFWD");
            }

            Gizmos.color = oldClr;

        }
#endif
    }
}