using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class VirtualCamera : MonoBehaviour
{
    [Header("Transform")]
    [SerializeField] float baseSize = 3;
    [FormerlySerializedAs("transformUjung1")] public Transform edgeTransform1;
    [FormerlySerializedAs("transformUjung2")] public Transform edgeTransform2;

    [Header("Colliders")]
    public int currentCollider = 0;
    public LayerMask mask;
    public List<Collider2D> colliderList;

    [Header("Camera")]
    public VirtualCamera virtualcameraPartner;
    [SerializeField] private bool isMainVCam;
    [SerializeField] private Camera renderCamera;
    [SerializeField] private RenderTexture targetRenderTexture;

    private void Awake()
    {
        var colliders = Physics2D.OverlapAreaAll(edgeTransform1.position, edgeTransform2.position, mask);
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

            if (pointA.x > virtualcameraPartner.edgeTransform1.position.x)
            {
                pointA.x = virtualcameraPartner.edgeTransform1.position.x;
            }

            if (pointA.y > virtualcameraPartner.edgeTransform1.position.y)
            {
                pointA.y = virtualcameraPartner.edgeTransform1.position.y;
            }

            if (pointB.x < virtualcameraPartner.edgeTransform2.position.x)
            {
                pointB.x = virtualcameraPartner.edgeTransform2.position.x;
            }

            if (pointB.y < virtualcameraPartner.edgeTransform2.position.y)
            {
                pointB.y = virtualcameraPartner.edgeTransform2.position.y;
            }

            pointA -= (Vector2)virtualcameraPartner.transform.position;
            pointB -= (Vector2)virtualcameraPartner.transform.position;

            BoxCollider2D box = colliderList[i] as BoxCollider2D;
            box.offset = (pointA + pointB) / 2f / virtualcameraPartner.transform.localScale.x;
            box.size = (pointA - pointB) / virtualcameraPartner.transform.localScale.x;
        }
    }


    [ContextMenu("Update Scale")]
    private void UpdateScale()
    {
        if (transform.localScale.x == baseSize || transform.localScale.y == baseSize)
            return;

        transform.localScale = new(baseSize, baseSize,1);

        if (renderCamera != null)
            renderCamera.orthographicSize = transform.localScale.x / 2;
    }

    private void UpdateScale(float size)
    {
        baseSize = size;
        transform.localScale = new(size, size,1);

        if (renderCamera != null)
            renderCamera.orthographicSize = transform.localScale.x / 2;
    }
    
    private void GetReference()
    {
        if (renderCamera == null) renderCamera = GetComponentInChildren<Camera>();

        if (targetRenderTexture != null)
            renderCamera.targetTexture = targetRenderTexture;

        if (isMainVCam && virtualcameraPartner != null)
            virtualcameraPartner.UpdateScale(baseSize);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        GetReference();
        if (isMainVCam)
            UpdateScale();
    }
#endif
}