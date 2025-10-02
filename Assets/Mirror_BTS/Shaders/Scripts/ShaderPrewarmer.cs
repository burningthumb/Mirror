using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlatformShaderCollection
{
    [Tooltip("Runtime platform this ShaderVariantCollection applies to.")]
    public RuntimePlatform platform;

    [Tooltip("ShaderVariantCollection asset to use for this platform.")]
    public ShaderVariantCollection collection;
}

public class ShaderPrewarmer : MonoBehaviour
{
    [Header("Platform Collections")]
    [Tooltip("Assign ShaderVariantCollections per platform.")]
    public List<PlatformShaderCollection> platformCollections = new();

    [Header("Warmup Settings")]
    [Tooltip("Time budget in seconds per frame for warming up shaders.")]
    [Min(0.001f)] public float timeBudgetPerFrame = 0.01f;

    [Tooltip("Minimum progress delta before progress notifications are sent (0.005 = 0.5%).")]
    [Range(0.0001f, 1f)] public float progressNotificationStep = 0.005f;

    [Tooltip("Start warming automatically on Awake.")]
    public bool autoStart = true;

    [Header("Events")]
    [Tooltip("Invoked once when warmup completes.")]
    public UnityEvent onWarmupComplete;

    [Tooltip("Invoked when progress changes by at least ProgressNotificationStep.")]
    public UnityEvent<float> onProgress;

    /// <summary>Invoked when warmup completes (C# event).</summary>
    public event Action WarmupCompleted;

    /// <summary>Invoked when progress changes by at least ProgressNotificationStep (C# event).</summary>
    public event Action<float> ProgressChanged;

    private static ShaderPrewarmer _instance;

    // Early subscriber queues
    private static readonly List<Action> _earlyCompletedSubs = new();
    private static readonly List<Action<float>> _earlyProgressSubs = new();

    private ShaderVariantCollection _activeCollection;
    private bool _isWarming;
    private float _lastProgressReported;

    // Adaptive batch sizing
    private int _variantsPerCall = 1;
    private bool _batchSizeLearned = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            FlushQueuedSubscribers();
        }
        else if (_instance != this)
        {
            Debug.LogWarning("[ShaderPrewarmer] Multiple instances found. Static helpers will use the first one.");
        }

        _activeCollection = GetCollectionForPlatform(Application.platform);

        if (_activeCollection == null)
        {
            Debug.LogWarning($"[ShaderPrewarmer] No ShaderVariantCollection configured for {Application.platform}");
            NotifyProgress(1f);
		    NotifyComplete();
        }
        else if (autoStart)
        {
            StartWarmup();
        }
    }

    private void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    public void StartWarmup()
    {
        if (_activeCollection == null || _isWarming)
            return;

        Debug.LogWarning($"[ShaderPrewarmer] Warming: {Application.platform} using {_activeCollection.name} with {_activeCollection.variantCount - _activeCollection.warmedUpVariantCount} / {_activeCollection.variantCount} remaining");

        _isWarming = true;
        _lastProgressReported = 0f;
        NotifyProgress(0f);
    }

    private void Update()
	{
		if (!_isWarming || _activeCollection == null)
			return;

		float frameStart = Time.realtimeSinceStartup;
		int startCount = _activeCollection.warmedUpVariantCount;

		while (!_activeCollection.isWarmedUp)
		{
			// Either discover the batch size or reuse the learned one
			int l_batchSize = _batchSizeLearned ? _variantsPerCall : 1;

			_activeCollection.WarmUpProgressively(l_batchSize);
			NotifyProgress(CurrentProgress);

			// If still learning, see how many compiled in this frame
			if (!_batchSizeLearned && (Time.realtimeSinceStartup - frameStart >= timeBudgetPerFrame))
			{
				int compiledThisFrame = _activeCollection.warmedUpVariantCount - startCount;
				_variantsPerCall = Mathf.Max(1, compiledThisFrame);
				_variantsPerCall = Mathf.Min(5, compiledThisFrame);
				_batchSizeLearned = true;
				return; // finish this frame, continue next
			}

			if (Time.realtimeSinceStartup - frameStart >= timeBudgetPerFrame)
			{
				return;
			}
		}

		// Finished
		_isWarming = false;

		NotifyProgress(1f);
		NotifyComplete();
	}

	private void NotifyComplete()
	{
		onWarmupComplete?.Invoke();
		WarmupCompleted?.Invoke();
	}

	private void NotifyProgress(float progress)
    {
        if (progress >= 1f ||
            Mathf.Approximately(0.0f, progress) ||
            progress - _lastProgressReported >= progressNotificationStep)
        {
            _lastProgressReported = progress;
            onProgress?.Invoke(progress);
            ProgressChanged?.Invoke(progress);
        }
    }

    private float CurrentProgress
    {
        get
        {
            if (_activeCollection == null || _activeCollection.variantCount <= 0)
                return 1.0f;

            return Mathf.Clamp01(
                (float)_activeCollection.warmedUpVariantCount / _activeCollection.variantCount
            );
        }
    }

    private ShaderVariantCollection GetCollectionForPlatform(RuntimePlatform platform)
    {
        foreach (var entry in platformCollections)
        {
            if (entry.platform == platform)
                return entry.collection;
        }
        return null;
    }

    private static void FlushQueuedSubscribers()
    {
        if (_instance == null) return;

        foreach (var sub in _earlyCompletedSubs)
            _instance.WarmupCompleted += sub;
        _earlyCompletedSubs.Clear();

        foreach (var sub in _earlyProgressSubs)
            _instance.ProgressChanged += sub;
        _earlyProgressSubs.Clear();
    }

    // ----------------------------
    // Static helpers
    // ----------------------------
    public static void SubscribeCompleted(Action callback)
    {
        if (_instance != null)
            _instance.WarmupCompleted += callback;
        else
            _earlyCompletedSubs.Add(callback);
    }

    public static void UnsubscribeCompleted(Action callback)
    {
        if (_instance != null)
            _instance.WarmupCompleted -= callback;
        else
            _earlyCompletedSubs.Remove(callback);
    }

    public static void SubscribeProgress(Action<float> callback)
    {
        if (_instance != null)
            _instance.ProgressChanged += callback;
        else
            _earlyProgressSubs.Add(callback);
    }

    public static void UnsubscribeProgress(Action<float> callback)
    {
        if (_instance != null)
            _instance.ProgressChanged -= callback;
        else
            _earlyProgressSubs.Remove(callback);
    }
}
