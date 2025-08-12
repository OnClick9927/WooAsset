# 更新_初始化 _加载_卸载
## 更新

#### 注意该例子演示的没有服务器的情况下，所以需要自己请求资源版本
#### 如果有服务器 可以跳过请求请求远端版本信息，直接从 Assets.DownloadVersionData(version) 开始
#### 如果是 WebGL 或者 不希望本地有缓存，直接从初始化开始

``` csharp
Assets.SetAssetsSetting(new LocalSetting());
//拉取远端版本信息
var op = await Assets.LoadRemoteVersions();
//网络错误/AssetDatabaseMode/RudeMode 会=null
if (op.Versions != null)
{
  //选择版本
    var version = op.Versions.NewestVersion();
  //下载远端版本文件
    var down = await Assets.DownloadVersionData(version);
    //得到版本数据
    var versionData = down.GetVersion();
    //本地和远端比较，第二个参数表示，比较哪些 pkg
    var compare = await Assets.CompareVersion(versionData, versionData.GetAllPkgs());
//下载所有需要更新的资源
    for (int i = 0; i < compare.add.Count; i++)
        await Assets.DownLoadBundle(versionData.version, compare.add[i].bundleName);
    for (int i = 0; i < compare.change.Count; i++)
        await Assets.DownLoadBundle(versionData.version, compare.change[i].bundleName);
}
```
## 初始化
``` csharp
Assets.SetAssetsSetting(new LocalSetting());
//初始化
//可选参数 version 初始化哪一个版本，不传就是本地版本，本地没有就是远端最新版本
//可选参数 ignoreLocalVersion 如果本地有版本文件是否忽略，直接去拿目标版本
//可选参数 again 再一次初始化（使用场景，热更新界面也热更）
//可选参数 getPkgs 始化包选择
await Assets.InitAsync();

```
## 加载与卸载
``` csharp
///正常加载
var asset = await Assets.LoadAssetAsync(path);
///获取要加载的资源
var sp = asset.GetAsset<Sprite>();
///加载Unity无法识别的资源
var asset = await Assets.LoadRawAssetAsync(path);
RawObject raw = asset.GetAsset();
Debug.Log(raw.bytes.Length);

///加载子资源
var mainAsset = await Assets.LoadSubAsset(path);
var sp = mainAsset.GetSubAsset<Sprite>("a_1");
///上面几个的卸载资源
Assets.Release(asset)




///加载卸载场景
var sceneAsset = await Assets.LoadSceneAssetAsync(path);
await sceneAsset.LoadSceneAsync(LoadSceneMode.Additive);
await Assets.UnloadSceneAsync(path, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
```