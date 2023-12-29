using FH;
using FH.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIViewTest : MonoBehaviour
{
    public Canvas Canvas;
    private FH.UI.UIButtonView _btn;
    private FH.UI.ScreenSafeAreaCalculator _screenSafeAreaCalculator;
    // Start is called before the first frame update
    void Start()
    {
        UIObjFinder.Show();

        _btn = FH.UI.UIBaseView.CreateView<FH.UI.UIButtonView>(Canvas.transform);
        _btn.OnClick = _OnOpenView;
        _screenSafeAreaCalculator = new FH.UI.ScreenSafeAreaCalculator(Canvas.GetComponent<CanvasScaler>());

        UIRedDotMgr.Init();
        UIRedDotMgr.Link("root.test.scene", "vroot.test.scene.vscene");
    }

    public void Update()
    {
        if (_screenSafeAreaCalculator.CalcSafeArea(out var ui_resolution, out var safe_area))
        {
            UISafeAreaPanel.ChangeSafeArea(ui_resolution, safe_area, Canvas.transform);
        }
        UIRedDotMgr.Update();
    }

    public void OnEnable()
    {
    }

    public void OnApplicationQuit()
    {
        UIRedDotMgr.Clear();
    }

    public void _DownloadFile()
    {
        TaskQueue.Init(10);

        string url = "http://127.0.0.1:8080/Apps.ppkg";
        string local_path = "Bundle/Download/Apps.ppkg";
        HttpDownloaderError error = default;
        TaskQueue.AddTask(() =>
        {
            error = HttpDownloader.Download(url, local_path, (current, total) =>
            {
                Debug.Log($"Download: {current}/{total} {(float)current / total}");
            }, 0, System.Threading.CancellationToken.None);
        },
        () =>
        {
            Debug.Log("Download All: ");
            error.PrintLog();
        });
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
        _Holder.PreCreate(FH.UI.UIPanelVariantView.C_AssetPath);
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
                UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
                break;
            }
            yield return string.Empty;
        }

        for (; ; )
        {
            if (scene_b.IsDone)
            {
                _SceneRefList.Add(scene_b);
                UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
                break;
            }
            yield return string.Empty;
        }
    }

    private void _OnCloseClick()
    {
        this.Destroy();
        UIRedDotMgr.Set("root.test.scene", 0);
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

        UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
    }


    private void _OnCloseLastSceneClick()
    {
        if (_SceneRefList.Count == 0)
            return;
        _SceneRefList[_SceneRefList.Count - 1].Unload();
        _SceneRefList.RemoveAt(_SceneRefList.Count - 1);
        UIRedDotMgr.Set("root.test.scene", _SceneRefList.Count);
    }
}
