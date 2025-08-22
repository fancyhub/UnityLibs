using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;

namespace Game
{
    public class UITestUpgradePage : UIPageBase<UITestUpgradeView>
    {
        private bool _is_inprogress = false;
        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            this.ResHolder.PreInst(UIButtonView.CPath, 2);
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;
            BaseView._BtnUpgrade.OnClick = _OnBtnUpgradeClick;
            BaseView._CurVersion.text = "Current Version: " + FileMgr.GetVersionInfo();
        }

        private void _OnBtnCloseClick()
        {
            if (_is_inprogress)
                return;

            this.UIClose();
        }

        private void _OnBtnUpgradeClick()
        {
            if (_is_inprogress)
                return;
            string version = BaseView._Version.text.Trim();
            if (string.IsNullOrEmpty(version))
                return;

#if UNITY_2023_2_OR_NEWER
            _is_inprogress = true;
            _UpgradeWrapper(version);
#endif
        }
#if UNITY_2023_2_OR_NEWER
        private async Awaitable _UpgradeWrapper(string version)
        {            
            ResRef old_res = ResMgr.Load(UIButtonView.CPath);
            old_res.AddUser(this);

            ResRef old_inst = ResMgr.Create(UIButtonView.CPath, this, true);

            await _Upgrade(version);
            NoticeApi.ShowCommon("upgrade done");
            _is_inprogress = false;

            ResRef new_res = ResMgr.Load(UIButtonView.CPath);
            new_res.AddUser(this);

            ResRef new_inst = ResMgr.Create(UIButtonView.CPath, this);

            Log.Assert(old_res.Id != new_res.Id, "资源相同,有问题");

            old_inst.RemoveUser(this);
            new_inst.RemoveUser(this);
            old_res.RemoveUser(this);
            new_res.RemoveUser(this);
        }

        private async Awaitable _Upgrade(string version)
        {
            Log.I("begin fetch file manifest");
            FileManifest file_manifest = await ResService.FetchRemoteFileManifest(version);
            Log.I("fetch file manifest done");

            if (file_manifest == null)
                return;

            List<FileManifest.FileItem> list = new List<FileManifest.FileItem>();

            Log.I("download all files");
            var job_list = ResService.DownloadAllFiles(file_manifest);
            for (; ; )
            {
                var stat = job_list.ExtGetSizeStat();
                if (stat.IsAllDone)
                    break;

                BaseView._Progress.value = stat.ProgressSize;
                await Awaitable.NextFrameAsync();
            }
            BaseView._Progress.value = 1.0f;
            Log.I("all files download");

            await ResService.Upgrade(file_manifest);
            Log.I("upgrade done");
            BaseView._CurVersion.text = "Current Version: " + FileMgr.GetVersionInfo();
        }
#endif
    }
}