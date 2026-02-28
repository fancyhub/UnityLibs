using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;
using System;

namespace Game
{
    public class UITestShare : UIPageBase<UITestShareView>
    {
        private Texture2D _Snapshot;
        private string _FilePath;

        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = _OnBtnCloseClick;
            BaseView._BtnDownload.OnClick = _OnBtnDownloadClick;
            BaseView._BtnShare.OnClick = _OnBtnShareClick;
            BaseView._BtnSimuateCapture.OnClick = _OnDetectedCapture;
            BaseView._Img.ExtSetOverrideTexture(null);

            ShareUtil.StartScreenshotListen(_OnDetectedCapture);
        }

        protected override void OnUI5Close()
        {
            base.OnUI5Close();
            ShareUtil.StopScreenshotListen();

            if (_Snapshot != null)
            {
                GameObject.Destroy(_Snapshot);
                _Snapshot = null;
            }
        }

        private void _OnDetectedCapture()
        {
            ShareUtil.Capture((t) =>
            {
                if (BaseView == null)
                {
                    if (t != null)
                        GameObject.Destroy(t);
                    return;
                }

                BaseView._Img.ExtSetOverrideTexture(t);
                if (_Snapshot != null)
                {
                    GameObject.Destroy(_Snapshot);
                    _Snapshot = null;
                }
                _Snapshot = t;
            });
        }

        private void _OnBtnCloseClick()
        {
            this.UIClose();
        }

        private void _OnBtnDownloadClick()
        {
            _GetShareImageFilePath((file_path) =>
            {
                var date = DateTime.Now;
                string file_name = $"screenshot_{date}.jpg";
                ShareUtil.CopyLocalImage2Gallery(file_path, file_name);

                NoticeApi.ShowCommon("Download");
            });
        }

        private void _OnBtnShareClick()
        {
            _GetShareImageFilePath((file_path) =>
            {
                ShareUtil.ShareImageWithText("test title", "test text : https://www.google.com/ncr", file_path);
            });
        }

        private void _GetShareImageFilePath(Action<string> callBack)
        {
            if (!string.IsNullOrEmpty(_FilePath))
            {
                callBack(_FilePath);
                return;
            }

            ShareUtil.Capture((t) =>
            {
                if (t == null)
                {
                    callBack(null);
                    return;
                }

                string path = System.IO.Path.Combine(Application.persistentDataPath, "screenshot.jpg");
                System.IO.File.WriteAllBytes(path, t.EncodeToJPG());
                _FilePath = path;
                Log.I("screenshot path: {0}", path);
                GameObject.Destroy(t);
                callBack(_FilePath);
            }, null, BaseView._Img.rectTransform);
        }

    }
}