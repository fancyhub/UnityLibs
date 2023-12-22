using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSAFileSystem : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            FH.SAFileSystem.EdSetObbPath(@"E:\fancyHub\UnityLibs\UnityLibs\Bundle\Player\Android\split.main.obb");

            Debug.LogError("<NativeIO> Start Test StreamingAssets");
            Debug.LogError($"<NativeIO> StreamingAssetsPath: {Application.streamingAssetsPath}");
            Debug.LogError($"<NativeIO> DataPath: {Application.dataPath}");
            byte[] bytes = FH.SAFileSystem.ReadAllBytes("Test.txt");
            if (bytes != null)
            {
                string str = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.LogError($"<NativeIO> From StreamingAssets SUCC: {str}");
            }
            else
                Debug.LogError($"<NativeIO> From StreamingAssets ERR: ");


            List<string> file_list = new List<string>();
            FH.SAFileSystem.GetFileList("A", true, file_list);
            Debug.LogError("<NativeIO> 文件数量: " + file_list.Count);

            foreach (var f in file_list)
            {
                Debug.LogError("<NativeIO> 文件: " + f);
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}
