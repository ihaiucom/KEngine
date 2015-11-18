﻿using System;
using UnityEngine;
using System.Collections;
using KEngine;

public class CUIAtlasDep : CAssetDep
{
    protected override void DoProcess(string resourcePath)
    {
        ProcessUIAtlas(resourcePath);
    }

    protected void ProcessUIAtlas(string path)
    {
        //Action<Material> matCallback = (Material _mat) => // 依赖材质
        //{
        //    OnFinishLoadDependencies(_mat);
        //};
        //new CStaticAssetLoader(path, OnLoadMaterialScript, path, matCallback);
        LoadMaterial(path, (mat) =>
        {
            OnFinishLoadDependencies(mat);
        });

    }

    public static KStaticAssetLoader LoadUIAtlas(string resourcePath, Action<UIAtlas> callback)
    {
        //System.Func<bool> doCheckCache = () =>
        //{
        //    UIAtlas cacheAtlas;  // 这里有个问题的，如果正在加载中，还没放进Cache列表...还是会多次执行, 但不要紧， CWWWLoader已经避免了重复加载, 这里确保快速回调，不延迟1帧
        //    if (CachedUIAtlas.TryGetValue(resourcePath, out cacheAtlas))
        //    {
        //        if (callback != null)
        //            callback(cacheAtlas);
        //        return true;
        //    }
        //    return false;
        //};

        //if (doCheckCache())
        //    return;
        bool exist = KStaticAssetLoader.GetRefCount<KStaticAssetLoader>(resourcePath) > 0;
        return KStaticAssetLoader.Load(resourcePath, (isOk, obj) =>
        {
            //if (doCheckCache())
            //    return;

            GameObject gameObj = obj as GameObject;  // Load UIAtlas Object Prefab
            gameObj.transform.parent = DependenciesContainer.transform;

            gameObj.name = resourcePath;
            UIAtlas atlas = gameObj.GetComponent<UIAtlas>();
            Logger.Assert(atlas);

            if (!exist)
            {
                // Wait Load Material
                var colDep = gameObj.GetComponent<CAssetDep>();
                Logger.Assert(colDep && colDep.GetType() == typeof(CUIAtlasDep));// CResourceDependencyType.UI_ATLAS);
                // 依赖材质Material, 加载后是Material
                colDep.AddFinishCallback((assetDep, _obj) =>
                {
                    // 塞Material进去UIAtlas
                    if (atlas.spriteMaterial == null) // 不为空意味已经加载过了！
                    {
                        Material _mat = _obj as Material;
                        atlas.spriteMaterial = _mat; // 注意，这一行性能消耗非常的大！
                    }
                    else
                        Logger.LogWarning("Atlas依赖的材质多次加载了（未缓存)!!!!!!!!!!!!!");

                    if (callback != null)
                        callback(atlas);
                });
            }
            else
            {
                if (callback != null)
                    callback(atlas);
            }

        });
    }

}
