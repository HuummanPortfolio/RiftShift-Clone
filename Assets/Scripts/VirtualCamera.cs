using RiftShiftClone.Scripts.VcamCollider;
using System.Collections.Generic;
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
        GetReference();

        if (isMainVCam)
        UpdateScale();
    }

    private void Update()
    {
        var overlappingColliders = Physics2D.OverlapAreaAll(edgeTransform1.position, edgeTransform2.position, mask);
        if (overlappingColliders.Length != currentCollider)
        {
            VCamColliderUtiliity.CreateCollider(gameObject, ref virtualcameraPartner, ref currentCollider, ref colliderList, ref overlappingColliders, overlappingColliders.Length - currentCollider);
        }

        VCamColliderUtiliity.RedrawCollider(ref virtualcameraPartner, ref colliderList, overlappingColliders);
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