using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

namespace FH.Ed
{
    //Base 
    public static partial class ObjFileWriter
    {
        #region Base
        public struct FaceIndex
        {
            public int VertexIndex; //index start with 1
            public int UVIndex; //>=0
            public int NormalIndex; //>=0

            public FaceIndex(int vertex, int uv = -1, int normal = -1)
            {
                VertexIndex = vertex;
                UVIndex = uv;
                NormalIndex = normal;
            }

            public FaceIndex(int vertex, bool hasUV, bool hasNormal)
            {
                VertexIndex = vertex;
                UVIndex = hasUV ? vertex : 0;
                NormalIndex = hasNormal ? vertex : 0;
            }

            public static implicit operator FaceIndex(int i)
            {
                return new FaceIndex(i);
            }

            public void Write(StreamWriter sw)
            {
                if (UVIndex > 0 && NormalIndex > 0) // vertex/uv/normal
                {
                    sw.Write(VertexIndex);
                    sw.Write('/');
                    sw.Write(UVIndex);
                    sw.Write('/');
                    sw.Write(NormalIndex);
                }
                else if (UVIndex > 0 && NormalIndex <= 0) //  vertex/uv
                {
                    sw.Write(VertexIndex);
                    sw.Write('/');
                    sw.Write(UVIndex);
                }
                else if (UVIndex <= 0 && NormalIndex > 0) // vertex//normal
                {
                    sw.Write(VertexIndex);
                    sw.Write("//");
                    sw.Write(NormalIndex);
                }
                else // vertex
                {
                    sw.Write(VertexIndex);
                }
            }
        }

        public static void WriteVertex(StreamWriter sw, Vector3 vertex)
        {
            sw.Write("v ");
            sw.Write(vertex.x);
            sw.Write(" ");
            sw.Write(vertex.y);
            sw.Write(" ");
            sw.Write(vertex.z);
            sw.Write('\n');
        }
        public static void WriteVertices(StreamWriter sw, params Vector3[] vertices)
        {
            foreach (var p in vertices)
            {
                WriteVertex(sw, p);
            }
        }

        public static void WriteUV(StreamWriter sw, Vector2 uv)
        {
            sw.Write("vt ");
            sw.Write(uv.x);
            sw.Write(" ");
            sw.Write(uv.y);
            sw.Write('\n');
        }

        public static void WriteUVWs(StreamWriter sw, params Vector3[] uvws)
        {
            foreach (var p in uvws)
                WriteUVW(sw, p);
        }

        public static void WriteUVW(StreamWriter sw, Vector3 uvw)
        {
            sw.Write("vt ");
            sw.Write(uvw.x);
            sw.Write(" ");
            sw.Write(uvw.y);
            sw.Write(" ");
            sw.Write(uvw.z); ;
            sw.Write('\n');
        }

        public static void WriteUVs(StreamWriter sw, params Vector2[] uvs)
        {
            foreach (var p in uvs)
                WriteUV(sw, p);
        }

        public static void WriteNormal(StreamWriter sw, Vector3 normal)
        {
            sw.Write("vn ");
            sw.Write(normal.x);
            sw.Write(" ");
            sw.Write(normal.y);
            sw.Write(" ");
            sw.Write(normal.z);
            sw.Write('\n');
        }

        public static void WriteNormals(StreamWriter sw, params Vector3[] normals)
        {
            foreach (var p in normals)
            {
                WriteNormal(sw, p);
            }
        }

        public static void WriteFace(StreamWriter sw, params FaceIndex[] face_indexes)
        {
            sw.Write("f");
            foreach (var p in face_indexes)
            {
                sw.Write(' ');
                p.Write(sw);
            }
            sw.Write('\n');
        }

        /// <summary>
        /// index0,index1,index2 要从1开始
        /// </summary>        
        public static void WriteTriangle(StreamWriter sw, int index0, int index1, int index2, bool hasUV = false, bool hasNormal = false)
        {
            WriteFace(sw, new FaceIndex(index0, hasUV, hasNormal), new FaceIndex(index1, hasUV, hasNormal), new FaceIndex(index2, hasUV, hasNormal));
        }

        public static void WriteGroupName(StreamWriter sw, string name)
        {
            if (string.IsNullOrEmpty(name))
                return;

            sw.Write("g ");
            sw.Write(name);
            sw.Write('\n');
        }

        public static void WriteObjName(StreamWriter sw, string name)
        {
            sw.Write("o ");
            sw.Write(name);
            sw.Write('\n');
        }

        #endregion

    }


