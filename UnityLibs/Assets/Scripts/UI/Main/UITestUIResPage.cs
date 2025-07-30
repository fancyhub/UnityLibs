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
            for (int i = 1; i < spriteNames.Length; i++)
                holder.ExtPreloadSprite(spriteNames[i]);
        }

        protected override void OnUI2Open()
        {
            base.OnUI2Open();
            BaseView._BtnClose.OnClick = this.UIClose;
            BaseView._BtnLoad.OnClick = _OnBtnLoadClick;

        }

        private void _OnBtnLoadClick()
        {
            BaseView._Img1.ExtAsyncSetSprite(spriteNames[0]);
            BaseView._Img2.ExtAsyncSetSprite(spriteNames[1]);

#if UNITY_2023_2_OR_NEWER
            _TestAync();
#endif
        }

#if UNITY_2023_2_OR_NEWER
        private async Awaitable _TestAync()
        {
            System.Threading.CancellationTokenSource c = new System.Threading.CancellationTokenSource();
            var a = ResMgr.AsyncCreate(UIButtonView.CPath,  c.Token);
            //c.Cancel();
            var p = await a;
            Debug.LogError("Done");
            p.AddUser(this);
            p.Get<GameObject>().transform.SetParent(BaseView.SelfRoot.transform, false);


            await TimerMgr.Wait(1000 * 5);

            p.RemoveUser(this);
        }
#endif
    }
}