# 个性化
## 打包个性化
``` csharp
 //个性化打包流程
public abstract class IAssetBuild
{
//是否记录这个路径
bool GetIsRecord(string path) ;
//自定义 资源 Tag （可以按照tag加载一组资源）
List<string> GetAssetTags(string path) ;
//自定义版本规则
string GetVersion(string settingVersion, AssetTaskContext context);
//一个资源是不是可以忽略
 protected virtual bool IsIgnorePath(string path);
//更具一个路径返回资源类型（覆盖）
AssetType CoverAssetType(string path, AssetType type) ;

// 自定义资源分组
void Create(List<EditorAssetData> assets, List<EditorBundleData> result, EditorPackageData pkg);

//自定义加密
IAssetStreamEncrypt GetBundleEncrypt(EditorPackageData pkg, EditorBundleData data, IAssetStreamEncrypt en) ;
int GetEncryptCode(IAssetStreamEncrypt en);
IAssetStreamEncrypt GetEncryptByCode(int code)

}
```
### 打包管线 
内建（自带）/SBP（后续文章有）
``` csharp
public interface IBuildPipeLine
{
    bool BuildAssetBundles(string outputPath, AssetBundleBuild[] builds, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform);
    List<string> GetAllAssetBundles(BundleNameType nameType);
    List<string> GetAllDependencies(string assetBundleName, BundleNameType nameType);
    uint GetBundleCrc(string directory, string bundleName, BundleNameType nameType);
    string GetBundleHash(string directory, string bundleName, BundleNameType nameType);
    BuildAssetBundleOptions GetBundleOption(AssetTaskParams param, out string err);
}
```
### 分包优化
``` csharp
public interface IBundleOptimizer
{
    List<EditorBundleData> Optimize(List<EditorBundleData> builds, EditorPackageData buildPkg, IAssetsBuild build);
}
```
### 内建资源选择
``` csharp
public interface IBuildInBundleSelector
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="files">打包出的所有文件</param>
    /// <param name="buildInAssets">编辑器配置</param>
    /// <param name="buildInConfig">必须拷贝的文件</param>
    /// <param name="manifest">打包出来的配置文件（Merged）</param>
    /// <param name="exports">打包报告（Merged）</param>
    /// <returns></returns>
    string[] Select(string[] files, List<string> buildInAssets, List<string> buildInConfig, ManifestData manifest, List<PackageExportData> exports);
}
```
### 资源选择器
低代码打包，配合资源组分包 使用，用于选出需要的资源
``` csharp
    public interface IAssetSelector
    {
        List<EditorAssetData> Select(List<EditorAssetData> assets, AssetSelectorParam param);
    }
```


## 加载个性化
``` csharp

///写一个 class 继承于 AssetsSetting
public abstract class AssetsSetting
{
  //是否要检查总版本文件
  //比如 服务器给出版本 0.0.1，是否去版本记录里面查有没有这个版本
  public virtual bool CheckVersionByVersionCollection() => false;
  //拷贝stream 路径重写
  public virtual string GetStreamingFileUrl(string url){}
  //是否从缓存加载ab
  public virtual bool GetCachesDownloadedBundles() => false;
  //按照名字搜索时候，名字是否有后缀，可以近似实现 Resources.Load
  public virtual FileNameSearchType GetFileNameSearchType(){}
  
  //是否需要拷贝stream到沙盒
  public virtual bool NeedCopyStreamBundles() {}
  ///一帧内最多多少毫秒在加载资源
  public virtual long GetLoadingMaxTimeSlice(){}
    /// 如果本地没有是否需要保存文件
  // GetBundleAlwaysFromWebRequest 返回 true 该选项不起效
  public virtual bool GetSaveBytesWhenPlaying() => true;

  ///下载的路径
  //举例 ：https://xxx.xxx.xx
  //http://127.0.0.1:8080
  // Application.StreamingPath 
  protected virtual string GetBaseUrl() {}
  ///自定义 bundle 去哪里下载
public virtual string GetUrlByBundleName(string buildTarget, string bundleName) => $"{GetBaseUrl()}/{buildTarget}/{bundleName}";
 public virtual string GetUrlByBundleName(string buildTarget, string version, string bundleName) => $"{GetBaseUrl()}/{buildTarget}/{version}/{bundleName}";


  //永远从远端下载
   public virtual bool GetBundleAlwaysFromWebRequest() {}

  /// 下载超时
  public virtual int GetWebRequestTimeout() ;
  ///下载重试次数
  public virtual int GetWebRequestRetryCount() ;
  ///同时存在下载器最大个数
  public virtual int GetWebRequestCountAtSameTime() ;



//是否开启模糊加载
//Assets.loadAsset("Assets/a.png")
//开启后 Assets.loadAsset("Assets/a")
//注意 开启之后，同一个文件夹下不可以出现同名文件
   public virtual string GetFuzzySearch(){}
  ///是否自动卸载 AB
  //如果未开启，需要在合适的时机手动调用   Assets.UnloadBundles
  public virtual bool GetAutoUnloadBundle() {}


  ///资源加密方式
  public virtual IAssetStreamEncrypt GetEncrypt(int code) {}


  /// 自定义的资源生命周期管理，bundle和asset均可以
  public virtual IAssetLife GetAssetLife(){}

}
```


## 资源加密
``` csharp
//个性化加密

    public interface IAssetEncrypt
    {
        byte[] Encode(string bundleName, byte[] buffer);
        byte[] Decode(string bundleName, byte[] buffer);
        byte[] Decode(string bundleName, byte[] buffer, int offset, int length);


    }

```
