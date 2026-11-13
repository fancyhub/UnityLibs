/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2021/5/17
 * Title   : 
 * Desc    : 
*************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Content;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Injector;
using UnityEditor.Build.Pipeline.Interfaces;

namespace FH.AssetBundleBuilder.Ed
{
#if !ENABLE_SBP
    public static partial class Builder
    {
        private static void _BuildABSbpCustom(BuildContext context, BundleNodeMap bundleMap)
        {            
            throw new NotImplementedException();
        }
    }
#else

    public static partial class Builder
    {
        private static void  _BuildABSbpCustom(ABBuildContext context, BundleNodeMap bundleMap)
        {
            var sbp_tasks = UnityEditor.Build.Pipeline.DefaultBuildTasks.Create(UnityEditor.Build.Pipeline.DefaultBuildTasks.Preset.AssetBundleCompatible);

            sbp_tasks.Insert(
                FindAssetDependencyTaskIndex(sbp_tasks) + 1,
                new FilterAnonymousAssetObjectsSbpCustom(bundleMap.GetAnonymousAssetGuids()));


            var sbp_content = bundleMap.GenBundleBuildContent_SbpSimple();

            var sbp_param = context.BuilderParam.SBPParam.ToSbpParam(context.Target, context.OutputDir);
            var exitCode = UnityEditor.Build.Pipeline.ContentPipeline.BuildAssetBundles(
                sbp_param, 
                sbp_content, 
                out var buildResults,
                sbp_tasks);

            if (exitCode != UnityEditor.Build.Pipeline.ReturnCode.Success)
                throw new UnityEditor.Build.BuildFailedException("SBP Build Error: " + exitCode);

            foreach (var p in bundleMap.GetAllNodes())
            {
                string path = System.IO.Path.Combine(context.OutputDir, p.GetNodeName());
                if (System.IO.File.Exists(path))
                    p.FileHash = FileUtil.CalcFileHash(path);
            }

        }

