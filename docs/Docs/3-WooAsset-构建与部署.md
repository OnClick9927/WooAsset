# 构建与部署
## 构建
* 设置好需要打包的组
* 点击Tools/WooAsset/Build
* 稍等片刻即可完成打包，内容全都在Sever目录下
* 按照Server 文件夹 拷贝到CDN即可

## 部署
```
CDN
└─android
    ├─v1.0（APP版本）
    ├─v1.1（APP版本）
    └─v2.0（APP版本）
    └─版本列表
└─iphone
    ├─v1.0（APP版本）
    ├─v1.1（APP版本）
    └─v2.0（APP版本）
    └─版本列表
```

如果版本有服务器、版本可以是服务器告知的，所以 各个平台的 版本列表可以不上传
```
CDN
└─android
    ├─v1.0（APP版本）
    ├─v1.1（APP版本）
    └─v2.0（APP版本）
└─iphone
    ├─v1.0（APP版本）
    ├─v1.1（APP版本）
    └─v2.0（APP版本）
```