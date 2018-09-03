﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallaxer : MonoBehaviour {

    class PoolObject {
        public Transform transform;
        public bool inUse;
        public PoolObject(Transform t) { transform = t; }
        public void Use() { inUse = true; }
        public void Dispose() { inUse = false; }
    }

    [System.Serializable]
    public struct YSpawnRange{
        public float min;
        public float max;
    }

    public GameObject Prefab;
    public int poolSize;
    public float shiftSpeed;
    public float spawnRate;

    public YSpawnRange ySpawnRange;
    public Vector3 defaultSpawnPos;
    public Vector3 immediateSpawnPos;
    public bool spawnImmediate;
    public Vector2 targetAspectRatio;

    float spawnTimer;
    float targetAspect;
    PoolObject[] poolObjects;
    GameManager game;
    public bool canReset = false;
	private void Awake()
	{
        Configure();
	}

	private void Start()
	{
        game = GameManager.Instance;
	}

	private void OnEnable()
	{
        GameManager.OnReset += OnReset;
	}

	private void OnDisable()
	{
        GameManager.OnReset -= OnReset;
	}

    void OnReset(){
        if(canReset){
            for (int i = 0; i < poolObjects.Length; i++)
            {
                poolObjects[i].Dispose();
                poolObjects[i].transform.position = Vector3.one * 1000;
            }
            if (spawnImmediate) SpawnImmediate();
        }
    }

    void Update()
    {
        Shift();
        spawnTimer += Time.deltaTime;
        if(spawnTimer > spawnRate){
            spawnTimer = 0;
            Spawn();
        }
    }

    void Configure(){
        targetAspect = targetAspectRatio.x / targetAspectRatio.y;
        poolObjects = new PoolObject[poolSize];
        for (int i = 0; i < poolObjects.Length; i++){
            GameObject go = Instantiate(Prefab) as GameObject;
            Transform t = go.transform;
            t.SetParent(transform);
            t.position = Vector3.one * 1000;
            poolObjects[i] = new PoolObject(t);
        }
        if (spawnImmediate) SpawnImmediate();
    }

    void Spawn(){
        Transform t = GetPoolObject();
        if (t == null) return;
        var pos = Vector3.zero;
        pos.x = defaultSpawnPos.x * Camera.main.aspect / targetAspect;
        pos.y = Random.Range(ySpawnRange.min, ySpawnRange.max);
        t.position = pos;
    }

    void SpawnImmediate(){
        Transform t = GetPoolObject();
        if (t == null) return;
        var pos = Vector3.zero;
        pos.x = immediateSpawnPos.x * Camera.main.aspect / targetAspect;
        pos.y = Random.Range(ySpawnRange.min, ySpawnRange.max);
        t.position = pos;
        Spawn();
    }

    void Shift(){
        for (int i = 0; i < poolObjects.Length; i++){
            poolObjects[i].transform.position += -Vector3.right * shiftSpeed * Time.deltaTime;
            CheckDisposeObject(poolObjects[i]);
        }
    }

    void CheckDisposeObject(PoolObject poolObject){
        if(poolObject.transform.position.x < (-defaultSpawnPos.x * Camera.main.aspect)/targetAspect){
            poolObject.Dispose();
            poolObject.transform.position = Vector3.one * 1000;
        }
        
    }
    Transform GetPoolObject(){
        for (int i = 0; i < poolObjects.Length; i++){
            if(!poolObjects[i].inUse){
                poolObjects[i].Use();
                return poolObjects[i].transform;
            }
        }
        return null;
    }

}
