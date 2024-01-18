using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class VCamMovementController : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Camera mainCam;
    [SerializeField] private Renderer objRenderer;
    [SerializeField] private Vector2 lastPosition;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector2 desiredPosition;
    [SerializeField] private float objectWidth = 0f;
    [SerializeField] private float objectHeight = 0f;
    [SerializeField] private bool isControlled = false;
    [SerializeField] private Transform vcamOther = null;
    [SerializeField] bool canMoveHorizontal;
    [SerializeField] bool canMoveVertical;
    [SerializeField] bool lockedFromHorizontal;
    [SerializeField] bool lockedFromVertical;
    private Rigidbody2D rb2d;

    #region MONO METHOD
    private void Awake()
    {
        Initialize();
    }

    private void OnMouseDown()
    {
        isControlled = true;
        offset = transform.position - mainCam.ScreenToWorldPoint(Input.mousePosition);
    }
    private void OnMouseDrag()
    {
        VcamMovement();
    }
    private void OnMouseUp()
    {
        isControlled = false;
    }

    #endregion


    #region PRIVATE METHOD
    /// <summary>
    /// Initialize something
    /// </summary>
    private void Initialize()
    {
        mainCam = Camera.main;
        objRenderer = GetComponent<Renderer>();
        rb2d = GetComponent<Rigidbody2D>();
        if (objRenderer != null)
        {
            objectWidth = objRenderer.bounds.extents.x;
            objectHeight = objRenderer.bounds.extents.y;
        }
    }

    private void VcamMovement()
    {
        //move virtual cam
        Vector2 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) + offset;

        Vector2 cornerPoint = new Vector2(transform.localScale.x / 2.1f, transform.localScale.y / 2.1f);
        Collider2D overlapCollider = null;

        RaycastVCam(lastPosition + cornerPoint, newPosition, ref desiredPosition, ref overlapCollider);
        RaycastVCam(lastPosition - cornerPoint, newPosition, ref desiredPosition, ref overlapCollider);
        RaycastVCam(lastPosition + new Vector2(cornerPoint.x, -cornerPoint.y), newPosition, ref desiredPosition, ref overlapCollider);
        RaycastVCam(lastPosition + new Vector2(-cornerPoint.x, cornerPoint.y), newPosition, ref desiredPosition, ref overlapCollider);

        Vector2 colliderCenter = newPosition;
        if (overlapCollider == null)
        {
            var overlapColliders = Physics2D.OverlapAreaAll(colliderCenter + cornerPoint, colliderCenter - cornerPoint, layerMask);
            foreach (Collider2D collider in overlapColliders)
            {
                if (collider.gameObject != gameObject)
                {
                    overlapCollider = collider;
                    desiredPosition = GetClosestPosition(lastPosition - (Vector2)collider.transform.position, collider.transform.position, transform.localScale.x / 2);
                    break;
                }
            }
        }



        Vector2 destination = new Vector2((canMoveHorizontal) ? newPosition.x : transform.position.x
                    , (canMoveVertical) ? newPosition.y : transform.position.y);

        if (isControlled && overlapCollider != null)
        {
            float horizontalDiff = Mathf.Abs(vcamOther.position.x - desiredPosition.x);
            float verticalDiff = Mathf.Abs(vcamOther.position.y - desiredPosition.y);
            if ((horizontalDiff >= transform.localScale.x - 0.1f &&
                horizontalDiff <= transform.localScale.x + 0.1f))//lock horizontal movement
            {
                if (desiredPosition.x < vcamOther.position.x)
                {
                    if (transform.position.x >= desiredPosition.x)
                        destination.x = vcamOther.position.x - vcamOther.localScale.x;
                }
                else if (desiredPosition.x > vcamOther.position.x)
                {
                    if (transform.position.x <= desiredPosition.x)
                        destination.x = vcamOther.position.x + vcamOther.localScale.x;
                }
            }
            else if ((verticalDiff >= transform.localScale.y - 0.1f &&
                verticalDiff <= transform.localScale.y + 0.1f))//lock vertical movement
            {
                if (desiredPosition.y < vcamOther.position.y)
                {
                    if (transform.position.y >= desiredPosition.y)
                        destination.y = vcamOther.position.y - vcamOther.localScale.y;
                }
                else if (desiredPosition.y > vcamOther.position.y)
                {
                    if (transform.position.y <= desiredPosition.y)
                        destination.y = vcamOther.position.y + vcamOther.localScale.y;
                }
            }
        }
        rb2d.MovePosition(destination);

        //clamp with camera device
        ClampVcamWihBoundingCamera();

        //save the last positon
        lastPosition = transform.position;
    }

    private Collider2D RaycastVCam(Vector2 origin, Vector2 destination, ref Vector2 closestPos, ref Collider2D colliderResult)
    {
        if (colliderResult != null)
        {
            return colliderResult;
        }


        Collider2D collider = null;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, destination - origin, (destination - origin).magnitude, layerMask);
        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject != gameObject)
            {
                collider = hit.collider;
                colliderResult = hit.collider;
                closestPos = GetClosestPosition(lastPosition - (Vector2)hit.collider.transform.position, hit.collider.transform.position, transform.localScale.x / 2);
                break;
            }
        }

        return collider;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="otherCollider"></param>
    /// <param name="size">length from the edge of the square to the center</param>
    /// <returns></returns>
    private Vector2 GetClosestPosition(Vector2 direction, Vector2 otherCollider, float size)
    {
        direction.Normalize();// 
        float magnitude = 1.01f;

        //horizontal
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            magnitude *= size * 2 / direction.x;
        }
        //vertical
        else
        {
            magnitude *= size * 2 / direction.y;
        }

        Vector2 pos = otherCollider + (direction * Mathf.Abs(magnitude));
        return pos;
    }

    /// <summary>
    /// Use to clamp plane position to not going outside of the camera while we playing, USED on PlayerMovement Method
    /// </summary>
    private void ClampVcamWihBoundingCamera()
    {
        if (mainCam == null || objRenderer == null)
            return;

        #region CLAMP WITH CAMERA
        float minX = mainCam.ViewportToWorldPoint(new Vector2(0, 0)).x + objectWidth;
        float maxX = mainCam.ViewportToWorldPoint(new Vector2(1, 0)).x - objectWidth;

        float minY = mainCam.ViewportToWorldPoint(new Vector2(0, 0)).y + objectHeight;
        float maxY = mainCam.ViewportToWorldPoint(new Vector2(0, 1)).y - objectHeight;

        Vector2 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(transform.position.x, minX, maxX);
        clampedPosition.y = Mathf.Clamp(transform.position.y, minY, maxY);
        transform.position = clampedPosition;
        #endregion
    }
    #endregion
}
