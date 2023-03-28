using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidanceBallBehavior : MonoBehaviour
{
    [SerializeField] private Transform guidanceBallTransform;
    private Transform t;
    private Vector3 guidanceDir;
    private float defaultDist;
    private float guidanceDist;
    
    void Start()
    {
        t = transform;
        defaultDist = (guidanceBallTransform.position - t.localPosition).magnitude;
    }
    
    void Update()
    {
        guidanceDir = (t.forward * .5f + new Vector3(0, .2f, 0)).normalized;
        RaycastHit hit;
        if (Physics.SphereCast(t.localPosition, .2f, guidanceDir, out hit, defaultDist,
                Services.EnemyAgent.enemyFov.obstacleLayers))
        {
            guidanceDist = hit.distance;
        }
        else guidanceDist = defaultDist;

        guidanceBallTransform.position = t.localPosition + guidanceDir * guidanceDist;
    }
}
