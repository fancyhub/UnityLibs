using FH;
using FH.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIViewTest : MonoBehaviour
{
    private Game.UIButtonView _btn;

    private UIViewLayerMgr _LayerMgr;
    // Start is called before the first frame update
    void Start()
    {
        FH.UI.UIObjFinder.Show();

        _LayerMgr = new UIViewLayerMgr(UISharedBG.Inst);
        _LayerMgr.AddLayer("test");

        _btn = FH.UI.UIBaseView.CreateView<Game.UIButtonView>(UIRoot.Root2D);
        _btn.OnClick = () => UIPageScene2.Create(this, _LayerMgr);
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
        ResServiceUtil.DownloadFilesByAssets(new() { "Assets/Scenes/a.unity" });
        ResServiceUtil.DownloadAllFiles();
    }

    public void _OnOpenView()
    {
        string content = VfsMgr.ReadAllText("Config/test.json");
        Debug.Log($"Config/test.json {content}");

        content = VfsMgr.ReadAllText("LuaProj/main.lua");
        Debug.Log($"LuaProj/main.lua {content}");
        UIPageScene2.Create(this, _LayerMgr);
    }

    [FH.Omi.Button]
    public void Testevent()
    {
#if UNITY_EDITOR
        FH.EventSet2Sample.TestEventSet.Test();
#endif
    }
}

public class UIPageScene2 : UIPageBase
{
    //private FH.UI.UIPanelVariantView _view;
    private UIViewLayerMgr _LayerMgr;

    public MonoBehaviour _Behaviour;
    public List<Coroutine> _Routinues = new List<Coroutine>();
    public List<SceneRef> _SceneRefList = new List<SceneRef>();

    public static UIPageScene2 Create(MonoBehaviour behaviour, UIViewLayerMgr layerMgr)
    {
        UIPageScene2 ret = new UIPageScene2();
        ret._Behaviour = behaviour;
        ret._LayerMgr = layerMgr;
        ret.UIOpen();
        return ret;
    }
    public string[] _SpriteNameList = new string[] { "s0", "s1", "s2" };

    protected override void OnUI1PrepareRes(IResInstHolder holder)
    {
        //base.OnUIPrepareRes(holder);
        //holder.PreCreate(FH.UI.UIPanelVariantView.CPath);
        foreach (var p in _SpriteNameList)
        {
            string path = UIResMapConfig.FindSprite(p);
            holder.PreLoad(path, true);
        }
    }

    protected override void OnUI5Close()
    {
        foreach (var p in _Routinues)
        {
            _Behaviour.StopCoroutine(p);
        }
        _Routinues.Clear();

        foreach (var p in _SceneRefList)
        {
            //p.Unload();
        }
        _SceneRefList.Clear();
    }

    protected override void OnUI4Hide()
    {
        Log.D("OnUIHide");
    }

    protected override void OnUI2Init()
    {
        Log.D("OnUIInit");
        //_view = CreateView<FH.UI.UIPanelVariantView>();
        //_view._btn_0.OnClick = _OnCloseClick;
        //_view._btn_1.OnClick = _OnTestSceneClick;
        //_view._btn_2.OnClick = _OnCloseFirstSceneClick;
        //_view._btn_4.OnClick = _OnCloseLastSceneClick;
        //_view._btn_3._Button2.onClick.AddListener(() =>
        //{
        //    //FH.UI.UIRedDotMgr.ResetIncrementFlag("vroot.test.scene");

        //    int index = UnityEngine.Random.Range(0, _SpriteNameList.Length);
        //    _view._img_0.ExtAsyncSetSprite(_SpriteNameList[index]);

        //    foreach (var p in _view._img_list)
        //    {
        //    }

        //});

        //_LayerMgr.AddView(_view, 0, this, EUIBgClickMode.Common);
    }

    protected override void OnUI3Show()
    {
        Log.D("OnUIShow");
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
