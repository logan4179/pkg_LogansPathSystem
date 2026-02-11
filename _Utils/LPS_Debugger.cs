using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LogansPathSystem
{
    public class LPS_Debugger : MonoBehaviour
    {
        [SerializeField] private bool amDebugging = false;

        [Header("REFERENCE (EXTERNAL)")]
        public List<LPS_Path> _Paths;
        public List<LPS_Entity> _Entities;

        [Header("OPTIONS")]
        [SerializeField] private bool debugOnlyWhenSelected = false;
        [SerializeField] private bool amDrawingPaths = true;
        [SerializeField] private bool amDrawingEntities = true;

        [SerializeField, UnityEngine.Range(0f, 3f)] private float radius_pathPoints = 0.2f;

        [SerializeField] private Vector3 v_labelOffset;

        [Header("=====================================")]
        [SerializeField] private bool drawCrossPoints = false;
        [SerializeField] private bool drawWidePoints = false;
        [SerializeField] private bool drawSmoothPoints = false;


        [ContextMenu("z call GrabPathsFromChildren()")]
        public void GrabPathsFromChildren()
        {
            _Paths = GetComponentsInChildren<LPS_Path>().ToList();

            Debug.Log($"Got '{_Paths.Count}' children transforms");
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            #region SHORT-CIRCUIT ================================================
            if 
            ( 
                !amDebugging || 
                (debugOnlyWhenSelected && Selection.activeObject == null) /*|| 
                (debugOnlyWhenSelected && Selection.activeGameObject != this)*/ 
            )
            {
                return;
            }

            int selectionType = 0; //0 = none, 1 = this manager, 2 = single path, 3 = single entity
            int selectionIndx = -1;

            if( Selection.activeGameObject == gameObject )
            {
                selectionType = 1;
            }
            else if( Selection.activeGameObject != null )
            {
                bool foundIt = false;

                for ( int i = 0; i < _Paths.Count; i++ )
                {
                    if( Selection.activeGameObject == null ) //This looks superfluous, but had to put this in bc when I stop the editor play, it will throw an error otherwise.
                    {
                        return;
                    }

                    if
                    ( 
                        Selection.activeGameObject == _Paths[i].gameObject || 
                        Selection.activeGameObject.transform.parent == _Paths[i].transform
                    )
                    {
                        foundIt = true;
                        selectionType = 2;
                        selectionIndx = i;
                        break;
                    }
 
                }

                if ( !foundIt )
                {
                    for ( int i = 0; i < _Entities.Count; i++ )
                    {
                        if ( Selection.activeGameObject == _Entities[i].gameObject )
                        {
                            foundIt = true;
                            selectionType = 3;
                            selectionIndx = i;
                            break;
                        }
                    }
                }

                if ( !foundIt )
                {
                    if( !debugOnlyWhenSelected )
                    {
                        selectionType = 1;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else //Selection.activeGameObject == null...
            {
                if ( !debugOnlyWhenSelected )
                {
                    selectionType = 1;
                }
            }
            #endregion

            //Debug.Log(selectionType);

            if (selectionType == 1) //this manager
            {
                if (amDrawingPaths && _Paths != null && _Paths.Count > 0)
                {
                    for (int i_paths = 0; i_paths < _Paths.Count; i_paths++)
                    {
                        if (_Paths[i_paths].gameObject.activeSelf)
                        {
                            _Paths[i_paths].DrawMyGizmos(
                                radius_pathPoints, v_labelOffset,
                                drawCrossPoints, drawWidePoints, drawSmoothPoints
                            );
                        }
                    }
                }

                if (amDrawingEntities && _Entities != null && _Entities.Count > 0)
                {
                    for (int i_entities = 0; i_entities < _Entities.Count; i_entities++)
                    {
                        if (_Entities[i_entities].gameObject.activeSelf)
                        {
                            _Entities[i_entities].DrawMyGizmos(v_labelOffset);
                        }
                    }
                }
            }
            else if (selectionType == 2) //single path
            {
                _Paths[selectionIndx].DrawMyGizmos(radius_pathPoints, v_labelOffset, 
                    drawCrossPoints, drawWidePoints, drawSmoothPoints
                );
            }
            else if (selectionType == 3) //single entity
            {
                _Entities[selectionIndx].DrawMyGizmos(v_labelOffset);

                if( _Entities[selectionIndx]._CurrentPath != null )
                {
                    _Entities[selectionIndx]._CurrentPath.DrawMyGizmos(radius_pathPoints, v_labelOffset,
                        drawCrossPoints, drawWidePoints, drawSmoothPoints
                    );
                }
            }
        }
#endif
    }
}