        private static int FindAssetDependencyTaskIndex(IList<UnityEditor.Build.Pipeline.Interfaces.IBuildTask> tasks)
        {
            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i] is UnityEditor.Build.Pipeline.Tasks.CalculateAssetDependencyData)
                    return i;
            }
            throw new Exception("SBP 默认任务链中找不到 CalculateAssetDependencyData");
        }
    }

    internal sealed class FilterAnonymousAssetObjectsSbpCustom : IBuildTask
    {
        public int Version => 1;
        private readonly HashSet<GUID> _anonymousAssetGuids;

        public FilterAnonymousAssetObjectsSbpCustom(HashSet<GUID> anonymousAssetGuids)
        {
            _anonymousAssetGuids = anonymousAssetGuids;
        }

#pragma warning disable 649
        [InjectContext(ContextUsage.In)]
        IBundleBuildParameters m_Parameters;

        [InjectContext]
        IDependencyData m_DependencyData;

        [InjectContext(ContextUsage.InOut, true)]
        IBuildResults m_Results;

        [InjectContext(ContextUsage.In, true)]
        IBuildExtendedAssetData m_ExtendedAssetData;

#pragma warning restore 649

        public ReturnCode Run()
        {
            if (_anonymousAssetGuids == null)
                return ReturnCode.SuccessNotRun;

            HashSet<GUID> anonymousGuids = _anonymousAssetGuids;

            var requiredObjects = new HashSet<ObjectIdentifier>();
            foreach (var pair in m_DependencyData.AssetInfo)
            {
                if (anonymousGuids.Contains(pair.Key))
                    continue;

                foreach (ObjectIdentifier obj in pair.Value.referencedObjects)
                {
                    if (anonymousGuids.Contains(obj.guid))
                        requiredObjects.Add(obj);
                }

                // Some SBP versions do not expose a model sub-object reference from
                // the prefab through AssetLoadInfo.referencedObjects. Resolve the
                // object-level dependencies of the explicit asset as a fallback.
                ObjectIdentifier[] explicitObjects = pair.Value.includedObjects.ToArray();
                ObjectIdentifier[] objectDependencies =
                    ContentBuildInterface.GetPlayerDependenciesForObjects(
                        explicitObjects,
                        m_Parameters.Target,
                        m_Parameters.ScriptInfo);
                if (objectDependencies != null)
                {
                    foreach (ObjectIdentifier obj in objectDependencies)
                    {
                        if (anonymousGuids.Contains(obj.guid) && IsAllowedAnonymousObject(obj))
                            requiredObjects.Add(obj);
                    }
                }
            }

            // Scene dependencies are stored separately from AssetInfo. This is the
            // important path for a scene such as 3DDemoScene: only the referenced
            // Mesh/AnimationClip ObjectIdentifiers should survive in an anonymous FBX.
            foreach (var pair in m_DependencyData.SceneInfo)
            {
                foreach (ObjectIdentifier obj in pair.Value.referencedObjects)
                {
                    if (anonymousGuids.Contains(obj.guid))
                        requiredObjects.Add(obj);
                }
            }

            foreach (GUID anonymousGuid in anonymousGuids)
            {
                if (!m_DependencyData.AssetInfo.TryGetValue(anonymousGuid, out AssetLoadInfo info))
                    continue;

                var keep = new List<ObjectIdentifier>();
                foreach (ObjectIdentifier obj in info.includedObjects)
                {
                    if (requiredObjects.Contains(obj))
                        keep.Add(obj);
                }

                if (keep.Count == 0)
                    continue;

                // Keep the dependency list calculated by CalculateAssetDependencyData.
                // A model sub-object can still have an internal serialization dependency
                // on a minimal source hierarchy (for example the FBX GameObject and its
                // Transform). Those same-file objects must be serialized as included
                // objects, otherwise Unity creates a missing Transform at load time.
                var refs = new List<ObjectIdentifier>(info.referencedObjects);
                var internalDependencies = ContentBuildInterface.GetPlayerDependenciesForObjects(
                    keep.ToArray(),
                    m_Parameters.Target,
                    m_Parameters.ScriptInfo);

                if (internalDependencies != null)
                {
                    foreach (ObjectIdentifier dependency in internalDependencies)
                    {
                        if (dependency.guid != anonymousGuid || keep.Contains(dependency))
                            continue;

                        // The dependency closure of a model Mesh may also contain
                        // Avatar/AnimationClip and other model sub-assets. They are
                        // not needed for loading this Mesh. Only retain the hierarchy
                        // objects Unity needs to resolve the Mesh's source linkage.
                        Type[] types = ContentBuildInterface.GetTypeForObjects(
                            new[] { dependency });
                        bool isHierarchyObject = false;
                        if (types != null)
                        {
                            foreach (Type type in types)
                            {
                                if (type == typeof(GameObject) || type == typeof(Transform))
                                {
                                    isHierarchyObject = true;
                                    break;
                                }
                            }
                        }

                        if (isHierarchyObject)
                            keep.Add(dependency);
                    }
                }

                // Update the existing lists in place. CalculateAssetDependencyData
                // and BuildResults share these list instances in several SBP paths.
                info.includedObjects.Clear();
                info.includedObjects.AddRange(keep);
                info.referencedObjects.Clear();
                if (refs != null)
                {
                    foreach (ObjectIdentifier reference in refs)
                    {
                        // References to other objects in the same model file
                        // (GameObject/Transform/Avatar, etc.) are not needed by the
                        // selected Mesh and must not be added back.
                        if (reference.guid == anonymousGuid)
                            continue;

                        if (!info.referencedObjects.Contains(reference))
                            info.referencedObjects.Add(reference);
                    }
                }

                RebuildUsage(anonymousGuid, info);
                RemoveRepresentations(anonymousGuid);
                SyncBuildResult(anonymousGuid, info);
            }

            return ReturnCode.Success;
        }

        private static bool IsAllowedAnonymousObject(ObjectIdentifier objectId)
        {
            Type[] types = ContentBuildInterface.GetTypeForObjects(new[] { objectId });
            if (types == null)
                return false;

            foreach (Type type in types)
            {
                if (type == typeof(Mesh) ||
                    type == typeof(AnimationClip) ||
                    type == typeof(GameObject) ||
                    type == typeof(Transform))
                    return true;
            }
            return false;
        }

        private void RebuildUsage(GUID asset, AssetLoadInfo info)
        {
            var allObjects = new List<ObjectIdentifier>(info.includedObjects);
            allObjects.AddRange(info.referencedObjects);
            var usage = new BuildUsageTagSet();
            ContentBuildInterface.CalculateBuildUsageTags(
                allObjects.ToArray(),
                info.includedObjects.ToArray(),
                m_DependencyData.GlobalUsage,
                usage,
                m_DependencyData.DependencyUsageCache);
            m_DependencyData.AssetUsage[asset] = usage;
        }

        private void RemoveRepresentations(GUID asset)
        {
            if (m_ExtendedAssetData != null)
                m_ExtendedAssetData.ExtendedData.Remove(asset);
        }

        private void SyncBuildResult(GUID asset, AssetLoadInfo info)
        {
            if (m_Results == null || !m_Results.AssetResults.TryGetValue(asset, out AssetResultData result))
                return;

            result.IncludedObjects = info.includedObjects;
            result.ReferencedObjects = info.referencedObjects;
            m_Results.AssetResults[asset] = result;
        }
    }

    public partial class BundleNodeMap
    {
        public HashSet<UnityEditor.GUID> GetAnonymousAssetGuids()
        {
            HashSet<UnityEditor.GUID> ret = new HashSet<UnityEditor.GUID>();
            foreach (BundleNode node in _nodes_map)
            {
                foreach (AssetObj obj in node)
                {
                    if (!obj.NeedExport)
                        ret.Add(new UnityEditor.GUID(obj.GUID));
                }
            }
            return ret;
        }
    }
#endif
}
