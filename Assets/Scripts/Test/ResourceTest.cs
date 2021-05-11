using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ResourceTest : MonoBehaviour {

     void Start()
    {
        TestLoadAB();
    }
    void TestLoadAB()
    {
        //TextAsset textAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/GameData/Data/ABData/AssetbundleConfig.bytes");
        AssetBundle configAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assetbundleconfig");
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");
        MemoryStream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleConfig testSerilize = (AssetBundleConfig)bf.Deserialize(stream);
        stream.Close();
        string path = "Assets/GameData/Prefabs/Attack.prefab";
        uint crc = CRC32.GetCRC32(path);
        ABBase abBase = null;
        for (int i = 0; i < testSerilize.ABList.Count; i++)
        {
            if (testSerilize.ABList[i].Crc == crc)
            {
                abBase = testSerilize.ABList[i];
                break;
            }
        }
        // 加载依赖ab包
        for (int i = 0; i < abBase.ABDependce.Count; i++)
        {
            AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABDependce[i]);
        }

        AssetBundle assetBundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABName);
        GameObject obj = GameObject.Instantiate(assetBundle.LoadAsset<GameObject>(abBase.AssetName));
    }
}
