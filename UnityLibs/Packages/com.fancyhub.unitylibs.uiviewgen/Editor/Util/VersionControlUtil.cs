using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;

namespace FH.UI.ViewGenerate.Ed
{
    public static class VersionControlUtil
    {

        public static bool Checkout(string path)
        {
            if (!Provider.enabled || !Provider.isActive)
                return true;

            var task = Provider.Checkout(path,CheckoutMode.Asset);
            task.Wait();
            return task.success;
        }         
    }
}
