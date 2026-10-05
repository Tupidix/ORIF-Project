using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappableCutlery : MonoBehaviour
{
    [SerializeField]
    private GameObject _prefabModel;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstantiatePrefab(Vector3 position, Quaternion rotation) {
        Instantiate(_prefabModel, position, rotation);
    }
}
