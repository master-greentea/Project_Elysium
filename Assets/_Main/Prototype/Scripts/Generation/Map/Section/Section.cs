using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Section : MonoBehaviour
{
    [HideInInspector] public string sectionID;
    [HideInInspector] public Vector3 sectionOrigin;

    void Awake()
    {
        sectionID = gameObject.GetInstanceID().ToString();
        sectionOrigin = transform.position;
    }
}
