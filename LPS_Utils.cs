using UnityEngine;

namespace LogansPathSystem
{
    public static class LPS_Utils
    {
        public static Vector3 FindTriangleCornerPosition
        (
            Vector3 ptA,
            float angleA,
            Vector3 ptB,
            float angleB,
            Vector3 legAProjectDir
        )
        {
            float angleC = 360f - angleA - angleB;
            float lenC = Vector3.Distance(ptA, ptB);

            float lenA = (Mathf.Sin(Mathf.Deg2Rad * angleA) * lenC) / Mathf.Sin(Mathf.Deg2Rad * angleC);

            return ptB + legAProjectDir * lenA;
        }

        /// <summary>
        /// Tells if an entity has become more aligned with the next position, as 
        /// opposed to the previous position, relative to the currentPos
        /// </summary>
        /// <param name="entityTrans"></param>
        /// <param name="prevPos"></param>
        /// <param name="currentPos"></param>
        /// <param name="nxtPos"></param>
        /// <returns></returns>
        public static bool HavePassedAlignment( 
            Transform entityTrans, Vector3 prevPos, Vector3 currentPos, Vector3 nxtPos 
        )
        {
            Vector3 v_currentPos_toEntity = entityTrans.position - currentPos;
            Vector3 v_crntPt_toPrev = prevPos - currentPos;
            Vector3 v_crntPt_toNxtPos = nxtPos - currentPos;
            float entityAlignment_withNxtPt = Vector3.Dot( 
                v_currentPos_toEntity.normalized, v_crntPt_toNxtPos.normalized );
            float entityAlignment_withPreviousPt = Vector3.Dot(
                v_currentPos_toEntity.normalized, v_crntPt_toPrev.normalized);

            if (entityAlignment_withNxtPt > entityAlignment_withPreviousPt)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Mode representing type of movement.
    /// <param>ForwardDriven_simple: Entity moves directly (linearly) to the next point if the entity is facing the next point enough to be considered 'in threshold'</param>
    /// <para>ForwardDriven_smoothed: Entity moves forward toward a Vector that continually smoothes so that it rounds corners.</para>
    /// </summary>
    public enum LPS_MovementMode
    {
        ForwardDriven_straight,
        ForwardDriven_smooth,
        PointDriven_straight,
        PointDriven_smooth,
    }
}