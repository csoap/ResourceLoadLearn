using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    //资源关系依赖配表，可以根据crc来找到对应的资源
    protected Dictionary<uint, ResourceItem> m_ResourceItemDic = new Dictionary<uint, ResourceItem>();
    //储存已加载的ab包，key为crc
    protected Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();
    //AssetbundleItem类对象池
    protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(500);


    /// <summary>
    /// 加载ab配置表
    /// </summary>
    /// <returns></returns>
    public bool LoadAssetBundleConfig()
    {
        m_ResourceItemDic.Clear();
        AssetBundle configAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assetbundleconfig");
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");
        if (textAsset == null) return false;

        MemoryStream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter bf = new BinaryFormatter();
        AssetBundleConfig confg = (AssetBundleConfig)bf.Deserialize(stream);
        stream.Close();

        for (int i = 0; i < confg.ABList.Count; i++)
        {
            ABBase abBase = confg.ABList[i];
            ResourceItem item = new ResourceItem();
            item.m_Crc = abBase.Crc;
            item.m_AssetName = abBase.AssetName;
            item.m_ABName = abBase.ABName;
            item.m_Dependices = abBase.ABDependce;
            if (m_ResourceItemDic.ContainsKey(item.m_Crc))
            {
                Debug.LogError("重复的crc 资源名：" + item.m_AssetName + "ab包名： " + item.m_ABName);
            }
            else
            {
                m_ResourceItemDic.Add(item.m_Crc, item);
            }
        }

        return true;
    }

    /// <summary>
    /// 根据路径的crc加载中间类ResourceItem
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem LoadResourceAssetsBundle(uint crc)
    {
        ResourceItem item = null;
        if (!m_ResourceItemDic.TryGetValue(crc, out item) || item == null)
        {
            Debug.LogError(string.Format("LoadResourceAssetBundle error：can not find crc {0} in AssetBundleConfig", crc.ToString()));
            return item;
        }
        if (item.m_AssetBundle != null)
        {
            return item;
        }
        item.m_AssetBundle = LoadAssetBundle(item.m_ABName);
        if (item.m_Dependices != null)
        {
            for (int i = 0; i < item.m_Dependices.Count; i++)
            {
                LoadAssetBundle(item.m_Dependices[i]);
            }
        }
        return item;
    }

    /// <summary>
    /// 加载单个assetbundle
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public AssetBundle LoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.GetCRC32(name);
        if (!m_AssetBundleItemDic.TryGetValue(crc, out item))
        {
            AssetBundle assetBundle = null;
            string fullPath = Application.streamingAssetsPath + "/" + name;
            if (File.Exists(fullPath))
            {
                assetBundle = AssetBundle.LoadFromFile(fullPath);
            }
            if (assetBundle == null)
            {
                Debug.LogError("Load AssetBundle Error: " + fullPath);
            }
            item = m_AssetBundleItemPool.Spwan(true);
            item.assetBundle = assetBundle;
            item.RefCount++;
            m_AssetBundleItemDic.Add(crc, item);
        }
        else
        {
            item.RefCount++;
        }
        return item.assetBundle;
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="item"></param>
    public void ReleaseAsset(ResourceItem item)
    {
        if (item == null) return;

        // 先卸载依赖项， 再卸载自己
        if (item.m_Dependices != null && item.m_Dependices.Count > 0)
        {
            for (int i = 0; i < item.m_Dependices.Count; i++)
            {
                UnloadAssetBundle(item.m_Dependices[i]);
            }
        }
        UnloadAssetBundle(item.m_ABName);
    }

    private void UnloadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = CRC32.GetCRC32(name);
        if (!m_AssetBundleItemDic.TryGetValue(crc, out item)  &&  item != null)
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.assetBundle != null)
            {
                item.assetBundle.Unload(true);
                item.Reset();
                m_AssetBundleItemPool.Recycle(item);
                m_AssetBundleItemDic.Remove(crc);
            }
        }
    }

    /// <summary>
    /// 根据crc获得ResourceItem
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem FindResourceItem(uint crc)
    {
        return m_ResourceItemDic[crc];
    }
}

public class AssetBundleItem
{
    public AssetBundle assetBundle = null;
    public int RefCount; //引用结束

    public void Reset()
    {
        assetBundle = null;
        RefCount = 0;
    }
}

public class ResourceItem
{
    // 资源路径的CRC
    public uint m_Crc = 0;
    // 资源的文件名
    public string m_AssetName = string.Empty;
    //  该资源所在的AssetBundle名字
    public string m_ABName = string.Empty;
    // 该资源依赖的AssetBundles
    public List<string> m_Dependices = null;
    // 该资源加载完的AB包
    public AssetBundle m_AssetBundle = null;
}
