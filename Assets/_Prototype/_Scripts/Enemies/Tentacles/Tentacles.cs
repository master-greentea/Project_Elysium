using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Tentacles : MonoBehaviour
{
    private Vector3 lastPosition;
    [SerializeField] private float checkDistanceInterval;
    private float checkDistanceTimer;
    [SerializeField] private float deltaDistance;
    
    [Header("Attachable Points")]
    [SerializeField] private LayerMask attachableLayers;
    [SerializeField] private float attachableRadius;
    private List<Vector3> attachablePoints = new List<Vector3>();

    [Header("Tentacle Values")]
    public GameObject tentaclePrefab;
    [SerializeField] private int tentacleAmount;
    private List<FABRIK> tentacleFABRIKs;
    private List<GameObject> spawnedPrefabs;

    private void OnValidate() {
        // apply scale and roll at each node
    }
    
    void Start()
    {
        lastPosition = transform.position;
        tentacleFABRIKs = new List<FABRIK>();
        for (int i = 0; i < tentacleAmount; i++)
        {
            FABRIK f = Instantiate(tentaclePrefab, transform).GetComponent<FABRIK>();
            tentacleFABRIKs.Add(f);
            Debug.Log("tentacle added");
        }
    }
    
    void Update()
    {
        CheckDistance();

        foreach (var f in tentacleFABRIKs)
        {
            foreach (var point in attachablePoints)
            {
                f.effectorPoint = point;
                attachablePoints.Remove(point);
                break;
            }
        }
        
    }

    void CheckDistance()
    {
        // check based on time
        checkDistanceTimer -= Time.deltaTime;
        if (checkDistanceTimer > 0) return;
        checkDistanceTimer = checkDistanceInterval;
        // if moved
        // if ((transform.position - lastPosition).magnitude < deltaDistance) return;
        // lastPosition = transform.position;
        // do stuff
        GetPoints();
    }

    void GetPoints()
    {
        // foreach (var t in spawnedPrefabs)
        // {
        //     Destroy(t);
        // }
        // spawnedPrefabs.Clear();
        attachablePoints.Clear();
        foreach (var col in Physics.OverlapSphere(transform.position, attachableRadius, attachableLayers, QueryTriggerInteraction.Collide))
        {
            Vector3[] verts = col.GetComponent<MeshFilter>().mesh.vertices;
            foreach (var vert in verts)
            {
                Vector3 worldVert = col.transform.TransformPoint(vert);
                Physics.Raycast(transform.position, (worldVert - transform.position).normalized, out RaycastHit hit, attachableRadius, attachableLayers);
                // find valid points
                if (hit.point == worldVert )
                {
                    attachablePoints.Add(hit.point);
                    // spawnedPrefabs.Add(f.gameObject);
                    // tentacleGuider.position = hit.point;
                    Debug.DrawLine(transform.position, hit.point, Color.green, checkDistanceInterval - .01f);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attachableRadius);
    }
}
