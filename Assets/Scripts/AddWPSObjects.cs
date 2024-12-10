using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Niantic.Lightship.AR.WorldPositioning;
using Niantic.Lightship.AR.XRSubsystems;
using UnityEngine.UI;
using System.IO;
using Random = UnityEngine.Random;

[System.Serializable]
public class SerializedObjectData
{
    public double Latitude;
    public double Longitude;
    public int PrefabIndex;
    public int ParticleSystemIndex;
}

[System.Serializable]
public class SerializedObjectList
{
    public List<SerializedObjectData> Objects = new List<SerializedObjectData>();
}

public class AddWPSObjects : MonoBehaviour
{
    [SerializeField] ARWorldPositioningObjectHelper PositioningObjectHelper;
    [SerializeField] private ARWorldPositioningManager PositioningManager;
    [SerializeField] private List<ParticleSystem> particleSystems;
    [SerializeField] private List<GameObject> prefabs;

    private ARWorldPositioningCameraHelper PositioningCameraHelper;
    
    private string saveFilePath;

    private SerializedObjectList spawnedObjects;
    private List<GameObject> instantiatedObjects = new();
    
    private bool _isInitialized = false;

    private WorldPositioningStatus _wpsStatus = WorldPositioningStatus.SubsystemNotRunning;
    void Start()
    {
        PositioningCameraHelper = FindObjectOfType<ARWorldPositioningCameraHelper>();
        PositioningManager.OnStatusChanged += OnStatusChanged;
        
        saveFilePath = Path.Combine(Application.persistentDataPath, "db.json");
        LoadSpawnedObjects();
    }
    
    private void OnStatusChanged(WorldPositioningStatus newVal)
    {
        _wpsStatus = newVal;
        MyLogger.Log("WPS Status changed: " + newVal);
        
        if (!_isInitialized && newVal == WorldPositioningStatus.Available)
        {
            RespawnSavedObjects();
            _isInitialized = true;
        }
    }

    public void SpawnObjectAtPosition()
    {
        double latitude = PositioningCameraHelper.Latitude;
        double longitude = PositioningCameraHelper.Longitude;
        
        if (prefabs.Count == 0)
        {
            MyLogger.Log("ERROR: No prefabs assigned in the list.");
            return;
        }
        
        if (particleSystems.Count == 0)
        {
            MyLogger.Log("ERROR: No particleSystems assigned in the list.");
            return;
        }

        int prefabIndex = Random.Range(0, prefabs.Count);
        int particleSystemIndex = Random.Range(0, particleSystems.Count);
        
        SpawnObject(latitude, longitude, prefabIndex, particleSystemIndex);
        
        SaveSpawnedObject(latitude, longitude, prefabIndex, particleSystemIndex);
    }

    private void SpawnObject(double latitude, double longitude, int prefabIndex, int particleSystemIndex)
    {
        try
        {
            double altitude = 0.0; 
            
            GameObject newAnchor = Instantiate(
                prefabs[prefabIndex],
                Vector3.zero,
                Quaternion.identity
            );
            instantiatedObjects.Add(newAnchor);
                
            newAnchor.AddComponent<DiagonalRotation>();
            
            ParticleSystem selectedParticleSystem = Instantiate(
                particleSystems[particleSystemIndex],
                newAnchor.transform.position,
                Quaternion.identity
            );
            selectedParticleSystem.transform.SetParent(newAnchor.transform);
            selectedParticleSystem.Play();
            
            PositioningObjectHelper.AddOrUpdateObject(newAnchor, latitude, longitude, altitude, Quaternion.identity);
            
            MyLogger.Log($"Anchor placed at: (lat = {latitude}; long = {longitude})");
        }
        catch (Exception e)
        {
            MyLogger.Log(e.Message);
            throw;
        }
    }
    
    private void SaveSpawnedObject(double latitude, double longitude, int prefabIndex, int particleSystemIndex)
    {
        SerializedObjectData newData = new SerializedObjectData
        {
            Latitude = latitude,
            Longitude = longitude,
            PrefabIndex = prefabIndex,
            ParticleSystemIndex = particleSystemIndex
        };

        spawnedObjects.Objects.Add(newData);
        SaveSpawnedObjectsToFile();
    }
    
    private void SaveSpawnedObjectsToFile()
    {
        string json = JsonUtility.ToJson(spawnedObjects);
        File.WriteAllText(saveFilePath, json);
    }

    private void LoadSpawnedObjects()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            spawnedObjects = JsonUtility.FromJson<SerializedObjectList>(json);
            MyLogger.Log($"Loaded anchors: {spawnedObjects.Objects.Count}");
        }
        else
        {
            spawnedObjects = new SerializedObjectList();
            MyLogger.Log($"No anchors data");
        }
    }
    
    private void RespawnSavedObjects()
    {
        foreach (var data in spawnedObjects.Objects)
        {
            if (
                data.PrefabIndex >= 0 && data.PrefabIndex < prefabs.Count
                && data.ParticleSystemIndex >= 0 && data.ParticleSystemIndex < particleSystems.Count
            )
            {
                SpawnObject(data.Latitude, data.Longitude, data.PrefabIndex, data.ParticleSystemIndex);
            }
        }
    }
    
    public void EraseAllSerializedObjects()
    {
        spawnedObjects.Objects.Clear();
        SaveSpawnedObjectsToFile();
        MyLogger.Log("All saved objects have been erased.");
    }
    
    public void EraseAllInstantiatedObjects()
    {
        PositioningObjectHelper.RemoveAllObjects();
        foreach (var obj in instantiatedObjects)
        {
            Destroy(obj);
        }
        instantiatedObjects.Clear();
        MyLogger.Log("All instantiated objects have been erased.");
    }
}