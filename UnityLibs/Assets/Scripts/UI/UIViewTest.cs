using FH;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIViewTest : MonoBehaviour
{
    public Canvas Canvas;
    private FH.UI.UIButtonView _btn;
    // Start is called before the first frame update
    void Start()
    {
        FH.UI.UIObjFinder.Show();

        _btn = FH.UI.UIBaseView.CreateView<FH.UI.UIButtonView>(Canvas.transform);
        _btn.OnClick = _DownloadFile;


        TaskQueue.Init(10);
        FH.UI.UIRedDotMgr.Init();
        FH.UI.UIRedDotMgr.Link("root.test.scene", "vroot.test.scene.vscene");
        FH.NoticeSample.NoticeApi.Init();
    }

    public void Update()
    {
        FH.UI.UIRedDotMgr.Update();
    }

    public void OnEnable()
    {
    }

    public void OnApplicationQuit()
    {
        FH.UI.UIRedDotMgr.Clear();
    }

    public void _DownloadFile()
    {
        var manifest = FileMgr.GetCurrentManifest();
        if (manifest == null)
            return;
        FileDownloadMgr.AddTasks(manifest.Files);
    }

    public void _OnOpenView()
    {
        string content = VfsMgr.ReadAllText("Config/test.json");
        Debug.Log($"Config/test.json {content}");

        content = VfsMgr.ReadAllText("LuaProj/main.lua");
        Debug.Log($"LuaProj/main.lua {content}");
        UIPageScene.Create(Canvas.transform, this);
    }
}

public sealed class UIPageScene : CPoolItemBase
{
    private Transform _UIRoot;
    private FH.UI.UIPanelVariantView _view;
    private IResInstHolder _Holder;
    public PtrList _PtrList;
    public MonoBehaviour _Behaviour;
    public List<Coroutine> _Routinues = new List<Coroutine>();
    public List<SceneRef> _SceneRefList = new List<SceneRef>();

    public static UIPageScene Create(Transform root, MonoBehaviour behaviour)
    {
        var ret = GPool.New<UIPageScene>();
        ret._UIRoot = root;
        ret._Behaviour = behaviour;
        ret._Open();
        return ret;
    }

    private void _Open()
    {
        _Holder = ResMgr.CreateHolder(false, false);
        _Holder.PreCreate(FH.UI.UIPanelVariantView.CPath);
        _PtrList += _Holder;

        _Routinues.Add(_Behaviour.StartCoroutine(_OpenView()));
    }

    private IEnumerator _OpenView()
    {
        for (; ; )
        {
            var stat = _Holder.GetStat();
            Log.D(stat.ToString());
            if (stat.IsAllDone)
                break;
            else
                yield return string.Empty;
        }

        _view = FH.UI.UIBaseView.CreateView<FH.UI.UIPanelVariantView>(_UIRoot, _Holder);
        _view._btn_0.OnClick = _OnCloseClick;
        _view._btn_1.OnClick = _OnTestSceneClick;
        _view._btn_2.OnClick = _OnCloseFirstSceneClick;
        _view._btn_4.OnClick = _OnCloseLastSceneClick;

        _PtrList += _view;
    }


    private void _OnTestSceneClick()
    {
        _Routinues.Add(_Behaviour.StartCoroutine(_TestSceneLoad()));
    }

    private IEnumerator _TestSceneLoad()
    {
        yield return new WaitForSeconds(1.0f);

        var scene_a = SceneMgr.LoadScene("Assets/Scenes/a.unity", true);
        var scene_b = SceneMgr.LoadScene("Assets/Scenes/a.unity", true);


        for (; ; )
        {
            if (scene_a.IsDone)
            {
                _SceneRefList.Add(scene_a);
                FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
                break;
            }
            yield return string.Empty;
        }

        for (; ; )
        {
            if (scene_b.IsDone)
            {
                _SceneRefList.Add(scene_b);
                FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
                break;
            }
            yield return string.Empty;
        }
    }

    private void _OnCloseClick()
    {
        this.Destroy();
        FH.UI.UIRedDotMgr.Set("root.test.scene", 0);
    }

    protected override void OnPoolRelease()
    {
        foreach (var p in _Routinues)
        {
            _Behaviour.StopCoroutine(p);
        }
        _Routinues.Clear();

        foreach (var p in _SceneRefList)
        {
            p.Unload();
        }
        _SceneRefList.Clear();

        _PtrList?.Destroy();
        _PtrList = null;
    }

    private void _OnCloseFirstSceneClick()
    {
        if (_SceneRefList.Count == 0)
            return;
        _SceneRefList[0].Unload();
        _SceneRefList.RemoveAt(0);

        FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
    }


    private void _OnCloseLastSceneClick()
    {
        if (_SceneRefList.Count == 0)
            return;
        _SceneRefList[_SceneRefList.Count - 1].Unload();
        _SceneRefList.RemoveAt(_SceneRefList.Count - 1);
        FH.UI.UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
    }
}