    //Unity
    public static partial class ObjFileWriter
    {
        public static void WriteCollider(StreamWriter sw, string group_name, UnityEngine.BoxCollider boxcollider)
        {
            WriteGroupName(sw, group_name);

            var v1 = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(boxcollider.size.x, boxcollider.size.y, -boxcollider.size.z) * 0.5f);
            var v2 = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(boxcollider.size.x, -boxcollider.size.y, -boxcollider.size.z) * 0.5f);
            var v3 = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(boxcollider.size.x, boxcollider.size.y, boxcollider.size.z) * 0.5f);
            var v4 = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(boxcollider.size.x, -boxcollider.size.y, boxcollider.size.z) * 0.5f);

            var v5 = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(-boxcollider.size.x, boxcollider.size.y, -boxcollider.size.z) * 0.5f);
            var v6 = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(-boxcollider.size.x, -boxcollider.size.y, -boxcollider.size.z) * 0.5f);
            var v7 = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(-boxcollider.size.x, boxcollider.size.y, boxcollider.size.z) * 0.5f);
            var v8 = boxcollider.transform.TransformPoint(boxcollider.center + new Vector3(-boxcollider.size.x, -boxcollider.size.y, boxcollider.size.z) * 0.5f);


            WriteVertices(sw, v1, v2, v3, v4, v5, v6, v7, v8);

            WriteFace(sw, 1, 5, 7, 3);
            WriteFace(sw, 4, 3, 7, 8);
            WriteFace(sw, 8, 7, 5, 6);
            WriteFace(sw, 6, 2, 4, 8);
            WriteFace(sw, 2, 1, 3, 4);
            WriteFace(sw, 6, 5, 1, 2);
        }

        public static void WriteMeshCollider(StreamWriter sw, string group_name, UnityEngine.MeshCollider meshCollider)
        {
            var sharedMesh = meshCollider.sharedMesh;
            if (sharedMesh == null)
            {
                return;
            }

            WriteGroupName(sw, group_name);

            Vector3 scale = Vector3.one;
            scale.x = Mathf.Sign(meshCollider.transform.lossyScale.x);
            scale.y = Mathf.Sign(meshCollider.transform.lossyScale.y);
            scale.z = Mathf.Sign(meshCollider.transform.lossyScale.z);

            {
                var meshVertices = new Vector3[sharedMesh.vertices.Length];
                Matrix4x4 mat = new Matrix4x4();
                mat.SetTRS(Vector3.zero, Quaternion.identity, scale);
                for (var i = 0; i < sharedMesh.vertices.Length; i++)
                {
                    meshVertices[i] = mat.MultiplyPoint(sharedMesh.vertices[i]);
                }
                WriteVertices(sw, meshVertices);
            }


            var meshIndices = sharedMesh.triangles;
            //flip
            if (scale.x * scale.y * scale.z < 0)
            {
                meshIndices = meshIndices.Reverse().ToArray();
            }

            for (int i = 0; i < meshIndices.Length; i += 3)
            {
                WriteTriangle(sw, meshIndices[i] + 1, meshIndices[i + 1] + 1, meshIndices[i + 2] + 1);
            }
        }

        public static void ExportScenePrimaryCollider(string file_path, MonoBehaviour prefab_root, BoxCollider collider)
        {
            if (prefab_root == null || collider == null)
                return;

            using (StreamWriter sw = new StreamWriter(file_path))
            {
                Transform tran = prefab_root.transform;
                Vector3 pos = tran.localPosition;
                var rot = tran.localRotation;
                tran.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                ObjFileWriter.WriteCollider(sw, prefab_root.name, collider);
                tran.SetLocalPositionAndRotation(pos, rot);
            }
        }

        public static void ExportSceneMeshCollider(string file_path, MonoBehaviour prefab_root, MeshCollider collider)
        {
            if (prefab_root == null || collider == null)
                return;

            using (StreamWriter sw = new StreamWriter(file_path))
            {
                Transform tran = prefab_root.transform;
                Vector3 pos = tran.localPosition;
                var rot = tran.localRotation;
                tran.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                ObjFileWriter.WriteMeshCollider(sw, prefab_root.name, collider);
                tran.SetLocalPositionAndRotation(pos, rot);
            }
        }

        public static void ExportNavMesh(string file_path, string name, UnityEngine.AI.NavMeshTriangulation navMesh)
        {
            using (StreamWriter sw = new StreamWriter(file_path))
            {
                WriteGroupName(sw, name);

                WriteVertices(sw, navMesh.vertices);

                var indices = navMesh.indices;
                for (int i = 0; i < indices.Length; i += 3)
                {
                    WriteTriangle(sw, indices[i] + 1, indices[i + 1] + 1, indices[i + 2] + 1);
                }
            }
        }
    }
}
