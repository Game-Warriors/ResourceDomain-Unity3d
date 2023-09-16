# ResourceDomain-Unity3d
![GitHub](https://img.shields.io/github/license/svermeulen/Extenject)

# Table Of Contents

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
<summory>

  - [Introduction](#introduction)
  - [Features](#features)
  - [Installation](#installation)
  - [Startup](#startup)
  - [How To Use](#how-to-use)
</summory>

# Introduction
This library provides centralized resource management, loading resources and prepare assets to access by some abstractions. by library features could retrieve objects such as asset bundles, scriptable objects, variables and etc, all over project. this package also contains the resumable download manager. the library implemented fully by C# language. The library dependents to Unity3D engine libraries and has internal unity editor to setup resources assets, prefabs and other features.

![Figure 1](../media/Images/Figure1.png?raw=true)

Support platforms: 
* PC/Mac/Linux
* iOS
* Android
* WebGL
* UWP App


* This library is design to be dependecy injection friendly, the recommended DI library is the [Dependency Injection](https://github.com/Game-Warriors/DependencyInjection-Unity3d) to be used.

```
```
This library used in the following games and apps:
</br>
[Street Cafe](https://play.google.com/store/apps/details?id=com.aredstudio.streetcafe.food.cooking.tycoon.restaurant.idle.game.simulation)
</br>
[Idle Family Adventure](https://play.google.com/store/apps/details?id=com.aredstudio.idle.merge.farm.game.idlefarmadventure)
</br>
[CLC BA](https://play.google.com/store/apps/details?id=com.palsmobile.clc)

# Features

# Installation
This library can be added by unity package manager form git repository or could be downloaded.
for more information about how to install a package by unity package manager, please read the manual in this link:
[Install a package from a Git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html)

# Startup
After adding package to using the library features, the main class “ResourceSystem” should be initialized. the class working and required other classes for its services. as describing in following: 

* ResourceSystem: This class provide all system feature and centralized loading assets. the features are loading, and hold persist sprites and other sprite assets, loading and manage defined variables and sync to remote data and manage local and remote asset contents. the feature present by three abstractions.

* ResourceConfig: This class provide required configurations value to resource system.

    * ShiftCount: The number of bits count which will apply left shift bitwise operator to integer values. this is for the manipolating exist interget in memory values bacome harder. 

    * IsPreloadBundles: The Flag which indicate the downloaded or cached bundles is loading in start up and pending loading process to finish.

    ```csharp
    public class ResourceConfiguration : IStorageConfig
    {
        int IResourceConfig.ShiftCount => 1;
        bool IResourceConfig.IsPreloadBundles => true;
    }
    ```
* ContentDownloader: This class provide downloading content manager and storing downloaded contents service. the download manager has limit parallel download feature and when limit number has reached the download request is queue and wait for some download finish to start new download from queue. the storing feature use file system and Unity3D playfabs feature to handle content persist state.

* RemoteDataHandler: This class provide remote data syncing and is bridge for server remote configuration. 

In order to initialize resource system, the “ResourceSystem” class should constructed, and Resource Config is required for system so has to Initialized and passed to constructor. the Content Downloader and Remote Data Handler is optional to initialize. after creating resource system, the WaitForLoading method should called. in the WaitForLoading all loading resource will do and because this operation is blocking process, its return task object. like following example.

```csharp
private async void Awake()
{
    DefaultContentDownloader contentDownloader = new DefaultContentDownloader();
    ResourceConfiguration resourceConfiguration = new ResourceConfiguration();
    ResourceSystem resourceSystem = new ResourceSystem(null, contentDownloader, resourceConfiguration);
    await resourceSystem.WaitForLoading();
    // All resources could access and retrieve here, after loading
}
```

# How To Use
In order to start using features there is three abstractions. the abstractions will brief in this section. Some abstractions feeding by data using system editor window. the editor window could be access in Unity3D by top navigation bar select “Tools/Resource Configuration”.

* ISpriteDatabase: The base abstraction which presents sprite fetching operations. the sprite could add in persist sprite container which persist over application lifetime in memory. The persist sprites could be added in system editor in sprite tab. the added sprites could be look up by its name. sprites could be retrieved by its asset name as key using “GetPersistSprite“ method.

    * GetSpriteFromCollection: Finding the sprit collection in sprite container by the input key and return the specific sprite at the requested index.

    * SpriteCollectionItemCount: Retrieving the sprite count in the specific sprite collection.

    * GetPersistSprite: Finding the specific sprite by key which is the name of asset. the asset should have been added to Sprite tab of the system editor. all persist sprites loaded in memory in system start up and ready to use. multiply

    * FindSpriteCollection: Finding the specific sprite collection object by key which is the name of asset. the sprite collection should have been added to Unity Object section of system editor to be retrievable at runtime.

    ```csharp
    public interface ISpriteDatabase
    {
        Sprite GetSpriteFromCollection(string key, int index);
        int SpriteCollectionItemCount(string key);
        Sprite GetPersistSprite(string key);
        ISpriteCollection FindSpriteCollection(string key);
    }
    ```
    ![Figure 2](../media/Images/Figure2.png?raw=true)

* “SpriteCollection”: A data collection of sprites which should be implemented by driving from Unity3D scriptable objects and “ISpriteCollection“ abstraction. the asset must be created from that implemented class. Created asset should be added in object section of resource system to be retrieved. finally, the asset could be fetched by its asset name as key using “FindSpriteCollection“ method in “ISpriteCollection“ abstraction type.
The “ISpriteCollection“ has some property and method as describe following.

    * ItemCount: The sprite count which is exist in sprite collection.

    * GetSprites: Iterate on exist sprite in sprite collection.

    * FindSprite: Find the target sprite by name.

    * GetSpriteByIndex: Find the target sprite by index. this method may work if sprite collection be indexable.

    ```csharp
    public interface ISpriteCollection
    {
        int ItemCount { get; }
        IEnumerable<Sprite> GetSprites { get; }
        Sprite FindSprite(string name);
        Sprite GetSpriteByIndex(int index);
    }
    ```
* VariableDatabase: The base abstraction to access variable data which exists in resource system. the variables data could be added in three exists primitive type in editor, in other word the system just support string, float and integer data type to fetch from system. the input data section has two field.

    * Name: The name of input variable which should be unique in variable type scope, and it will use for retrieving as key in runtime.

    * Value: The initial and local value for define data. The expression of local is because the value may change in runtime by remote data. when the remote data receive from server the return value update by the new remote value and local value will be ignored.

    ![Figure 3](../media/Images/Figure3.png?raw=true)

    The database has other feature for loading any object which drive or be any kind of Unity3D Object. the object should create or exist in project and add to object section. the objects could be fetched in run time by its own name as key.

    ![Figure 4](../media/Images/Figure4.png?raw=true)

    The abstraction has two methods for fetching data.

    1. __GetDataObject__: This method could retrieve all Objects asset which added to Unity Object section in system editor before. all objects loaded in memory in system start up and ready to use by zero loading time. the key is name of object which present in editor.

    2. __GetVariableObject__: This method could retrieve all values which is added in String, Float, Integer tab in system editor. the key value is specific name of the value which is input in editor for each item.

    ```csharp
    public interface IVariableDatabase
    {
        T GetDataObject<T>(string key) where T : UnityEngine.Object;
        T GetVariable<T>(string variableKey) where T : IConvertible;
    }
    ```

* IContentDatabase: The base abstraction which presents content providing features like, downloading contents from remote, loading local or downloaded contents and unload, remove downloaded contents.

    * DownloadingCount: The count of current in progress download in download manager.

    * GetContent: This method retrieves unity object in content container. Start searching for target object in exists assetbundles, if item doesn’t find then start searching in object which is in unity resources folder. the object will load if it isn’t existing in memory and operation will be IO bound although may return immediately if loaded before.

    * UnloadContent: This method will unallocated memory usage for specific content. if the target content be a asset bundle, the bundle unload from memory and the “unloadAllObjects“ flag indicating to unload all related asset to assetbundle, then if the content found in unity resource folder, will unallocated from unity resource system.

    ```csharp
    public interface IContentDatabase
    {
        int DownloadingCount { get; }
        event Action<string> OnContentDownloadComplete;
        event Action<string> OnContentDownloadFailed;

        bool IsContentLoaded(string bundleName);
        T GetContent<T>(string contentName) where T : UnityEngine.Object;
        void GetContentAsync<T>(string contentName, Action<T> onLoad) where T : UnityEngine.Object;
        float DownloadProgress(string bundleName);
        void ForceStartDownload(string bundleName, Action<bool> onStart);
        bool IsBundleDownloaded(string bundleName);
        void UnloadContent(string contentName, bool unloadAllObjects);
        void RemoveBundle(string bundleName);
        bool IsBundleExist(string bundleName);
        bool IsBundleLoaded(string bundleName);
        void LoadBundleAsync(string bundleName, Action<string> onLoadDone);
    }
    ```