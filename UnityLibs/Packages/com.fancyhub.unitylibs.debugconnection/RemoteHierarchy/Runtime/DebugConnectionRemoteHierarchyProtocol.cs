/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   : 
 * Desc    : 
*************************************************************************************/
using System;

namespace FH
{
    public static class DebugConnectionRemoteHierarchyProtocol
    {
        public static readonly Guid RequestChildren = new Guid("8d7dd8d9-68c0-4d46-a2a2-4dcfa511ca01");
        public static readonly Guid ChildrenResponse = new Guid("02c2e3b4-2edb-4b4a-8306-337d8fb89e68");
        public static readonly Guid RequestComponents = new Guid("3311ee52-9507-4e14-91be-d70f86d1a91d");
        public static readonly Guid ComponentsResponse = new Guid("dfd1f0dd-3418-489b-a6b1-8aa2ba137066");
        public static readonly Guid SetGameObjectActive = new Guid("f813a47b-6760-4057-8d94-54d4f570b01f");
        public static readonly Guid SetComponentEnabled = new Guid("e6520833-53b2-4d81-bc7d-4108c05083f3");
    }

    [Serializable]
    public sealed class DebugConnectionRemoteHierarchyRequest
    {
        public int InstanceId;
        public bool IncludePublicFields = true;
        public bool IncludeSerializableFields = true;
        public bool IncludePublicProperties;
    }

    [Serializable]
    public sealed class DebugConnectionRemoteHierarchySetActiveRequest
    {
        public int InstanceId;
        public bool Active;
    }

    [Serializable]
    public sealed class DebugConnectionRemoteHierarchySetComponentEnabledRequest
    {
        public int InstanceId;
        public int ComponentIndex;
        public bool Enabled;
        public bool IncludePublicFields = true;
        public bool IncludeSerializableFields = true;
        public bool IncludePublicProperties;
    }

    [Serializable]
    public sealed class DebugConnectionRemoteHierarchyChildrenResponse
    {
        public int ParentId;
        public DebugConnectionRemoteHierarchyNode[] Nodes = Array.Empty<DebugConnectionRemoteHierarchyNode>();
    }

    [Serializable]
    public sealed class DebugConnectionRemoteHierarchyNode
    {
        public int InstanceId;
        public string Name;
        public string SceneName;
        public string Path;
        public bool ActiveSelf;
        public bool ActiveInHierarchy;
        public bool IsScene;
        public int ChildCount;
        public int ComponentCount;
    }

    [Serializable]
    public sealed class DebugConnectionRemoteHierarchyComponentsResponse
    {
        public int InstanceId;
        public string Name;
        public DebugConnectionRemoteHierarchyComponent[] Components = Array.Empty<DebugConnectionRemoteHierarchyComponent>();
    }

    [Serializable]
    public sealed class DebugConnectionRemoteHierarchyComponent
    {
        public int ComponentIndex;
        public string TypeName;
        public bool HasEnabled;
        public bool Enabled;
        public DebugConnectionRemoteHierarchyProperty[] Properties = Array.Empty<DebugConnectionRemoteHierarchyProperty>();
    }

    [Serializable]
    public sealed class DebugConnectionRemoteHierarchyProperty
    {
        public string Name;
        public string Value;
    }
}
