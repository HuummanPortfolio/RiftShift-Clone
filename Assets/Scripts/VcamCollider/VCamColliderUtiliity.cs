using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.VcamCollider
{
    public class VCamColliderUtiliity
    {
        public static void CreateCollider(
            GameObject baseGameObject, 
            ref VirtualCamera targetVcam, 
            ref int currentCollider, 
            ref List<Collider2D> baseCollidersList, 
            ref Collider2D[] colsOverlap, int n)
        {
            if (n > 0)
                for (int i = 0; i < n; i++)
                {
                    var col = baseGameObject.AddComponent<BoxCollider2D>();
                    baseCollidersList.Add(col);
                }
            else
                for (int i = Mathf.Abs(n) - 1; i >= 0; i--)
                {
                    var col = baseCollidersList[i];
                    baseCollidersList.Remove(col);
                    Object.Destroy(col);
                }

            currentCollider = colsOverlap.Length;
        }

        public static void RedrawCollider(ref VirtualCamera targetVCam, ref List<Collider2D> colliderList, Collider2D[] cols)
        {
            for (int i = 0; i < cols.Length; i++)
            {
                Vector2 pointA = cols[i].transform.GetChild(0).position;
                Vector2 pointB = cols[i].transform.GetChild(1).position;

                if (pointA.x > targetVCam.edgeTransform1.position.x)
                {
                    pointA.x = targetVCam.edgeTransform1.position.x;
                }

                if (pointA.y > targetVCam.edgeTransform1.position.y)
                {
                    pointA.y = targetVCam.edgeTransform1.position.y;
                }

                if (pointB.x < targetVCam.edgeTransform2.position.x)
                {
                    pointB.x = targetVCam.edgeTransform2.position.x;
                }

                if (pointB.y < targetVCam.edgeTransform2.position.y)
                {
                    pointB.y = targetVCam.edgeTransform2.position.y;
                }

                pointA -= (Vector2)targetVCam.transform.position;
                pointB -= (Vector2)targetVCam.transform.position;

                BoxCollider2D box = colliderList[i] as BoxCollider2D;
                box.offset = (pointA + pointB) / 2f / targetVCam.transform.localScale.x;
                box.size = (pointA - pointB) / targetVCam.transform.localScale.x;
            }
        }
    }
}
