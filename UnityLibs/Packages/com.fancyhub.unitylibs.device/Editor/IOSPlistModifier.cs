/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/02/03
 * Title   : 
 * Desc    : 
*************************************************************************************/
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace FH.DI.Ed
{
    public class IOSPlistModifier : ScriptableObject
    {
        [SerializeField] public UnityEditor.DefaultAsset CustomInfoPlistForAdd; //Need plist asset
        [SerializeField] public UnityEditor.DefaultAsset CustInfoPlistForDelete; //Need plist asset

#if UNITY_IOS
        [PostProcessBuild(1)]
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
                        Debug.Log($"Plist Set {key}");
                        rootDict[key] = value;
                        dirty = true;
                    }
                }
            }


            //delete
            if (modifier.CustInfoPlistForDelete != null)
            {
                string plistDeletePath = UnityEditor.AssetDatabase.GetAssetPath(modifier.CustInfoPlistForDelete);
                if (plistDeletePath.EndsWith(".plist"))
                {
                    var targetDict = rootDict.values;
                    UnityEditor.iOS.Xcode.PlistDocument plistDelete = new UnityEditor.iOS.Xcode.PlistDocument();
                    plistDelete.ReadFromFile(plistDeletePath);

                    foreach (var p in plistDelete.root.values)
                    {
                        string key = p.Key;
                        
                        if(rootDict.values.Remove(key))
                        {
                            Debug.Log($"Plist Delete {key}");
                        }
                        dirty = true;
                    }
                }
            }

            if (dirty)
                plistDoc.WriteToFile(plistPath);
            Debug.Log("======= Plist End Modify =======");
        }
#endif
    }
}
