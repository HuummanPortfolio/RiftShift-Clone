using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VirtualCamera : MonoBehaviour
{
    public Transform transformUjung1;
    public Transform transformUjung2;
    public VirtualCamera virtualcameraPartner;
    public int currentCollider = 0;
    public LayerMask mask;
    public List<Collider2D> colliderList;

    private void Update()
    {
        var colliders = Physics2D.OverlapAreaAll(transformUjung1.position, transformUjung2.position, mask);
        if (colliders.Length != currentCollider)
        {
            virtualcameraPartner.CreateCollider(colliders.Length - currentCollider);
            currentCollider = colliders.Length;
        }

        virtualcameraPartner.RedrawCollider(colliders);
    }

    void CreateCollider(int n)
    {
        if (n > 0)
            for (int i = 0; i < n; i++)
            {
                var col = gameObject.AddComponent<BoxCollider2D>();
                colliderList.Add(col);
            }
        else
            for (int i = Mathf.Abs(n) - 1; i >= 0; i--)
            {
                var col = colliderList[i];
                colliderList.Remove(col);
                Destroy(col);
            }
    }

    void RedrawCollider(Collider2D[] cols)
    {
        for (int i = 0; i < cols.Length; i++)
        {
            Vector2 pointA = cols[i].transform.GetChild(0).position;
            Vector2 pointB = cols[i].transform.GetChild(1).position;

            if (pointA.x > virtualcameraPartner.transformUjung1.position.x)
            {
                pointA.x = virtualcameraPartner.transformUjung1.position.x;
            }

            if (pointA.y > virtualcameraPartner.transformUjung1.position.y)
            {
                pointA.y = virtualcameraPartner.transformUjung1.position.y;
            }

            if (pointB.x < virtualcameraPartner.transformUjung2.position.x)
            {
                pointB.x = virtualcameraPartner.transformUjung2.position.x;
            }

            if (pointB.y < virtualcameraPartner.transformUjung2.position.y)
            {
                pointB.y = virtualcameraPartner.transformUjung2.position.y;
            }

            pointA -= (Vector2)virtualcameraPartner.transform.position;
            pointB -= (Vector2)virtualcameraPartner.transform.position;

            BoxCollider2D box = colliderList[i] as BoxCollider2D;
            box.offset = (pointA + pointB) / 2f / virtualcameraPartner.transform.localScale.x;
            box.size = (pointA - pointB)  / virtualcameraPartner.transform.localScale.x;
        }
    }
}
