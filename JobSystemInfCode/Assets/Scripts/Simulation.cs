﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class Simulation : MonoBehaviour
{
    [SerializeField] private bool _useJobSystem;
    [SerializeField] private MeshFilter _meshFilter;
    [SerializeField] private List<PerlinNoiseLayer> _perlinNoiseLayers;
    // Start is called before the first frame update
    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    void Start()
    {
        _meshFilter.mesh.MarkDynamic();
    }

    // Update is called once per frame
    void Update()
    {
        var vertices = _meshFilter.mesh.vertices;

        FlattenMesh(vertices);

        if (_useJobSystem)
        {
            ExecutePerlinNoiseJob(vertices);
        }
        else
        {
            foreach (var layer in _perlinNoiseLayers)
            {
                AddPerlinNoise(vertices, layer, Time.timeSinceLevelLoad);
            }
        }

        _meshFilter.mesh.SetVertices(vertices.ToList());
        _meshFilter.mesh.RecalculateNormals();
    }


    private void FlattenMesh(Vector3[] vertices)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = 0;
        }
    }

    private void AddPerlinNoise(Vector3[] vertices, PerlinNoiseLayer layer, float time)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            var x = vertices[i].x * layer.Scale + time * layer.Speed;
            var z = vertices[i].z * layer.Scale + time * layer.Speed;

            vertices[i].y += (Mathf.PerlinNoise(x, z) - 0.5f) * layer.Height;
        }
    }

    private void ExecutePerlinNoiseJob(Vector3[] vertices)
    {
        foreach (var layer in _perlinNoiseLayers)
        {
            var vertexArray = new NativeArray<Vector3>(vertices, Allocator.TempJob);
            
            var job = new PerlinNoiseLayerJob()
            {
                vertices = vertexArray,
                layer =  layer,
                time =  Time.timeSinceLevelLoad
            };
            JobHandle jobHandle = job.Schedule(vertices.Length, 250);
            jobHandle.Complete();
            
            vertexArray.CopyTo(vertices);
            vertexArray.Dispose();
        }
    }
}
