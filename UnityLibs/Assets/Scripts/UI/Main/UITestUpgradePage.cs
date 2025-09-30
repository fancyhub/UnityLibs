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
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;
            BaseView._BtnUpgrade.OnClick = ()=> _UpgradeToVersion(BaseView._VersionInput.text.Trim()); 
            BaseView._BtnBackToBase.OnClick = ()=> _UpgradeToVersion(FileMgr.GetBaseVersionInfo().ToString());
            BaseView._CDNInput.text = FileDownloadMgr.GetDefaultSvrUrl();
            BaseView._CDNInput.onValueChanged.AddListener((url) =>
            {
                FileDownloadMgr.SetDefaultSvrUrl(url.Trim());
            });
            _RefreshVersionInfo();
        }

        private void _RefreshVersionInfo()
        {
            BaseView._VersionInfo.text = $"Base Version: {FileMgr.GetBaseVersionInfo()} \nCurr  Version: {FileMgr.GetCurrentVersionInfo()}";
        }

        private void _OnBtnCloseClick()
        {
            if (_is_inprogress)
            {
                NoticeApi.ShowCommon("in upgrade progress");
                return;
            }

            this.UIClose();
        }     

        private void _UpgradeToVersion(string version)
        {
            if (_is_inprogress)
            {
                NoticeApi.ShowCommon("upgrade is in progress");
                return;
            }
            if (string.IsNullOrEmpty(version))
            {
                NoticeApi.ShowCommon("input is null");
                return;
            }

            _is_inprogress = true;

#if UNITY_2023_2_OR_NEWER
            _UpgradeWrapper(version);
#else 
            GlobalCoroutine.StartCoroutine(_UpgradeWrapperRoutine(version));
#endif
        }

        private IEnumerator _UpgradeWrapperRoutine(string version)
        {
            ResRef old_res = ResMgr.Load(UIButtonView.CPath);
            old_res.AddUser(this);

            ResRef old_inst = ResMgr.Create(UIButtonView.CPath, this, true);

            Log.I("begin fetch file manifest");
            FileManifest file_manifest = null;
            yield return ResService.FetchRemoteFileManifestRoutine(version, (c) => { file_manifest = c; });
            Log.I("fetch file manifest done");

            if (file_manifest == null)
            {
                _is_inprogress = false;
                NoticeApi.ShowCommon("upgrade failed");
                yield break;
            }

            if(FileMgr.GetCurrentVersionInfo() == new VersionInfo(file_manifest.Version))
            {
                _is_inprogress = false;
                NoticeApi.ShowCommon("version is same");
                yield break;
            }

            List<FileManifest.FileItem> list = new List<FileManifest.FileItem>();

            Log.I("download all files");
            var job_list = ResService.DownloadAllFiles(file_manifest);
            for (; ; )
            {
                var stat = job_list.ExtGetSizeStat();
                if (stat.IsAllDone)
                    break;

                BaseView._Progress.value = stat.ProgressSize;
                yield return null;
            }
            BaseView._Progress.value = 1.0f;
            Log.I("all files download");

            yield return ResService.UpgradeRoutine(file_manifest);
            Log.I("upgrade done");
            NoticeApi.ShowCommon("upgrade succ");
            _RefreshVersionInfo();
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