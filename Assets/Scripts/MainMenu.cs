using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameManager game = null;
    [SerializeField]
    private PostProcessVolume postProcessing = null;

    private DepthOfField menuBlur = null;


    [SerializeField]
    private CanvasGroup tutorialCanvasGroup = null;
    [SerializeField]
    private CanvasGroup playCanvasGroup = null;
    [SerializeField]
    private CanvasGroup optionsCanvasGroup = null;
    [SerializeField]
    private CanvasGroup creditsCanvasGroup = null;
    
    [SerializeField]
    private RectTransform tutorialRect = null;
    [SerializeField]
    private RectTransform playRect = null;
    [SerializeField]
    private RectTransform optionsRect = null;
    [SerializeField]
    private RectTransform creditsRect = null;

    void Start()
    {
        if (HasPlayed())
        {
            tutorialRect.gameObject.SetActive(false);
        }

        postProcessing.profile.TryGetSettings(out menuBlur);
    }


    public void PlayTutorial()
    {
        if (game.state != GameState.Menu) return;

        IncrementPlayCount();
        game.StartTutorial();
    }

    public void Play()
    {
        if (game.state != GameState.Menu) return;

        Sequence menuToSkyAnimation = AnimateMenuToSky();
        if (!HasPlayed())
        {
            // Tell player tutorial can be found in the options menu
            ShowTutorialDialog();
            // Disable the Tutorial button once menu is done fading out.
            menuToSkyAnimation.AppendCallback(() => tutorialRect.gameObject.SetActive(false));
        }

        IncrementPlayCount();
        game.GoToSky();
    }

    public void OpenOptions()
    {
        AnimateMenuButtons();
    }

    public void ShowCredits()
    {

    }

    public void OpenMainMenu()
    {

    }

    private void ShowTutorialDialog()
    {

    }

    private void AcceptTutorialDialog()
    {

    }
    private void CancelTutorialDialog()
    {

    }

    private Sequence AnimateMenuToSky()
    {
        return DOTween.Sequence().Append(DOTween.To(() => menuBlur.focusDistance.value, x => menuBlur.focusDistance.value = x, 5f, 2f));
    }

    private void IncrementPlayCount()
    {
        PlayerPrefs.SetInt("Play Count", PlayerPrefs.HasKey("Play Count") ? PlayerPrefs.GetInt("Play Count") : 1);
    }

    private bool HasPlayed()
    {
        return PlayerPrefs.HasKey("Play Count") && PlayerPrefs.GetInt("Play Count") > 1;
    }

    private Sequence AnimateMenuButtons(bool appearing = false)
    {
        Vector2 verticalMovement = 20f * (appearing ? Vector2.up : Vector2.down);
        float alpha = appearing ? 1f : 0f;
        float delay = appearing ? -0.05f : 0.05f;
        float offset = appearing ? 3f * delay : 0f;

        Sequence animation = DOTween.Sequence()
            .Insert(0f * delay + offset, creditsCanvasGroup  .DOFade      (alpha,                 0.2f))
            .Insert(0f * delay + offset, creditsRect         .DOAnchorPos (verticalMovement,      0.2f).SetEase(Ease.InQuad).SetRelative())
            .Insert(1f * delay + offset, optionsCanvasGroup  .DOFade      (alpha,                 0.2f))
            .Insert(1f * delay + offset, optionsRect         .DOAnchorPos (verticalMovement,      0.2f).SetEase(Ease.InQuad).SetRelative())
            .Insert(2f * delay + offset, playCanvasGroup     .DOFade      (alpha,                 0.2f))
            .Insert(2f * delay + offset, playRect            .DOAnchorPos (verticalMovement,      0.2f).SetEase(Ease.InQuad).SetRelative())
            .Insert(3f * delay + offset, tutorialCanvasGroup .DOFade      (alpha,                 0.2f))
            .Insert(3f * delay + offset, tutorialRect        .DOAnchorPos (verticalMovement,      0.2f).SetEase(Ease.InQuad).SetRelative());

        return animation;
    }
}
