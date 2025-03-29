using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FH;
using FH.UI;
using System.Threading.Tasks;

namespace Game
{
    public class UITestUIResPage : UIPageBase<UITestUIResView>
    {
        static string[] spriteNames = new string[] { "s0", "s1", "s2" };

        //protected override IResInstHolder CreateHolder()
        //{
        //    var ret = ResMgr.CreateHolder(false, false);
        //    PtrList += ret;
        //    return ret;
        //}

        protected override void OnUI1PrepareRes(IResInstHolder holder)
        {
            holder.PreCreate(UITestUIResView.CPath, 20);
            foreach (var p in spriteNames)
                holder.ExtPreloadSprite(p);
        }

        protected override void OnUI2Init()
        {
            base.OnUI2Init();
            BaseView._BtnClose.OnClick = this.UIClose;
            BaseView._Img1.ExtAsyncSetSprite(spriteNames[0]);
            BaseView._Img2.ExtAsyncSetSprite(spriteNames[1]);
            _TestAync();
        }


        private async Awaitable _TestAync()
        {
            System.Threading.CancellationTokenSource c = new System.Threading.CancellationTokenSource();
            var a = ResMgr.AsyncCreate(UIButtonView.CPath, this, c.Token);
            //c.Cancel();
            var p = await a;
            Debug.LogError("Done");
            p.Get<GameObject>().transform.SetParent(BaseView.SelfRoot.transform, false);
        }
    }
}