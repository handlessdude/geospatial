using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Niantic.Lightship.AR.WorldPositioning;
using Niantic.Lightship.AR.XRSubsystems;
using UnityEngine.UI;

public class AddWPSObjects : MonoBehaviour
{
    [SerializeField] ARWorldPositioningObjectHelper PositioningObjectHelper;
    [SerializeField] private ARWorldPositioningManager PositioningManager;
    
    // [SerializeField] private List<Material> materials;
    [SerializeField] private List<ParticleSystem> particleSystems;
    [SerializeField] private List<GameObject> prefabs;

    private ARWorldPositioningCameraHelper PositioningCameraHelper;
    

    void Start()
    {
        PositioningCameraHelper = FindObjectOfType<ARWorldPositioningCameraHelper>();
        PositioningManager.OnStatusChanged += OnStatusChanged;
    }

    private void OnStatusChanged(WorldPositioningStatus obj)
    {
        MyLogger.Log("WPS Status changed: " + obj);
    }

    public void SpawnObjectAtPosition()
    {
        double latitude = PositioningCameraHelper.Latitude;
        double longitude = PositioningCameraHelper.Longitude;
        double altitude = 0.0; 

        /*
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<MeshRenderer>().material = materials[Random.Range(0, materials.Count-1)];
        cube.transform.localScale *= 0.25f;
        cube.AddComponent<DiagonalRotation>();
        */
        if (prefabs.Count == 0)
        {
            MyLogger.Log("ERROR: No prefabs assigned in the list.");
            return;
        }
        GameObject newAnchor = Instantiate(
            prefabs[Random.Range(0, prefabs.Count)],
            Vector3.zero,
            Quaternion.identity
        );
        newAnchor.transform.localScale *= 1.2f;
        newAnchor.AddComponent<DiagonalRotation>();
        
        if (particleSystems.Count > 0)
        {
            ParticleSystem selectedParticleSystem = Instantiate(
                particleSystems[Random.Range(0, particleSystems.Count)],
                newAnchor.transform.position,
                Quaternion.identity
            );
            selectedParticleSystem.transform.SetParent(newAnchor.transform);
            selectedParticleSystem.Play();
        }
        
        PositioningObjectHelper.AddOrUpdateObject(newAnchor, latitude, longitude, altitude, Quaternion.identity);
        
        MyLogger.Log($"Anchor placed at: (lat = {latitude}; long = {longitude})");
    }
}