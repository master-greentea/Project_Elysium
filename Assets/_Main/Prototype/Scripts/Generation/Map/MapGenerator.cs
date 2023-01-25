using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    private GameObject[] sections;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void GetCurrentSection()
    {
        sections = GameObject.FindGameObjectsWithTag("Section");
    }
}
