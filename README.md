## upm 地址 https://github.com/OnClick9927/WooAsset.git#src

# WooAsset [文档](https://onclick9927.github.io/2023/05/19/Doc/WooAsset/0-WooAsset-%E7%AE%80%E4%BB%8B)
* 一个简单、高效、易扩展的 AssetBundle管理工具

* 支持各个模式切换（无需修改代码）
  * 编辑器模拟加载         (纯粹编辑器模拟)
  * 纯粹的C#空包，   （注意：本地不会有任何ab）
  * 发布 正常流程包       （比模式1 多一个版本检查）
  * 发布 游戏前期的体验包  (把资源拷贝到stream)
  * 支持边玩边下载
  * 支持先准备一部分，后续靠下载
* 运行时
  * 支持加载  常规资源、子资源、场景、Resources下资源、原生文件、未曾打包的文件资源
  * 支持直接加载在（沙盒、Stream、服务器的 Bundle）
  * 支持只可以路径加载的插件/代码
  * 支持分帧加载
  * 支持本地Bundle路径重写
  * 支持资源模糊搜索
  * 支持选择版本运行
  * 支持资源懒卸载/内存大小控制/自动卸载
  * 支持下载重试、等待时常控制
  * 支持加载流程个性化

* 编辑器
  * 支持分布式构建(支持mod)
  * 支持 shader 变体收集
  * 支持 构建 spriteatlas
  * 支持打包/加载Unity无法识别的文件
  * 支持打包流程自定义（自定义分包、版本规划、结束流程、自定义标签等）
  * 支持资源分析
  * 支持历史版本找回
  * 支持控制线上存在的版本个数
  * 支持打包报告
  * 支持 自动检索哪些Bundle 需要拷贝到 Stream
* 其他
  * 支持自定义资源加密
  * 包含本地资源服务器
  * Editor、Runtime均支持同步、异步、委托、携程
  * 支持webgl、win、osx、android


### 成功案例,欢迎加入我们一起交流（QQ 782290296）

<table>
<tr>
    <td>
      <div align="center">
        <image src="http://yxwlgame.com/wp-content/uploads/2023/07/游戏图标.png" style="width:64px;height:64px;"></image>
        <br>
        <a  href="http://yxwlgame.com/simplexx_home/" target="_blank">简单修仙</a>
      </div>
    </td>

  <td>
      <div align="center">
        <image src="https://github.com/OnClick9927/OnClick9927.github.io/blob/main/source/Webs/WooAsset_WEBGL/TemplateData/favicon.ico" style="width:64px;height:64px;"></image>
        <br>
        <a  href="https://onclick9927.github.io/2024/08/06/Doc/WooAsset/10-WooAsset-%E4%BE%8B%E5%AD%90WebGL/" target="_blank">2048(WebGL)</a>
 <br>
        <a  href="https://github.com/OnClick9927/WooAsset/tree/main/Examples/2048" target="_blank">源码</a>
      </div>
  </td>
   
</tr>
</table>

## 近期Star趋势
[![Stargazers over time](https://starchart.cc/OnClick9927/WooAsset.svg)](https://starchart.cc/OnClick9927/WooAsset)
