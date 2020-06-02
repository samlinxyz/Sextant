using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PauseMenu : MonoBehaviour
{
    private static bool paused = false;
    [SerializeField]
    private GameObject pauseMenu = null;
    [SerializeField]
    private GameObject playCanvas = null;
    [SerializeField]
    private CanvasGroup pauseMenuCanvasGroup = null;
    [SerializeField]
    private CanvasGroup playCanvasGroup = null;

    [SerializeField]
    private Player player = null;

    [SerializeField]
    private PostProcessVolume postProcessing = null;
    private DepthOfField menuBlur = null;

    private void Start()
    {
        // Tweens use unscaled time.
        DOTween.defaultTimeScaleIndependent = true;

        postProcessing.profile.TryGetSettings(out menuBlur);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && player.allowMovement)
        {
            TogglePause();
        }
    }

    private void TogglePause()
    {
        if (paused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        AnimatePauseMenuAndPlayUI(fadeInPauseMenu: true);
        paused = true;
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        AnimatePauseMenuAndPlayUI(fadeInPauseMenu: false);
        paused = false;
    }
    public void ReturnToLevelFromPaused()
    {
        Time.timeScale = 1f;
        AnimatePauseMenuAndPlayUI(fadeInPauseMenu: false, exitingPlayModeFromPause: true);
        paused = false;
        GameManager.instance.ReturnToLevel();
    }

    private Sequence pauseMenuAndPlayUIAnimation = null;

    public void AnimatePauseMenuAndPlayUI(bool fadeInPauseMenu, bool exitingPlayModeFromPause = false)
    {
        if (pauseMenuAndPlayUIAnimation != null)
        {
            pauseMenuAndPlayUIAnimation.Kill();
        }

        if (player.PlayCanvasGroupFade != null)
        {
            player.PlayCanvasGroupFade.Kill();
        }

        pauseMenuAndPlayUIAnimation = DOTween.Sequence();

        if (exitingPlayModeFromPause)
        {
            pauseMenuAndPlayUIAnimation
                .Append(pauseMenuCanvasGroup.DOFade(0f, 0.25f))
                .Join(AnimateSkyBlur(movingIntoFocus: true))
                .OnComplete(() => pauseMenu.SetActive(false))
                .OnComplete(() => menuBlur.active = false);
        }
        else if (fadeInPauseMenu)
        {
            pauseMenuAndPlayUIAnimation
                .Append(    playCanvasGroup.DOFade      (0f, 0.1f))
                .Append(    pauseMenuCanvasGroup.DOFade (1f, 0.4f))//.SetEase(Ease.OutExpo))
                .Join(      AnimateSkyBlur              (movingIntoFocus: false))
                .OnStart(   () => pauseMenu.SetActive(true))
                .OnComplete(() => playCanvas.SetActive(false));
        }
        else
        {
            pauseMenuAndPlayUIAnimation
                .Append(pauseMenuCanvasGroup.DOFade(0f, 0.25f))
                .Append(playCanvasGroup.DOFade(1f, 0.25f))
                .Insert(0f, AnimateSkyBlur(movingIntoFocus: true))
                .OnStart(() => playCanvas.SetActive(true))
                .OnComplete(() => pauseMenu.SetActive(false));
        }
    }

    private Sequence AnimateSkyBlur(bool movingIntoFocus)
    {
        Sequence animation = DOTween.Sequence();
        animation.Append(DOTween.To(() => menuBlur.focusDistance.value, x => menuBlur.focusDistance.value = x, movingIntoFocus ? 5f : 0.1f, 0.4f));
        animation.InsertCallback(movingIntoFocus ? animation.Duration() : 0f, () => menuBlur.active = !movingIntoFocus);
        return animation;
    }
}
