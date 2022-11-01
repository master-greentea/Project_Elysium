using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class EnemyFov : MonoBehaviour
{
    [Header("Wedge Mesh")]
    [SerializeField] public float sightRange = 10;
    [SerializeField] [Range(15, 90)] private float angle = 30;
    [SerializeField] private float height = 1f;
    [SerializeField] private float heightOffset;
    [SerializeField] private Color meshColor = Color.red;
    private Mesh fovMesh;

    [Space(10)] [Header("Object Detection")]
    [SerializeField] private int scanFrequency = 30;
    [SerializeField] public LayerMask detectingLayers;
    [SerializeField] public LayerMask obstacleLayers;
    [SerializeField] private float absoluteRange;
    [HideInInspector] public List<GameObject> objectsDetected;

    private readonly Collider[] collidersInRange = new Collider[50];
    private readonly Collider[] collidersInAbsoluteRange = new Collider[50];
    private int scanCounter;
    private int scanAbsoluteCounter;
    private float scanInterval;
    private float scanTimer;

    private void Start()
    {
        scanInterval = 1f / scanFrequency;
    }

    private void Update()
    {
        scanTimer -= Time.deltaTime;
        if (scanTimer < 0)
        {
            scanTimer += scanInterval;
            Scan();
        }
    }

    void Scan()
    {
        // scan for sight range
        scanCounter = Physics.OverlapSphereNonAlloc(transform.position, sightRange, collidersInRange, detectingLayers,
            QueryTriggerInteraction.Collide);
        // scan for absolute range
        scanAbsoluteCounter = Physics.OverlapSphereNonAlloc(transform.position, absoluteRange, collidersInAbsoluteRange, detectingLayers,
            QueryTriggerInteraction.Collide);

        objectsDetected.Clear();
        for (var i = 0; i < scanCounter; ++i)
        {
            GameObject obj = collidersInRange[i].gameObject;
            if (IsInSight(obj) || IsInAbsoluteRange(obj))
            {
                objectsDetected.Add(obj);
            }
        }
    }

    private bool IsInSight(GameObject obj)
    {
        // if object is in vision height
        Vector3 origin = transform.position;
        Vector3 dest = obj.transform.position;
        Vector3 direction = dest - origin;
        if (direction.y < heightOffset || direction.y > height)
        {
            return false;
        }
        // if object is in vision angle
        direction.y = heightOffset;
        float deltaAngle = Vector3.Angle(direction, transform.forward);
        if (deltaAngle > angle)
        {
            return false;
        }
        // if object is not blocked by obstacle
        origin.y += height / 2;
        dest.y = origin.y;
        if (Physics.Linecast(origin, dest, obstacleLayers))
        {
            return false;
        }
        
        return true;
    }

    private bool IsInAbsoluteRange(GameObject obj)
    {
        for (int i = 0; i < scanAbsoluteCounter; ++i)
        {
            if (collidersInAbsoluteRange[i].gameObject.CompareTag(obj.tag))
            {
                return true;
            }
        }

        return false;
    }

    private Mesh CreateWedgeMesh()
    {
        Mesh mesh = new Mesh();

        int segments = 10;
        int numTriangles = (segments * 4) + 2 + 2;
        int numVertices = numTriangles * 3;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numVertices];

        // create traingles
        Vector3 bottomCenter = Vector3.zero + new Vector3(0, heightOffset, 0);
        Vector3 bottomLeft = Quaternion.Euler(0, -angle, 0) * Vector3.forward * sightRange + new Vector3(0, heightOffset, 0);
        Vector3 bottomRight = Quaternion.Euler(0, angle, 0) * Vector3.forward * sightRange + new Vector3(0, heightOffset, 0);

        Vector3 topCenter = bottomCenter + Vector3.up * height;
        Vector3 topLeft = bottomLeft + Vector3.up * height;
        Vector3 topRight = bottomRight + Vector3.up * height;

        int vert = 0;
        
        // left
        vertices[vert++] = bottomCenter;
        vertices[vert++] = bottomLeft;
        vertices[vert++] = topLeft;
        
        vertices[vert++] = topLeft;
        vertices[vert++] = topCenter;
        vertices[vert++] = bottomCenter;
        
        // right
        vertices[vert++] = bottomCenter;
        vertices[vert++] = topCenter;
        vertices[vert++] = topRight;
        
        vertices[vert++] = topRight;
        vertices[vert++] = bottomRight;
        vertices[vert++] = bottomCenter;

        float currentAngle = -angle;
        float deltaAngle = (angle * 2) / segments;
        for (int i = 0; i < segments; ++i)
        {
            bottomLeft = Quaternion.Euler(0, currentAngle, 0) * Vector3.forward * sightRange + new Vector3(0, heightOffset, 0);
            bottomRight = Quaternion.Euler(0, currentAngle + deltaAngle, 0) * Vector3.forward * sightRange + new Vector3(0, heightOffset, 0);
            
            topLeft = bottomLeft + Vector3.up * height;
            topRight = bottomRight + Vector3.up * height;
            
            // far
            vertices[vert++] = bottomLeft;
            vertices[vert++] = bottomRight;
            vertices[vert++] = topRight;
        
            vertices[vert++] = topRight;
            vertices[vert++] = topLeft;
            vertices[vert++] = bottomLeft;
        
            // top
            vertices[vert++] = topCenter;
            vertices[vert++] = topLeft;
            vertices[vert++] = topRight;
        
            // bottom
            vertices[vert++] = bottomCenter;
            vertices[vert++] = bottomRight;
            vertices[vert++] = bottomLeft;
            
            currentAngle += deltaAngle;
        }

        for (var i = 0; i < numVertices; i++)
        {
            triangles[i] = i;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }

    private void OnValidate()
    {
        fovMesh = CreateWedgeMesh();
        scanInterval = 1f / scanFrequency;
    }

    private void OnDrawGizmos()
    {
        // draw wedge mesh
        if (fovMesh)
        {
            Gizmos.color = meshColor;
            Gizmos.DrawMesh(fovMesh, transform.position, transform.rotation);
        }
        
        // draw yellow for sight range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        
        // draw red for absolute range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, absoluteRange);
        
        // draw green around detected objects
        Gizmos.color = Color.green;
        foreach (var obj in objectsDetected)
        {
            Gizmos.DrawSphere(obj.transform.position, .2f);
        }
    }
}
