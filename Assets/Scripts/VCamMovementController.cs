using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[RequireComponent(typeof(VirtualCamera))]
public class VCamMovementController : MonoBehaviour
{
    [Tooltip("For getting Vcam Mask to check overlaping and raycast")]
    [SerializeField] private LayerMask layerMask;
    [Tooltip("To enable or disable the horizontal movement")]
    [SerializeField] bool canMoveHorizontal;
    [Tooltip("To enable or disable the vertical movement")]
    [SerializeField] bool canMoveVertical;
    /// <summary>
    /// save the last positon of the vcam
    /// </summary>
    private Vector2 lastPosition;
    /// <summary>
    /// position of where the vcam collied with other vcam
    /// </summary>
    private Vector2 desiredPosition;
    /// <summary>
    /// true if we touch the vcam, so we can controll it
    /// </summary>
    private bool isControlled = false;
    private VirtualCamera vCam;
    private Rigidbody2D rb2d;

    #region VARIABLE FOR CLAMP CAMERA PURPOSE
    /// <summary>
    /// sprite renderer of the VCAM, to get the width and height the renderer
    /// </summary>
    private Renderer objRenderer;
    private Camera mainCam;
    private float objectWidth = 0f;
    private float objectHeight = 0f;
    #endregion

    #region MONO METHOD
    private void Awake()
    {
        Initialize();
    }

    private void OnMouseDown()
    {
        isControlled = true;
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
        vCam = GetComponent<VirtualCamera>();
        if (objRenderer != null)
        {
            objectWidth = objRenderer.bounds.extents.x;
            objectHeight = objRenderer.bounds.extents.y;
        }
    }

    private void VcamMovement()
    {
        //get position of the cursor from the world point
        Vector2 newPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);

        //get the corner point of the virtual camera
        Vector2 cornerPoint = new Vector2(transform.localScale.x / 2.1f, transform.localScale.y / 2.1f);

        ///check for if we collide with the partner

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

        ///if we controll the camera, and its overlap with partner
        if (isControlled && overlapCollider != null)
        {
            //get the transform of the vcam partner
            Transform vcamPartnerTransform = vCam.virtualcameraPartner.transform;

            //get the different of the X axis from partner position with our desired to check we should lock horizontal movement or no
            float horizontalDiff = Mathf.Abs(vcamPartnerTransform.position.x - desiredPosition.x);

            //get the different of the Y axis from partner position with our desired to check we should lock vertical movement or no
            float verticalDiff = Mathf.Abs(vcamPartnerTransform.position.y - desiredPosition.y);

            if ((horizontalDiff >= transform.localScale.x - 0.1f &&
                horizontalDiff <= transform.localScale.x + 0.1f))//lock horizontal movement
            {
                if (desiredPosition.x < vcamPartnerTransform.position.x)
                {
                    // snap it to left
                    if (transform.position.x >= desiredPosition.x)
                        destination.x = vcamPartnerTransform.position.x - vcamPartnerTransform.localScale.x;
                }
                else if (desiredPosition.x > vcamPartnerTransform.position.x)
                {
                    // snap it to right
                    if (transform.position.x <= desiredPosition.x)
                        destination.x = vcamPartnerTransform.position.x + vcamPartnerTransform.localScale.x;
                }
            }
            else if ((verticalDiff >= transform.localScale.y - 0.1f &&
                verticalDiff <= transform.localScale.y + 0.1f))//lock vertical movement
            {
                if (desiredPosition.y < vcamPartnerTransform.position.y)
                {
                    // snap it to bottom
                    if (transform.position.y >= desiredPosition.y)
                        destination.y = vcamPartnerTransform.position.y - vcamPartnerTransform.localScale.y;
                }
                else if (desiredPosition.y > vcamPartnerTransform.position.y)
                {
                    // snap it to top
                    if (transform.position.y <= desiredPosition.y)
                        destination.y = vcamPartnerTransform.position.y + vcamPartnerTransform.localScale.y;
                }
            }
        }

        //set the transform position using rigidbody
        rb2d.MovePosition(destination);

        //clamp with camera device
        ClampVcamWihBoundingCamera();

        //save the last positon
        lastPosition = transform.position;
    }

    /// <summary>
    /// Get the collider of the partner virtual camera, using raycast
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="destination"></param>
    /// <param name="closestPos"></param>
    /// <param name="colliderResult"></param>
    /// <returns></returns>
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
    /// Calculate the closest positon vector on our movement
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
