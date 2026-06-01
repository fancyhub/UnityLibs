/*************************************************************************************
 * Author  : cunyu.fan
 * Time    : 2026/6/1
 * Title   :
 * Desc    :
*************************************************************************************/

using UnityEngine.UIElements;

namespace FH.EditorToolHub
{
    public interface IEditorToolModule
    {
        VisualElement CreateGUI(EditorToolModuleContext context);
    }

    public interface IEditorToolModuleLifecycle
    {
        void OnDisable();
    }

    public abstract class IMGUIEditorToolModule : IEditorToolModule
    {
        public VisualElement CreateGUI(EditorToolModuleContext context)
        {
            IMGUIContainer container = new IMGUIContainer(() => OnGUI(context));
            container.style.flexGrow = 1;
            return container;
        }

        protected abstract void OnGUI(EditorToolModuleContext context);
    }
}
