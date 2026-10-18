using System.Collections.Generic;
using UnityEngine;

public class Scanner : MonoBehaviour
{
    public float scanRange;
    public LayerMask targetLayer;
    // 初始预留容量，后续查询重复利用。
    private readonly List<RaycastHit2D> targets = new List<RaycastHit2D>(64);
    public Transform nearestTarget;

    private void FixedUpdate()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayer);
        filter.useTriggers = Physics2D.queriesHitTriggers;

        targets.Clear();

        Physics2D.CircleCast(
            transform.position,
            scanRange,
            Vector2.zero,
            filter,
            targets,
            0f);

        nearestTarget = GetNearest();
    }

    private Transform GetNearest()
    {
        Transform result = null;
        float diff = 100;

        foreach (RaycastHit2D target in targets)
        {
            Vector3 myPos = transform.position;
            Vector3 targetPos = target.transform.position;
            float curDiff = Vector3.Distance(myPos, targetPos);

            if (curDiff < diff)
            {
                diff = curDiff;
                result = target.transform;
            }
        }
        return result;
    }
}
