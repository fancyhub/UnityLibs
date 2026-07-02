/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/02/03
 * Title   : 
 * Desc    : 
*************************************************************************************/
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;

namespace FH.DI.Ed
{
    public class IOSPlistModifier : ScriptableObject
    {
        [SerializeField] public UnityEditor.DefaultAsset CustomInfoPlistForAdd; //Need plist asset
        [SerializeField] public UnityEditor.DefaultAsset CustomInfoPlistForDelete; //Need plist asset

#if UNITY_IOS
        [PostProcessBuild(1000)]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string pathToBuildProject)
        {
            if (buildTarget != BuildTarget.iOS)
                return;

            IOSPlistModifier modifier = ScriptableObject.CreateInstance<IOSPlistModifier>();


            Debug.Log("====== Plist Begin Modify =======");
            string plistPath = pathToBuildProject + "/Info.plist";
            UnityEditor.iOS.Xcode.PlistDocument plistDoc = new UnityEditor.iOS.Xcode.PlistDocument();
            plistDoc.ReadFromFile(plistPath);

            //get root
            UnityEditor.iOS.Xcode.PlistElementDict rootDict = plistDoc.root;

            bool dirty = false;

            //add
            if (modifier.CustomInfoPlistForAdd != null)
            {
                string plistAddPath = UnityEditor.AssetDatabase.GetAssetPath(modifier.CustomInfoPlistForAdd);
                if (plistAddPath.EndsWith(".plist"))
                {
                    UnityEditor.iOS.Xcode.PlistDocument plistAdd = new UnityEditor.iOS.Xcode.PlistDocument();
                    plistAdd.ReadFromFile(plistAddPath);

                    foreach (var p in plistAdd.root.values)
                    {
                        string key = p.Key;
                        UnityEditor.iOS.Xcode.PlistElement value = p.Value;
                        Debug.Log($"Plist Add {key}");
                        _MergeDictValue(rootDict, key, value, key);
                        dirty = true;
                    }
                }
            }


            //delete
            if (modifier.CustomInfoPlistForDelete != null)
            {
                string plistDeletePath = UnityEditor.AssetDatabase.GetAssetPath(modifier.CustomInfoPlistForDelete);
                if (plistDeletePath.EndsWith(".plist"))
                {
                    UnityEditor.iOS.Xcode.PlistDocument plistDelete = new UnityEditor.iOS.Xcode.PlistDocument();
                    plistDelete.ReadFromFile(plistDeletePath);

                    foreach (var p in plistDelete.root.values)
                    {
                        string key = p.Key;

                        if (_DeleteDictValue(rootDict, key, p.Value, key))
                            Debug.Log($"Plist Delete {key}");
                        dirty = true;
                    }
                }
            }

            if (dirty)
                plistDoc.WriteToFile(plistPath);
            Debug.Log("======= Plist End Modify =======");
        }

        private static void _MergeDictValue(
            UnityEditor.iOS.Xcode.PlistElementDict targetDict,
            string key,
            UnityEditor.iOS.Xcode.PlistElement sourceValue,
            string path)
        {
            if (!targetDict.values.ContainsKey(key))
            {
                targetDict[key] = _CloneElement(sourceValue);
                return;
            }

            UnityEditor.iOS.Xcode.PlistElement targetValue = targetDict[key];
            _ThrowIfTypeMismatch(targetValue, sourceValue, path);

            if (sourceValue is UnityEditor.iOS.Xcode.PlistElementDict sourceDict)
            {
                _MergeDict(targetValue.AsDict(), sourceDict, path);
            }
            else if (sourceValue is UnityEditor.iOS.Xcode.PlistElementArray sourceArray)
            {
                _MergeArray(targetValue.AsArray(), sourceArray);
            }
            else
            {
                targetDict[key] = _CloneElement(sourceValue);
            }
        }

        private static void _MergeDict(
            UnityEditor.iOS.Xcode.PlistElementDict targetDict,
            UnityEditor.iOS.Xcode.PlistElementDict sourceDict,
            string path)
        {
            foreach (var pair in sourceDict.values)
                _MergeDictValue(targetDict, pair.Key, pair.Value, $"{path}.{pair.Key}");
        }

