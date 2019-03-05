﻿using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct PerlinNoiseLayerJob : IJobParallelFor
{
    public NativeArray<Vector3> vertices;
    public PerlinNoiseLayer layer;
    public float time;
    public void Execute(int i)
    {
        Debug.Log($"executing at {i}");
        var vertex = vertices[i];
        var x = vertex.x * layer.Scale + time * layer.Speed;
        var z =  vertex.z * layer.Scale + time * layer.Speed;
        vertex.y += (Mathf.PerlinNoise(x, z) - 0.5f) * layer.Height;

        vertices[i] = vertex;
    }
}
