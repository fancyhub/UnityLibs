/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FH
{
    public static class DebugConnectionRemoteHierarchyService
    {
        private const int MaxComponentProperties = 48;
        private const int SceneNodeIdBase = -1000000;
        private const int DontDestroyOnLoadSceneNodeId = -2000000;
        private const string DontDestroyOnLoadSceneName = "DontDestroyOnLoad";
        private const string DontDestroyProbeName = "__DDOL_Probe__";
        private static bool _Registered;
        private static Scene _DontDestroyOnLoadScene;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoRegister()
        {
            Register();
        }

        public static void Register()
        {
            if (_Registered)
                return;

            _Registered = true;
            DebugConnectionServer.Register(DebugConnectionRemoteHierarchyProtocol.RequestChildren, OnRequestChildren);
            DebugConnectionServer.Register(DebugConnectionRemoteHierarchyProtocol.RequestComponents, OnRequestComponents);
            DebugConnectionServer.Register(DebugConnectionRemoteHierarchyProtocol.SetGameObjectActive, OnSetGameObjectActive);
            DebugConnectionServer.Register(DebugConnectionRemoteHierarchyProtocol.SetComponentEnabled, OnSetComponentEnabled);
        }

        public static void Unregister()
        {
            if (!_Registered)
                return;

            _Registered = false;
            DebugConnectionServer.Unregister(DebugConnectionRemoteHierarchyProtocol.RequestChildren, OnRequestChildren);
            DebugConnectionServer.Unregister(DebugConnectionRemoteHierarchyProtocol.RequestComponents, OnRequestComponents);
            DebugConnectionServer.Unregister(DebugConnectionRemoteHierarchyProtocol.SetGameObjectActive, OnSetGameObjectActive);
            DebugConnectionServer.Unregister(DebugConnectionRemoteHierarchyProtocol.SetComponentEnabled, OnSetComponentEnabled);
        }

        private static void OnRequestChildren(DebugConnectionMessageEventArgs args)
        {
            DebugConnectionRemoteHierarchyRequest request = FromJson<DebugConnectionRemoteHierarchyRequest>(args.Data);
            int parentId = request == null ? 0 : request.InstanceId;

            DebugConnectionRemoteHierarchyChildrenResponse response = new DebugConnectionRemoteHierarchyChildrenResponse
            {
                ParentId = parentId,
                Nodes = GetChildren(parentId).ToArray(),
            };

            Send(args.ConnectionId, DebugConnectionRemoteHierarchyProtocol.ChildrenResponse, response);
        }

        private static void OnRequestComponents(DebugConnectionMessageEventArgs args)
        {
            DebugConnectionRemoteHierarchyRequest request = FromJson<DebugConnectionRemoteHierarchyRequest>(args.Data);
            int instanceId = request == null ? 0 : request.InstanceId;
            GameObject obj = IsSceneNodeId(instanceId) ? null : FindGameObject(instanceId);

            DebugConnectionRemoteHierarchyComponentsResponse response = new DebugConnectionRemoteHierarchyComponentsResponse
            {
                InstanceId = instanceId,
                Name = obj == null ? GetSceneNodeName(instanceId) : obj.name,
                Components = obj == null ? Array.Empty<DebugConnectionRemoteHierarchyComponent>() : GetComponents(obj, request).ToArray(),
            };

            Send(args.ConnectionId, DebugConnectionRemoteHierarchyProtocol.ComponentsResponse, response);
        }

        private static void OnSetGameObjectActive(DebugConnectionMessageEventArgs args)
        {
            DebugConnectionRemoteHierarchySetActiveRequest request = FromJson<DebugConnectionRemoteHierarchySetActiveRequest>(args.Data);
            if (request == null)
                return;

            GameObject obj = FindGameObject(request.InstanceId);
            if (obj == null)
                return;

            obj.SetActive(request.Active);

            int parentId = obj.transform.parent == null ? GetSceneNodeId(obj.scene) : obj.transform.parent.gameObject.GetInstanceID();
            DebugConnectionRemoteHierarchyChildrenResponse response = new DebugConnectionRemoteHierarchyChildrenResponse
            {
                ParentId = parentId,
                Nodes = GetChildren(parentId).ToArray(),
            };
            Send(args.ConnectionId, DebugConnectionRemoteHierarchyProtocol.ChildrenResponse, response);
        }

        private static void OnSetComponentEnabled(DebugConnectionMessageEventArgs args)
        {
            DebugConnectionRemoteHierarchySetComponentEnabledRequest request =
                FromJson<DebugConnectionRemoteHierarchySetComponentEnabledRequest>(args.Data);
            if (request == null)
                return;

            GameObject obj = FindGameObject(request.InstanceId);
            if (obj == null)
                return;

            Component[] components = obj.GetComponents<Component>();
            if (request.ComponentIndex < 0 || request.ComponentIndex >= components.Length)
                return;

            SetComponentEnabled(components[request.ComponentIndex], request.Enabled);

            DebugConnectionRemoteHierarchyRequest componentRequest = new DebugConnectionRemoteHierarchyRequest
            {
                InstanceId = request.InstanceId,
                IncludePublicFields = request.IncludePublicFields,
                IncludeSerializableFields = request.IncludeSerializableFields,
                IncludePublicProperties = request.IncludePublicProperties,
            };

            DebugConnectionRemoteHierarchyComponentsResponse response = new DebugConnectionRemoteHierarchyComponentsResponse
            {
                InstanceId = request.InstanceId,
                Name = obj.name,
                Components = GetComponents(obj, componentRequest).ToArray(),
            };
            Send(args.ConnectionId, DebugConnectionRemoteHierarchyProtocol.ComponentsResponse, response);
        }

        private static List<DebugConnectionRemoteHierarchyNode> GetChildren(int parentId)
        {
            List<DebugConnectionRemoteHierarchyNode> nodes = new List<DebugConnectionRemoteHierarchyNode>();
            if (parentId == 0)
            {
                for (int i = 0; i < SceneManager.sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (!scene.IsValid() || !scene.isLoaded)
                        continue;

                    nodes.Add(CreateSceneNode(scene, SceneNodeIdBase - i));
                }

                Scene dontDestroyScene = GetDontDestroyOnLoadScene();
                if (dontDestroyScene.IsValid() && dontDestroyScene.isLoaded && CountVisibleSceneRoots(dontDestroyScene) > 0)
                    nodes.Add(CreateSceneNode(dontDestroyScene, DontDestroyOnLoadSceneNodeId));
                return nodes;
            }

            if (IsSceneNodeId(parentId))
            {
                AddSceneRoots(FindSceneByNodeId(parentId), nodes);
                return nodes;
            }

            GameObject parent = FindGameObject(parentId);
            if (parent == null)
                return nodes;

            Transform transform = parent.transform;
            for (int i = 0; i < transform.childCount; i++)
                nodes.Add(CreateNode(transform.GetChild(i).gameObject));

            return nodes;
        }

        private static void AddSceneRoots(Scene scene, List<DebugConnectionRemoteHierarchyNode> nodes)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null || root.name == DontDestroyProbeName)
                    continue;

                nodes.Add(CreateNode(root));
            }
        }

        private static DebugConnectionRemoteHierarchyNode CreateSceneNode(Scene scene, int instanceId)
        {
            string sceneName = GetSceneName(scene);
            return new DebugConnectionRemoteHierarchyNode
            {
                InstanceId = instanceId,
                Name = sceneName,
                SceneName = sceneName,
                Path = sceneName,
                ActiveSelf = true,
                ActiveInHierarchy = true,
                IsScene = true,
                ChildCount = CountVisibleSceneRoots(scene),
                ComponentCount = 0,
            };
        }

        private static DebugConnectionRemoteHierarchyNode CreateNode(GameObject obj)
        {
            return new DebugConnectionRemoteHierarchyNode
            {
                InstanceId = obj.GetInstanceID(),
                Name = obj.name,
                SceneName = GetSceneName(obj.scene),
                Path = GetPath(obj.transform),
                ActiveSelf = obj.activeSelf,
                ActiveInHierarchy = obj.activeInHierarchy,
                IsScene = false,
                ChildCount = obj.transform.childCount,
                ComponentCount = obj.GetComponents<Component>().Length,
            };
        }

        private static string GetSceneName(Scene scene)
        {
            if (!scene.IsValid())
                return string.Empty;

            return string.IsNullOrEmpty(scene.name) ? DontDestroyOnLoadSceneName : scene.name;
        }

        private static bool IsSceneNodeId(int instanceId)
        {
            return instanceId <= SceneNodeIdBase;
        }

        private static string GetSceneNodeName(int instanceId)
        {
            if (!IsSceneNodeId(instanceId))
                return string.Empty;

            return GetSceneName(FindSceneByNodeId(instanceId));
        }

        private static Scene FindSceneByNodeId(int nodeId)
        {
            if (nodeId == DontDestroyOnLoadSceneNodeId)
                return GetDontDestroyOnLoadScene();

            int sceneIndex = SceneNodeIdBase - nodeId;
            if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCount)
                return default;

            return SceneManager.GetSceneAt(sceneIndex);
        }

        private static Scene GetDontDestroyOnLoadScene()
        {
            if (_DontDestroyOnLoadScene.IsValid())
                return _DontDestroyOnLoadScene;

            GameObject probe = new GameObject(DontDestroyProbeName);
            UnityEngine.Object.DontDestroyOnLoad(probe);
            _DontDestroyOnLoadScene = probe.scene;
            UnityEngine.Object.Destroy(probe);
            return _DontDestroyOnLoadScene;
        }

        private static int CountVisibleSceneRoots(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
                return 0;

            int count = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] != null && roots[i].name != DontDestroyProbeName)
                    count++;
            }

            return count;
        }

        private static List<DebugConnectionRemoteHierarchyComponent> GetComponents(
            GameObject obj,
            DebugConnectionRemoteHierarchyRequest request)
        {
            List<DebugConnectionRemoteHierarchyComponent> ret = new List<DebugConnectionRemoteHierarchyComponent>();
            Component[] components = obj.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
                ret.Add(CreateComponent(components[i], i, request));

            return ret;
        }

        private static DebugConnectionRemoteHierarchyComponent CreateComponent(
            Component component,
            int componentIndex,
            DebugConnectionRemoteHierarchyRequest request)
        {
            if (component == null)
            {
                return new DebugConnectionRemoteHierarchyComponent
                {
                    TypeName = "Missing Component",
                    Properties = Array.Empty<DebugConnectionRemoteHierarchyProperty>(),
                };
            }

            List<DebugConnectionRemoteHierarchyProperty> properties = new List<DebugConnectionRemoteHierarchyProperty>();
            AddCommonProperties(component, properties);
            AddReflectedProperties(component, request, properties);

            bool hasEnabled = TryGetEnabled(component, out bool enabled);
            return new DebugConnectionRemoteHierarchyComponent
            {
                ComponentIndex = componentIndex,
                TypeName = component.GetType().FullName,
                HasEnabled = hasEnabled,
                Enabled = enabled,
                Properties = properties.ToArray(),
            };
        }

        private static void AddCommonProperties(Component component, List<DebugConnectionRemoteHierarchyProperty> properties)
        {
            Transform transform = component as Transform;
            if (transform == null)
                return;

            AddProperty(properties, "position", FormatValue(transform.position));
            AddProperty(properties, "rotation", FormatValue(transform.rotation.eulerAngles));
            AddProperty(properties, "localPosition", FormatValue(transform.localPosition));
            AddProperty(properties, "localRotation", FormatValue(transform.localRotation.eulerAngles));
            AddProperty(properties, "localScale", FormatValue(transform.localScale));
        }

        private static void AddReflectedProperties(
            Component component,
            DebugConnectionRemoteHierarchyRequest request,
            List<DebugConnectionRemoteHierarchyProperty> properties)
        {
            if (request == null)
                request = new DebugConnectionRemoteHierarchyRequest();

            Type type = component.GetType();

            if (request.IncludePublicFields)
                AddFields(component, type.GetFields(BindingFlags.Instance | BindingFlags.Public), false, properties);

            if (request.IncludeSerializableFields)
                AddFields(component, type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic), true, properties);

            if (!request.IncludePublicProperties)
                return;

            PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < props.Length && properties.Count < MaxComponentProperties; i++)
            {
                PropertyInfo prop = props[i];
                if (!prop.CanRead || prop.GetIndexParameters().Length > 0)
                    continue;

                if (!TryFormatValue(prop.PropertyType, delegate { return prop.GetValue(component, null); }, out string value))
                    continue;

                AddProperty(properties, prop.Name, value);
            }
        }

        private static void AddFields(
            Component component,
            FieldInfo[] fields,
            bool onlySerializeField,
            List<DebugConnectionRemoteHierarchyProperty> properties)
        {
            for (int i = 0; i < fields.Length && properties.Count < MaxComponentProperties; i++)
            {
                FieldInfo field = fields[i];
                if (field.IsStatic || field.IsNotSerialized)
                    continue;

                if (onlySerializeField && !Attribute.IsDefined(field, typeof(SerializeField)))
                    continue;

                if (!TryFormatValue(field.FieldType, delegate { return field.GetValue(component); }, out string value))
                    continue;

                AddProperty(properties, field.Name, value);
            }
        }

        private static void AddProperty(List<DebugConnectionRemoteHierarchyProperty> properties, string name, string value)
        {
            properties.Add(new DebugConnectionRemoteHierarchyProperty
            {
                Name = name ?? string.Empty,
                Value = value ?? string.Empty,
            });
        }

        private static bool TryGetEnabled(Component component, out bool enabled)
        {
            Behaviour behaviour = component as Behaviour;
            if (behaviour != null)
            {
                enabled = behaviour.enabled;
                return true;
            }

            Renderer renderer = component as Renderer;
            if (renderer != null)
            {
                enabled = renderer.enabled;
                return true;
            }

            Collider collider = component as Collider;
            if (collider != null)
            {
                enabled = collider.enabled;
                return true;
            }

            enabled = false;
            return false;
        }

        private static void SetComponentEnabled(Component component, bool enabled)
        {
            Behaviour behaviour = component as Behaviour;
            if (behaviour != null)
            {
                behaviour.enabled = enabled;
                return;
            }

            Renderer renderer = component as Renderer;
            if (renderer != null)
            {
                renderer.enabled = enabled;
                return;
            }

            Collider collider = component as Collider;
            if (collider != null)
                collider.enabled = enabled;
        }

        private static int GetSceneNodeId(Scene scene)
        {
            if (!scene.IsValid())
                return 0;

            if (string.IsNullOrEmpty(scene.name))
                return DontDestroyOnLoadSceneNodeId;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i) == scene)
                    return SceneNodeIdBase - i;
            }

            return 0;
        }

        private static bool TryFormatValue(Type type, Func<object> getter, out string value)
        {
            value = string.Empty;
            if (!IsSupportedValueType(type))
                return false;

            try
            {
                value = FormatValue(getter());
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsSupportedValueType(Type type)
        {
            if (type == typeof(string) || type == typeof(bool) || type.IsEnum)
                return true;

            if (type.IsPrimitive || type == typeof(decimal))
                return true;

            return type == typeof(Vector2)
                || type == typeof(Vector3)
                || type == typeof(Vector4)
                || type == typeof(Quaternion)
                || type == typeof(Color)
                || typeof(UnityEngine.Object).IsAssignableFrom(type);
        }

        private static string FormatValue(object value)
        {
            if (value == null)
                return "null";

            Vector2 v2;
            Vector3 v3;
            Vector4 v4;
            Quaternion q;
            Color color;
            UnityEngine.Object obj;

            if (value is float)
                return ((float)value).ToString("0.###");
            if (value is double)
                return ((double)value).ToString("0.###");
            if (value is Vector2)
            {
                v2 = (Vector2)value;
                return string.Format("({0:0.###}, {1:0.###})", v2.x, v2.y);
            }
            if (value is Vector3)
            {
                v3 = (Vector3)value;
                return string.Format("({0:0.###}, {1:0.###}, {2:0.###})", v3.x, v3.y, v3.z);
            }
            if (value is Vector4)
            {
                v4 = (Vector4)value;
                return string.Format("({0:0.###}, {1:0.###}, {2:0.###}, {3:0.###})", v4.x, v4.y, v4.z, v4.w);
            }
            if (value is Quaternion)
            {
                q = (Quaternion)value;
                return string.Format("({0:0.###}, {1:0.###}, {2:0.###}, {3:0.###})", q.x, q.y, q.z, q.w);
            }
            if (value is Color)
            {
                color = (Color)value;
                return string.Format("RGBA({0:0.###}, {1:0.###}, {2:0.###}, {3:0.###})", color.r, color.g, color.b, color.a);
            }
            if (value is UnityEngine.Object)
            {
                obj = (UnityEngine.Object)value;
                return obj == null ? "null" : obj.name + " (" + obj.GetType().Name + ")";
            }

            return value.ToString();
        }

        private static string GetPath(Transform transform)
        {
            if (transform == null)
                return string.Empty;

            List<string> names = new List<string>();
            Transform current = transform;
            while (current != null)
            {
                names.Add(current.name);
                current = current.parent;
            }

            names.Reverse();
            return string.Join("/", names.ToArray());
        }

        private static GameObject FindGameObject(int instanceId)
        {
            if (instanceId == 0)
                return null;

            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform transform = transforms[i];
                if (transform == null || transform.gameObject == null)
                    continue;

                GameObject obj = transform.gameObject;
                if (obj.GetInstanceID() != instanceId)
                    continue;

                if (!obj.scene.IsValid())
                    continue;

                return obj;
            }

            return null;
        }

        private static T FromJson<T>(byte[] data) where T : class
        {
            if (data == null || data.Length == 0)
                return null;

            try
            {
                return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(data));
            }
            catch
            {
                return null;
            }
        }

        private static void Send(int connectionId, Guid messageId, object payload)
        {
            string json = JsonUtility.ToJson(payload);
            byte[] data = Encoding.UTF8.GetBytes(json);
            if (connectionId > 0)
                DebugConnectionServer.SendTo(connectionId, messageId, data);
            else
                DebugConnectionServer.Send(messageId, data);
        }
    }
}
