## 环境准备
- ## 平台 ：
Win10/Win11 
- ## 工具链：
  - Slay the Spire 2
  - Visual Studio 2012 或更高版本
  - Godot v4.5.1
  - Rider 2025.3.3 或更高版本
  - .NET 9.0 或更高版本
- ## 源码下载与编译
1. 使用```git clone https://github.com/Air695/SpaceTimeWitch.git``` 下载完整项目源码
2. 修改 ```local.props```中的 Slay the Spire 2地址
3. 运行 ```dotnet build```

- ## Godot资源打包与导出
1. 使用Godot v4.5.1导入项目目录
2. ```项目>导出>Windows Desktop``` 导出路径配置为 ```$(Sts2Dir)\mods\SpaceTimeWitch\SpaceTimeWitch.pck```