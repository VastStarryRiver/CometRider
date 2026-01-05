using YooAsset;
using System.Collections;



namespace Invariable
{
    public class InitializeYooAsset : IStateNode
    {
        private StateMachine m_machine;

        public void OnCreate(StateMachine machine)
        {
            m_machine = machine;
        }

        public void OnEnter()
        {
            InitializeAssetSystem();
        }

        public void OnExit()
        {

        }

        public void OnUpdate()
        {

        }

        /// <summary>
        /// 初始化YooAsset资源管理系统
        /// </summary>
        private void InitializeAssetSystem()
        {
            if (YooAssets.Initialized)
            {
                m_machine.ChangeState<HotUpdateOver>();
                return;
            }

            YooAssetManager.Instance.SetWebInfo();

            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化资源管理系统");

            YooAssets.Initialize();

            YooAssets.SetOperationSystemMaxTimeSlice(1000);

            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化资源管理系统，成功！");

            ResourcePackage package = YooAssetManager.Instance.Package;

            GameManager.Instance.StartCoroutine(InitializePackage(package));
        }

        /// <summary>
        /// 初始化Package
        /// </summary>
        private IEnumerator InitializePackage(ResourcePackage package)
        {
            GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化Package");

            EPlayMode playMode = (EPlayMode)m_machine.GetBlackboardValue("EPlayMode");

            InitializationOperation initOperation = null;

            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var buildResult = EditorSimulateModeHelper.SimulateBuild(YooAssetManager.Instance.PackageName);
                var packageRoot = buildResult.PackageRootDirectory;
                var fileSystemParams = FileSystemParameters.CreateDefaultEditorFileSystemParameters(packageRoot);

                EditorSimulateModeParameters createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = fileSystemParams;

                initOperation = package.InitializeAsync(createParameters);
            }
            else if (playMode == EPlayMode.HostPlayMode)
            {
                string defaultHostServer = ConfigUtils.CDNPath;
                string fallbackHostServer = defaultHostServer;
                IRemoteServices remoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
                FileSystemParameters cacheFileSystemParams = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices);
                FileSystemParameters buildinFileSystemParams = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();

                HostPlayModeParameters createParameters = new HostPlayModeParameters();
                createParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                createParameters.CacheFileSystemParameters = cacheFileSystemParams;

                initOperation = package.InitializeAsync(createParameters);
            }

            yield return initOperation;

            if (initOperation.Status == EOperationStatus.Succeed)
            {
                GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化Package，成功！");
                m_machine.ChangeState<CheckCatalogUpdate>();
            }
            else
            {
                GameManager.Instance.InvokeEventCallBack("Launcher_ShowTips", "初始化Package，失败！");
            }
        }
    }
}