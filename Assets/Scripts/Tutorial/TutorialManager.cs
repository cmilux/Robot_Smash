using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    //local tutorial
    //shows one pop up at a time
    //when tutorialevents.raise(steps) fires for the current step, the pop up fades out the next step appears
    [Serializable]
    public class Step
    {
        public TutorialStep id;
        [TextArea] public string message;
        public float showDelay = 0.5f;      //pause before the pop up appears
        public float doneDelay = 1f;        //how long it stays after the player completes it
    }

    [Header("Steps")]
    [SerializeField] List<Step> _steps = new List<Step>();

    [Header("Pop up UI")]
    [SerializeField] GameObject _popupRoot;
    [SerializeField] CanvasGroup _popupGroup;        //set blocks raycasts = false so it never blocks inventory clicks
    [SerializeField] TextMeshProUGUI _messageText;
    [SerializeField] float _fadeTime = 0.25f;

    [Header("Bridge unlock")]
    //step that unlocks the bridge when completed
    [SerializeField] TutorialStep _unlockAfterStep = TutorialStep.Craft;

    int _index = -1;
    bool _running;
    Coroutine _routine;

    private void OnEnable() => TutorialEvents.OnStepDone += HandleStepDone;
    private void OnDisable() => TutorialEvents.OnStepDone -= HandleStepDone;

    private IEnumerator Start()
    {
        _popupGroup.alpha = 0f;
        _popupRoot.SetActive(false);

        //wait until the local player exists (playerspawner spawns it after the scene loads)
        yield return new WaitUntil(() =>
            NetworkManager.Singleton != null &&
            NetworkManager.Singleton.LocalClient != null &&
            NetworkManager.Singleton.LocalClient.PlayerObject != null);

        //no tutorial: unloock the bridge right away
        if (_steps.Count == 0 || !TutorialSettings.Enabled)
        {
            TutorialEvents.RaiseCompleted();
            yield break;
        }


        _running = true;
        ShowStep(0);
    }

    void ShowStep(int index)
    {
        _index = index;

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(_steps[index]));
    }

    IEnumerator ShowRoutine(Step step)
    {
        yield return new WaitForSeconds(step.showDelay);

        _messageText.text = step.message;
        _popupRoot.SetActive(true);
        yield return Fade(1f);
    }

    void HandleStepDone(TutorialStep done)
    {
        if (!_running || _index < 0) return;
        if (_steps[_index].id != done) return;      //ignore events that arent the current step
        if (done == _unlockAfterStep) TutorialEvents.RaiseCompleted();

        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(CompleteRoutine(_steps[_index]));
    }

    IEnumerator CompleteRoutine(Step step)
    {
        yield return new WaitForSeconds(step.doneDelay);
        yield return Fade(0f);
        _popupRoot.SetActive(false);

        _routine = null;

        int next = _index + 1;
        if (next >= _steps.Count)
        {
            FinishTutorial();
            yield break;
        }

        ShowStep(next);
    }

    IEnumerator Fade(float target)
    {
        float start = _popupGroup.alpha;
        float t = 0f;

        while (t < _fadeTime)
        {
            t += Time.unscaledDeltaTime;
            _popupGroup.alpha = Mathf.Lerp(start, target, t/_fadeTime);
            yield return null;
        }

        _popupGroup.alpha = target;
    }

    void FinishTutorial()
    {
        _running = false;
        TutorialEvents.RaiseCompleted();
    }
}
