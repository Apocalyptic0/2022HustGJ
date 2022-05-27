using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/** <summary>�������</summary> */
[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class Window : MonoBehaviour
{
    /** <summary>��������</summary> */
    [Serializable]
    public struct EaseOptions
    {
        /** <summary>��������ʱ��</summary> */
        public float duration;
        /** <summary>����������ʽ</summary> */
        public Ease ease;

        /** <summary>�Ƿ�任͸����</summary> */
        public bool fade;
        /** <summary>��ʾʱ��͸����</summary> */
        public float alphaOnShow;
        /** <summary>����ʱ��͸����</summary> */
        public float alphaOnHide;

        /** <summary>�Ƿ�任��С</summary> */
        public bool scale;
        /** <summary>��ʾʱ�Ĵ�С</summary> */
        public Vector3 scaleOnShow;
        /** <summary>����ʱ�Ĵ�С</summary> */
        public Vector3 scaleOnHide;

        /** <summary>�Ƿ�任�ֲ�����</summary> */
        public bool move;
        /** <summary>��ʾʱ�ľֲ�����</summary> */
        public Vector3 posOnShow;
        /** <summary>����ʱ�ľֲ�����</summary> */
        public Vector3 posOnHide;

        /** <summary>���������Ƿ���Ч(ֻ��)</summary> */
        public bool IsValid
        {
            get
            {
                if (fade)
                    return true;
                if (scale)
                    return true;
                if (move)
                    return true;
                return false;
            }
        }

        /** <summary>Ĭ�϶�������(ֻ��)</summary> */
        public static readonly EaseOptions Default = new EaseOptions()
        {
            duration = 0.5f,
            ease = DG.Tweening.Ease.InOutQuad,

            fade = true,
            alphaOnShow = 1.0f,
            alphaOnHide = 0.0f,

            scale = false,
            move = false
        };

        /**
         * <summary>�������캯��</summary>
         * <param name="ref">Ҫ���ƵĶ�������</param>
         */
        public EaseOptions(EaseOptions @ref)
        {
            duration = @ref.duration;
            ease = @ref.ease;

            fade = @ref.fade;
            alphaOnShow = @ref.alphaOnShow;
            alphaOnHide = @ref.alphaOnHide;

            scale = @ref.scale;
            scaleOnShow = @ref.scaleOnShow;
            scaleOnHide = @ref.scaleOnHide;

            move = @ref.move;
            posOnShow = @ref.posOnShow;
            posOnHide = @ref.posOnHide;
        }

        /**
         * <summary>����͸���ȱ任����</summary>
         * <param name="fade">�Ƿ�任͸����</param>
         * <param name="alphaOnShow">��ʾʱ��͸����</param>
         * <param name="alphaOnHide">����ʱ��͸����</param>
         * <returns>������������</returns>
         */
        public EaseOptions Fade(bool fade, float alphaOnShow, float alphaOnHide)
        {
            this.fade = fade;
            this.alphaOnShow = alphaOnShow;
            this.alphaOnHide = alphaOnHide;
            return this;
        }
        /**
         * <summary>���ô�С�任����</summary>
         * <param name="scale">�Ƿ�任��С</param>
         * <param name="scaleOnShow">��ʾʱ�Ĵ�С</param>
         * <param name="scaleOnHide">����ʱ�Ĵ�С</param>
         * <returns>������������</returns>
         */
        public EaseOptions Scale(bool scale, Vector3 scaleOnShow, Vector3 scaleOnHide)
        {
            this.scale = scale;
            this.scaleOnShow = scaleOnShow;
            this.scaleOnHide = scaleOnHide;
            return this;
        }
        /**
         * <summary>��������任����</summary>
         * <param name="move">�Ƿ�任����</param>
         * <param name="posOnShow">��ʾʱ������</param>
         * <param name="posOnHide">����ʱ������</param>
         * <returns>������������</returns>
         */
        public EaseOptions Move(bool move, Vector3 posOnShow, Vector3 posOnHide)
        {
            this.move = move;
            this.posOnShow = posOnShow;
            this.posOnHide = posOnHide;
            return this;
        }
        /**
         * <summary>���û�����������</summary>
         * <param name="duration">����ʱ��</param>
         * <param name="ease">������ֵ������ʽ</param>
         * <returns>������������</returns>
         */
        public EaseOptions Ease(Ease ease, float duration = 1.0f)
        {
            this.ease = ease;
            this.duration = duration;
            return this;
        }
    }

    /** <summary>���嵱ǰ�Ƿ�ɼ�(ֻ��)</summary> */
    public bool Visible { get; private set; }
    public bool IsAnimating = false;

    public EaseOptions options = EaseOptions.Default;
    [SerializeField]
    [Tooltip("��ʼʱ�Ƿ���ʾ")]
    private bool _showOnLoad;

    [SerializeField]
    [Tooltip("��������")]
    private string windowName = "";

    private CanvasGroup _canvasGroup;

    private static readonly Dictionary<string, Window> windows = new Dictionary<string, Window>();

    private Tweener tweener;

    public bool Show(Action<Window> onShow = null)
    {
        return Show(options, onShow);
    }
    /**
     * <summary>������ʾ����</summary>
     * <param name="options">��������</param>
     * <param name="onShow">��ʾ���ʱ�Ļص�</param>
     * <returns>�Ƿ���ʾ�ɹ�</returns>
     */
    public bool Show(EaseOptions options, Action<Window> onShow = null)
    {
        if (IsAnimating)
            Complete();
        if (Visible)
            return false;
        if (options.IsValid)
        {
            IsAnimating = true;
            var progress = 0.0f;
            tweener = DOTween.To(() => progress, x => progress = x, 1.0f, options.duration)
                .SetEase(options.ease)
                .OnUpdate(() =>
                {
                    if (options.fade)
                        _canvasGroup.alpha = Mathf.Lerp(options.alphaOnHide, options.alphaOnShow, progress);
                    if (options.scale)
                        transform.localScale = Vector3.LerpUnclamped(options.scaleOnHide, options.scaleOnShow, progress);
                    if (options.move)
                        transform.localPosition = Vector3.LerpUnclamped(options.posOnHide, options.posOnShow, progress);
                })
                .OnComplete(() =>
                {
                    Visible = true;
                    IsAnimating = false;
                    _canvasGroup.blocksRaycasts = true;
                    onShow?.Invoke(this);
                })
                .Play();
        }
        else
        {
            Visible = true;
            IsAnimating = false;
            Refresh();
            onShow?.Invoke(this);
        }
        return true;
    }

    public bool Hide(Action<Window> onHide = null)
    {
        return Hide(options, onHide);
    }
    /**
     * <summary>�������ش���</summary>
     * <param name="options">��������</param>
     * <param name="onHide">�������ʱ�Ļص�</param>
     * <returns>�Ƿ����سɹ�</returns>
     */
    public bool Hide(EaseOptions options, Action<Window> onHide = null)
    {
        if (IsAnimating)
            Complete();
        if (!Visible)
            return false;
        if (options.IsValid)
        {
            IsAnimating = true;
            var progress = 1.0f;
            tweener = DOTween.To(() => progress, x => progress = x, 0.0f, options.duration)
                .SetEase(options.ease)
                .OnUpdate(() =>
                {
                    if (options.fade)
                        _canvasGroup.alpha = Mathf.Lerp(options.alphaOnHide, options.alphaOnShow, progress);
                    if (options.scale)
                        transform.localScale = Vector3.LerpUnclamped(options.scaleOnHide, options.scaleOnShow, progress);
                    if (options.move)
                        transform.localPosition = Vector3.LerpUnclamped(options.posOnHide, options.posOnShow, progress);
                })
                .OnComplete(() =>
                {
                    Visible = false;
                    IsAnimating = false;
                    _canvasGroup.blocksRaycasts = false;
                    onHide?.Invoke(this);
                })
                .Play();
        }
        else
        {
            Visible = false;
            IsAnimating = false;
            Refresh();
            onHide?.Invoke(this);
        }
        return true;
    }

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        windows[windowName] = this;
    }
    private void Start()
    {
        Visible = _showOnLoad;
        Refresh();
    }
    private void OnValidate()
    {
        Visible = _showOnLoad;
        Refresh();
    }

    private void Refresh()
    {
        if (!_canvasGroup)
            _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = Visible ? 1.0f : 0.0f;
        _canvasGroup.blocksRaycasts = Visible;
    }

    /**
     * <summary>������ʾ����</summary>
     * <param name="windowName">Ҫ��ʾ�Ĵ�������</param>
     * <param name="options">��������</param>
     * <param name="onShow">��ʾ���ʱ�Ļص�</param>
     * <returns>�Ƿ���ʾ�ɹ�</returns>
     */
    public static bool Show(string windowName, EaseOptions options, Action<Window> onShow = null)
    {
        if (windows.TryGetValue(windowName, out Window window))
            return window.Show(options, onShow);
        return false;
    }
    /**
     * <summary>�������ش���</summary>
     * <param name="windowName">Ҫ���صĴ�������</param>
     * <param name="options">��������</param>
     * <param name="onHide">�������ʱ�Ļص�</param>
     * <returns>�Ƿ����سɹ�</returns>
     */
    public static bool Hide(string windowName, EaseOptions options, Action<Window> onHide = null)
    {
        if (windows.TryGetValue(windowName, out Window window))
            return window.Hide(options, onHide);
        return false;
    }

    public static Window GetWindow(string windowName)
    {
        Window window;
        if (windows.TryGetValue(windowName, out window))
            return window;
        else
            return null;
    }
    public static void Show(string windowName)
    {
        Show(windowName, EaseOptions.Default);
    }
    public static void Hide(string windowName)
    {
        Hide(windowName, EaseOptions.Default);
    }

    public void Complete()
    {
        tweener.Kill(true);
        IsAnimating = false;
    }
}