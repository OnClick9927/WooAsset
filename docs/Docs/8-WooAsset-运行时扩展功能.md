# 运行时扩展

## 实例化 物体
``` csharp
// 创建
  var oppp = await Assets.InstantiateAsync(path, parent);

// 干点啥

  //销毁
  //销毁时候会自动改变引用计数，不需要手动Assets.Release
  oppp.Destroy();
```
## AssetReference && 使用例子
``` csharp
  [System.Serializable]
  public class AssetReference<T> : AssetReference where T : UnityEngine.Object
  {
      public override Type type => typeof(T);
  }
```

``` csharp
    public class AssetExample : UnityEngine.MonoBehaviour
    {
        public Image image;
        public AssetReference<UnityEngine.Sprite> assetReference;

       async void Test(){
          var asset = await assetReference.LoadAssetAsync();
          await asset;
          image.sprite = asset.GetAsset<UnityEngine.Sprite>();
        }
    }

```
## 一些扩展方法
``` csharp
//使用时机-》avpro这种只能用路径的插件
public static string GetRawAssetUrlOrPath(assetpath)
//获得所有资源路径
 public static IReadOnlyList<string> GetAllAssetPaths() ;
 //获得一个tag 的所有路径
 public static IReadOnlyList<string> GetTagAssetPaths(string tag) ;
 //获得所有资源路径
 public static IReadOnlyList<string> GetAllTags();

 //更具tag 获得唯一 资源
 public static string GetUniqueAssetPathByTag(string tag);
 //获得所有满足条件的资源
 public static IReadOnlyList<string> GetAssetPath(Func<AssetData, bool> fit);
 //获得唯一满足条件的资源
 public static string GetUniqueAssetPath(Func<AssetData, bool> fit);
 //通过名字找到名字一致的资源
 public static IReadOnlyList<string> GetAssetsByAssetName(string name) ;
//得到对应路径数据
 public static AssetData GetAssetData(string assetPath);
```
## 资源组加载
### 何时使用：某些地方必须需要同步加载资源（先准备一下）/ 一次型加载配置表，读取到内存之后，卸载

``` csharp
//准备一组资源
string[] groups ;
var assets= await Assets.PrepareAssets(groups)
//或者按照 tag 准备一组资源
var assets=Assets.PrepareAssetsByTag(tag)



///加载对应的资源方法一
string path;
var asset = assets.FindAsset(path)
///加载对应的资源方法二
var asset = Assets.LoadAssetAsync(path)
///注意：方式二会增加引用计数，需要在合适的地方 Assets.Release(asset)
///方式一不会增加引用计数



///把整组资源全都卸载了
assets.Release();


///配合资源模糊搜索一起使用
//使用场景，进入战斗场景之前把战斗需要的资源全加载
```
## 方便的资源卸载
### 基础方式
* 场景
* 一个ui界面上面有一个image
* 运行时候需要不停的替换image的sprite
* 界面关闭的时候需要把image的sprite卸载
``` csharp
///所有的设置图片都走这个方法
public static async void SetSprite(Image image, string path)
{
    var asset = await Assets.LoadAssetAsync(path);
    if (asset.isErr) return;
    image.sprite = asset.GetAsset<Sprite>();
    Assets.AddBridge(new GameObjectBridge(image.gameObject,asset));
}
///在合适的时候调用一次即可（比如：切换场景时候）
public static void ReleaseUselessBridges()
{
    Assets.ReleaseUselessBridges();
}
```

其他类型的资源/组件，可以 继承 AssetBridge< T > 自行实现即可
### 按照功能分组的释放的方式
* 场景
* 一个ui界面上面有一个image
* 运行时候需要不停的替换image的sprite
* 界面关闭的时候需要把image的sprite卸载
* 与上一种方式对比，gc会少一些
``` csharp
    public static async void SetSprite(string key, UnityEngine.UI.Image image, string path, Action callback = null)
    {
        if (string.IsNullOrEmpty(path))
            image.sprite = null;
        else
        {
            var collection = Assets.GetAssetCollection(key);
            var asset = collection.Get(path, () => Assets.LoadAssetAsync<Sprite>(path));
            await asset;
            Sprite sp = (asset as WooAsset.Asset).GetAsset<Sprite>();
            image.sprite = sp;
        }
        callback?.Invoke();
    }

    //释放
    public static void ClearAssetCollection(string key)
    {
        Assets.ClearAssetCollection(key);
    }

```

### 更加方便的方式（有风险）
``` csharp
public class LocalSetting : AssetsSetting
{
    public override IAssetLife GetAssetLife()
    {
      /// 参数是缓存的内存大小，超过这个数字会自动卸载最早的资源
        return new LRULife(1024 * 50);
    }
}
Assets.SetAssetsSetting(new LocalSetting());

```
* 可以实现 bundle 不自动卸载，到达一定大小在开始卸载
* 如果内存不足时候，最早被使用的资源会被优先卸载
* 内存设置的别太小，容易出现资源丢失
