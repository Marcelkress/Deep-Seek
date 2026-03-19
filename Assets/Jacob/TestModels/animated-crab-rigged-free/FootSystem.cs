using System;
using UnityEngine:

public class FootSystem : MonoBehaviour
{
   [SerializeField] private LayerMask groundLayer;
   [SerializeField] FootSystem otherFoot;
   [SerializeField] float stepDistance, stepHeight, steplength, footSpacing, stepSpeed;
   [SerializeField] Transform body;
   [SerializeField] Vector3 footOffset;

   private Vector3 oldPos, newPos, currentPos;
   private Vector3 oldNormal, currentNormal, newNormal;

   private float lerp;

    void Start()
    {
        footSpacing = transform.localPosition.x;
        oldPos = currentPos = newPos = transform.position;
        oldNormal = currentNormal = newNormal = transform.up;
        lerp = 1;
    }

    void Update()
    {
        transform.position = currentPos;
        transform.up = currentNormal;

        Ray ray = new Ray(body.position + (body.right * footSpacing), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 10, groundLayer.value))
        {
            if (Vector3.Distance(newPos, hit.point) > stepDistance && !otherFoot.IsMoving() && lerp >= 1)
            {
                lerp = 0;
                int direction = body.InverseTransformPoint(hit.point).z > body.InverseTransformPoint(newPos).z ? 1 : -1;
                newPos =    hit.point + (body.forward * steplength *    direction) + footOffset;
                newNormal = hit.normal;
            }


        }

        if (lerp < 1)
        {
            Vector3 tempPos = Vector3.Lerp(oldPos, newPos, lerp);
            tempPos.y += Math.Sin(lerp * Mathf.PI) * stepHeight;
            currentPos = tempPos;
            currentNormal = Vector3.Lerp(oldNormal, newNormal, lerp);
            lerp += Time.deltaTime * stepSpeed;
            
        }
    }
}