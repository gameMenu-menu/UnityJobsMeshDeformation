using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
public class Changer : MonoBehaviour
{
     [BurstCompile(CompileSynchronously = true)]
    struct ChangeColorJob: IJobParallelFor
    {
        public NativeArray<Color> Colors;
        public NativeArray<Vector3> Vertex;
        public NativeArray<Color> MainTextureColors, SideTextureColors;
        public Vector3 Position;
        public NativeArray<int> ChangedIndexes;
        public NativeArray<float> MovingIndexes;
        public float MaxDistance;
        public float SmoothNumber;
        public void Execute (int index)
        {
            float distance = (Vertex[index] - Position).sqrMagnitude;
            if( distance < MaxDistance )
            {
                ChangedIndexes[index] = 1;
                Colors[index] = SideTextureColors[index];
                /*Vertex[index] += new Vector3(0, Mathf.Sin(MovingIndexes[index]), 0);
​
                MovingIndexes[index] += 0.02f;*/
                Vertex[index] += new Vector3(0, 0, Mathf.Sin( ( MovingIndexes[index]) - 0.5f ) / 120f);
                MovingIndexes[index] += 0.2f;
                
            }
            else if( ChangedIndexes[index] == 0 )
            {
                //Colors[index] = MainTextureColors[index];
                //MovingIndexes[index] += 0.16f;
            }
            else
            {
                Vertex[index] += new Vector3(0, 0, Mathf.Sin( ( MovingIndexes[index]) - 0.5f ) / 120f);
                MovingIndexes[index] += 0.2f;
            }
        }
    }
    public MeshFilter Filter;
    ChangeColorJob colorJob;
    JobHandle handle;
    NativeArray<Color> Colors;
    NativeArray<Vector3> Vertices;
    public Transform Position;
    Color[] NewColors;
    public float MaxDistance;
    NativeArray<Vector2> UV;
    public Texture2D MainTexture, SideTexture;
    public NativeArray<int> ChangedIndexes;
    public NativeArray<Color> MainTextureColors, SideTextureColors;
    public NativeArray<float> MovingIndexes;
    void Start()
    {
        
        Colors = new NativeArray<Color>(Filter.mesh.vertices.Length, Allocator.Persistent);
        Vertices = new NativeArray<Vector3>(Filter.mesh.vertices, Allocator.Persistent);
        ChangedIndexes = new NativeArray<int>(Vertices.Length, Allocator.Persistent);
        MovingIndexes = new NativeArray<float>(Vertices.Length, Allocator.Persistent);
        MainTextureColors = new NativeArray<Color>(Filter.mesh.vertices.Length, Allocator.Persistent);
        SideTextureColors = new NativeArray<Color>(Filter.mesh.vertices.Length, Allocator.Persistent);
        UV = new NativeArray<Vector2>(Filter.mesh.uv, Allocator.Persistent);
        
        for(int i=0; i<ChangedIndexes.Length; i++)
        {
            ChangedIndexes[i] = 0;
        }
        for(int i=0; i<MovingIndexes.Length; i++)
        {
            MovingIndexes[i] = 0f;
        }
        for(int i=0; i<MainTextureColors.Length; i++)
        {
            MainTextureColors[i] = MainTexture.GetPixel( (int) ( UV[i].x * MainTexture.width ),  (int) ( UV[i].y * MainTexture.height));
            Colors[i] = MainTextureColors[i];
        }
        for(int i=0; i<SideTextureColors.Length; i++)
        {
            SideTextureColors[i] = SideTexture.GetPixel( (int) ( UV[i].x * SideTexture.width ),  (int) ( UV[i].y * SideTexture.height));
        }
        
        NewColors = new Color[Colors.Length];
    }
    void Update()
    {
        colorJob = new ChangeColorJob { };

        colorJob.Colors = Colors;
        colorJob.Vertex = Vertices;
        colorJob.Position = Position.localPosition;
        colorJob.MaxDistance = MaxDistance;
        colorJob.ChangedIndexes = ChangedIndexes;
        colorJob.MainTextureColors = MainTextureColors;
        colorJob.SideTextureColors = SideTextureColors;
        colorJob.MovingIndexes = MovingIndexes;

        handle = colorJob.Schedule(Colors.Length, 1);
    }
    void LateUpdate()
    {
        handle.Complete();
        for(int i=0; i<Colors.Length; i++)
        {
            NewColors[i] = Colors[i];
        }
        Vector3[] verts = new Vector3[Vertices.Length];
        for(int i=0; i<verts.Length; i++)
        {
            verts[i] = Vertices[i];
        }
        Filter.mesh.vertices = verts;
        Filter.mesh.colors = NewColors;
        Filter.mesh.RecalculateNormals();
        for(int i=0; i<MovingIndexes.Length; i++)
        {
            MovingIndexes[i] += Random.Range(-0.03f, 0.03f);
        }
        
        
        
    }
    void OnDestroy()
    {
        Colors.Dispose();
        Vertices.Dispose();
        ChangedIndexes.Dispose();
        UV.Dispose();
        MainTextureColors.Dispose();
        SideTextureColors.Dispose();
        MovingIndexes.Dispose();
        
    }
}