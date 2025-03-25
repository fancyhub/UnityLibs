using System;
using System.Collections.Generic;
using FH.UI;
using FH;
namespace Game
{
    public class UIUpgraderPage : FH.UI.UIPageBase<Game.UIUpgraderView>, IUIUpdater
    {
        private List<FileDownloadJobInfo> _AllDownloadJobs = new List<FileDownloadJobInfo>();

        protected override void OnUI2Init()
        {
            base.OnUI2Init();
            BaseView._BtnUpgrade.OnClick = _OnBtnUpgradeClick;
            BaseView._BtnNext.OnClick = _OnBtnNextClick;
            BaseView._BtnNext.Active = false;            
        }

        protected override void OnUI3Show()
        {
            base.OnUI3Show();

            if(FileMgr.IsAllTagsReady())
            {
                UISceneMgr.ChangeScene<UISceneMain>();
            }
        }

        protected override void OnUI5Close()
        {

        }


        public void OnUIUpdate()
        {
            FileDownloadStat stat = _AllDownloadJobs.ExtGetSizeStat();
            BaseView._BtnNext.Active = stat.IsAllDone;
            BaseView._progress.value = stat.ProgressSize;
            BaseView._LblDesc.text = $"{stat.DownloadedSize}/{stat.TotalSize}";

            //Log.I($"{stat.IsAllDone}");

            //Log.I($"Download : {stat.DownloadingCount},{stat.PausedCount} {stat.FailedCount} {stat.SuccCount} {stat.TotalCount}");
            //Log.I($"Download : {stat.DownloadedSize}/{stat.TotalSize} ,{stat.ProgressSize}");
        }

        private void _OnBtnNextClick()
        {
            UISceneMgr.ChangeScene<UISceneMain>();
        }

        private void _OnBtnUpgradeClick()
        {
            //ResServiceUtil.DownloadFilesByAssets(new() { "Assets/Scenes/a.unity" });
            FH.ResServiceUtil.DownloadAllFiles();
            FH.FileDownloadMgr.GetAllInfo(_AllDownloadJobs);
            UISceneMgr.AddUpdate(this);
        }
    }
}