        private static void _MergeArray(
            UnityEditor.iOS.Xcode.PlistElementArray targetArray,
            UnityEditor.iOS.Xcode.PlistElementArray sourceArray)
        {
            foreach (var sourceItem in sourceArray.values)
            {
                bool exists = false;
                foreach (var targetItem in targetArray.values)
                {
                    if (_ElementEquals(targetItem, sourceItem))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                    targetArray.values.Add(_CloneElement(sourceItem));
            }
        }

        private static bool _DeleteDictValue(
            UnityEditor.iOS.Xcode.PlistElementDict targetDict,
            string key,
            UnityEditor.iOS.Xcode.PlistElement deleteSpec,
            string path)
        {
            if (!targetDict.values.ContainsKey(key))
                return false;

            UnityEditor.iOS.Xcode.PlistElement targetValue = targetDict[key];

            if (deleteSpec is UnityEditor.iOS.Xcode.PlistElementBoolean)
                return targetDict.values.Remove(key);

            _ThrowIfTypeMismatch(targetValue, deleteSpec, path);

            if (deleteSpec is UnityEditor.iOS.Xcode.PlistElementDict sourceDict)
            {
                bool changed = _DeleteDict(targetValue.AsDict(), sourceDict, path);
                if (targetValue.AsDict().values.Count == 0)
                    changed |= targetDict.values.Remove(key);
                return changed;
            }

            if (deleteSpec is UnityEditor.iOS.Xcode.PlistElementArray sourceArray)
            {
                bool changed = _DeleteArray(targetValue.AsArray(), sourceArray);
                if (targetValue.AsArray().values.Count == 0)
                    changed |= targetDict.values.Remove(key);
                return changed;
            }

            throw new InvalidOperationException($"Info.plist delete spec at '{path}' must be bool, array, or dict.");
        }

        private static bool _DeleteDict(
            UnityEditor.iOS.Xcode.PlistElementDict targetDict,
            UnityEditor.iOS.Xcode.PlistElementDict sourceDict,
            string path)
        {
            bool changed = false;
            foreach (var pair in sourceDict.values)
                changed |= _DeleteDictValue(targetDict, pair.Key, pair.Value, $"{path}.{pair.Key}");
            return changed;
        }

        private static bool _DeleteArray(
            UnityEditor.iOS.Xcode.PlistElementArray targetArray,
            UnityEditor.iOS.Xcode.PlistElementArray sourceArray)
        {
            bool changed = false;
            foreach (var sourceItem in sourceArray.values)
            {
                for (int i = targetArray.values.Count - 1; i >= 0; i--)
                {
                    if (!_ElementEquals(targetArray.values[i], sourceItem))
                        continue;

                    targetArray.values.RemoveAt(i);
                    changed = true;
                }
            }
            return changed;
        }

        private static void _ThrowIfTypeMismatch(
            UnityEditor.iOS.Xcode.PlistElement targetValue,
            UnityEditor.iOS.Xcode.PlistElement sourceValue,
            string path)
        {
            if (targetValue.GetType() != sourceValue.GetType())
            {
                throw new InvalidOperationException(
                    $"Info.plist type mismatch at '{path}'. Target is {targetValue.GetType().Name}, custom plist is {sourceValue.GetType().Name}.");
            }
        }

        private static bool _ElementEquals(
            UnityEditor.iOS.Xcode.PlistElement left,
            UnityEditor.iOS.Xcode.PlistElement right)
        {
            if (left.GetType() != right.GetType())
                return false;

            if (left is UnityEditor.iOS.Xcode.PlistElementDict leftDict)
            {
                var rightDict = right.AsDict();
                if (leftDict.values.Count != rightDict.values.Count)
                    return false;

                foreach (var pair in leftDict.values)
                {
                    if (!rightDict.values.ContainsKey(pair.Key) || !_ElementEquals(pair.Value, rightDict[pair.Key]))
                        return false;
                }
                return true;
            }

            if (left is UnityEditor.iOS.Xcode.PlistElementArray leftArray)
            {
                var rightArray = right.AsArray();
                if (leftArray.values.Count != rightArray.values.Count)
                    return false;

                for (int i = 0; i < leftArray.values.Count; i++)
                {
                    if (!_ElementEquals(leftArray.values[i], rightArray.values[i]))
                        return false;
                }
                return true;
            }

            if (left is UnityEditor.iOS.Xcode.PlistElementString)
                return left.AsString() == right.AsString();
            if (left is UnityEditor.iOS.Xcode.PlistElementInteger)
                return left.AsInteger() == right.AsInteger();
            if (left is UnityEditor.iOS.Xcode.PlistElementBoolean)
                return left.AsBoolean() == right.AsBoolean();
            if (left is UnityEditor.iOS.Xcode.PlistElementReal)
                return Math.Abs(left.AsReal() - right.AsReal()) < double.Epsilon;

            throw new InvalidOperationException($"Unsupported plist element type: {left.GetType().Name}.");
        }

        private static UnityEditor.iOS.Xcode.PlistElement _CloneElement(UnityEditor.iOS.Xcode.PlistElement source)
        {
            if (source is UnityEditor.iOS.Xcode.PlistElementDict sourceDict)
            {
                var result = new UnityEditor.iOS.Xcode.PlistElementDict();
                foreach (var pair in sourceDict.values)
                    result.values[pair.Key] = _CloneElement(pair.Value);
                return result;
            }

            if (source is UnityEditor.iOS.Xcode.PlistElementArray sourceArray)
            {
                var result = new UnityEditor.iOS.Xcode.PlistElementArray();
                foreach (var item in sourceArray.values)
                    result.values.Add(_CloneElement(item));
                return result;
            }

            if (source is UnityEditor.iOS.Xcode.PlistElementString)
                return new UnityEditor.iOS.Xcode.PlistElementString(source.AsString());
            if (source is UnityEditor.iOS.Xcode.PlistElementInteger)
                return new UnityEditor.iOS.Xcode.PlistElementInteger(source.AsInteger());
            if (source is UnityEditor.iOS.Xcode.PlistElementBoolean)
                return new UnityEditor.iOS.Xcode.PlistElementBoolean(source.AsBoolean());
            if (source is UnityEditor.iOS.Xcode.PlistElementReal)
                return new UnityEditor.iOS.Xcode.PlistElementReal(source.AsReal());

            throw new InvalidOperationException($"Unsupported plist element type: {source.GetType().Name}.");
        }
#endif
    }
}
