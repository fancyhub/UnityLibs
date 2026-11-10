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
        private static void _BuildABSbpCustom(BuildTarget target, BuilderParam param, string outputDir, BundleNodeMap bundleMap)
        {            
            throw new NotImplementedException();
        }
    }
#else

    public static partial class Builder
    {
        private static void  _BuildABSbpCustom(BuildTarget target, BuilderParam param, string outputDir, BundleNodeMap bundleMap)
        {
            var sbp_tasks = UnityEditor.Build.Pipeline.DefaultBuildTasks.Create(UnityEditor.Build.Pipeline.DefaultBuildTasks.Preset.AssetBundleCompatible);

            sbp_tasks.Insert(
                FindAssetDependencyTaskIndex(sbp_tasks) + 1,
                new FilterAnonymousAssetObjectsSbpCustom(bundleMap.GetAnonymousAssetGuids()));


            var sbp_content = bundleMap.GenBundleBuildContent_SbpSimple();

            var sbp_param = param.SBPParam.ToSbpParam(target, outputDir);
            var exitCode = UnityEditor.Build.Pipeline.ContentPipeline.BuildAssetBundles(
                sbp_param, 
                sbp_content, 
                out var buildResults,
                sbp_tasks);

            if (exitCode != UnityEditor.Build.Pipeline.ReturnCode.Success)
                throw new UnityEditor.Build.BuildFailedException("SBP Build Error: " + exitCode);

            foreach (var p in bundleMap.GetAllNodes())
            {
                string path = System.IO.Path.Combine(outputDir, p.GetNodeName());
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
                // Re-querying dependencies for a partial model object can cause Unity
                // to return a larger serialization closure than the original SBP data.
                // We only need to remove references to other objects in this anonymous
                // asset; external asset references must remain unchanged.
                var refs = new List<ObjectIdentifier>(info.referencedObjects);

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
