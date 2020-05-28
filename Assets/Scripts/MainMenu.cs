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
    private Bloom bloom = null;

    [SerializeField]
    private GameObject menuPanel = null; 

    [SerializeField]
    private CanvasGroup menuButton = null; 

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

    [SerializeField]
    private MouseSkyNavigation mouseSky = null;

    [SerializeField]
    private CanvasGroup optionsScreenCanvas = null;
    [SerializeField]
    private CanvasGroup creditsScreenCanvas = null;

    void Start()
    {
        if (HasPlayed())
        {
            tutorialRect.gameObject.SetActive(false);
        }

        postProcessing.profile.TryGetSettings(out menuBlur);
        postProcessing.profile.TryGetSettings(out bloom);
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

        Sequence menuToSkyAnimation = DOTween.Sequence()
            .Append(AnimateSkyBlur(movingIntoFocus: true))
            .Join(AnimateMenuButtons(appearing: false));

        if (!HasPlayed())
        {
            // Tell player tutorial can be found in the options menu
            ShowTutorialDialog();
            // Disable the Tutorial button once menu is done fading out.
            menuToSkyAnimation.AppendCallback(() => tutorialRect.gameObject.SetActive(false));
        }

        menuToSkyAnimation
            .Append(AnimateMenuButtonAlpha(fadeIn: true));
            

        IncrementPlayCount();
        game.GoToSky();
        mouseSky.takingInput = true;
    }

    public void OpenOptions()
    {
        DOTween.Sequence()
            .Append(AnimateMenuButtons(appearing: false))
            .Append(AnimateOptionsScreenCanvasAlpha(fadeIn: true));
    }
    public void ReturnFromOptions()
    {
        DOTween.Sequence()
            .Append(AnimateOptionsScreenCanvasAlpha(fadeIn: false))
            .Append(AnimateMenuButtons(appearing: true));
    }

    public void ShowCredits()
    {
        DOTween.Sequence()
            .Append(AnimateMenuButtons(appearing: false))
            .Append(AnimateCreditsScreenCanvasAlpha(fadeIn: true));
    }
    public void ReturnFromCredits()
    {
        DOTween.Sequence()
            .Append(AnimateCreditsScreenCanvasAlpha(fadeIn: false))
            .Append(AnimateMenuButtons(appearing: true));
    }

    public void OpenMainMenu()
    {
        if (game.state != GameState.Sky) return;

        game.state = GameState.Menu;
        mouseSky.takingInput = false;

        DOTween.Sequence()
            .Append(AnimateSkyBlur(movingIntoFocus: false))
            .Join(AnimateMenuButtons(appearing: true))
            .Join(AnimateMenuButtonAlpha(fadeIn: false));
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

    private void IncrementPlayCount()
    {
        PlayerPrefs.SetInt("Play Count", PlayerPrefs.HasKey("Play Count") ? PlayerPrefs.GetInt("Play Count") : 1);
    }

    private bool HasPlayed()
    {
        return PlayerPrefs.HasKey("Play Count") && PlayerPrefs.GetInt("Play Count") > 1;
    }

    #region Animation

    public Sequence AnimateOptionsScreenCanvasAlpha(bool fadeIn)
    {
        Sequence animation = DOTween.Sequence()
            .Append(optionsScreenCanvas.DOFade(fadeIn ? 1f : 0f, 0.17f).SetEase(fadeIn ? Ease.OutQuad : Ease.InQuad));

        animation.InsertCallback(fadeIn ? 0f : animation.Duration(), () => optionsScreenCanvas.gameObject.SetActive(fadeIn));

        return animation;
    }
    public Sequence AnimateCreditsScreenCanvasAlpha(bool fadeIn)
    {
        Sequence animation = DOTween.Sequence()
            .Append(creditsScreenCanvas.DOFade(fadeIn ? 1f : 0f, 0.17f).SetEase(fadeIn ? Ease.OutQuad : Ease.InQuad));

        animation.InsertCallback(fadeIn ? 0f : animation.Duration(), () => creditsScreenCanvas.gameObject.SetActive(fadeIn));

        return animation;
    }



    public Sequence AnimateMenuButtonAlpha(bool fadeIn)
    {
        Sequence animation = DOTween.Sequence()
            .Append(menuButton.DOFade(fadeIn ? 1f : 0f, 0.5f));

        animation.InsertCallback(fadeIn ? 0f : animation.Duration(), () => menuButton.gameObject.SetActive(fadeIn));
        
        return animation;
    }

    private Tween AnimateSkyBlur(bool movingIntoFocus)
    {
        return DOTween.To(() => menuBlur.focusDistance.value, x => menuBlur.focusDistance.value = x, movingIntoFocus ? 5f : 0.1f, 1f);
    }
    public void Bloom(bool turnOn)
    {
        bloom.intensity.value = turnOn ? 25f : 0f;
    }

    private Sequence AnimateMenuButtons(bool appearing)
    {
        Vector2 verticalMovement = 20f * (appearing ? Vector2.up : Vector2.down);
        float alpha = appearing ? 1f : 0f;
        float delay = appearing ? -0.05f : 0.05f;
        float offset = appearing ? -3f * delay : 0f;

        Ease ease = appearing ? Ease.OutQuad : Ease.InQuad;

        Sequence animation = DOTween.Sequence()
            .Insert(0f * delay + offset, creditsCanvasGroup  .DOFade      (alpha,                 0.2f))
            .Insert(0f * delay + offset, creditsRect         .DOAnchorPos (verticalMovement,      0.2f).SetEase(ease).SetRelative())
            .Insert(1f * delay + offset, optionsCanvasGroup  .DOFade      (alpha,                 0.2f))
            .Insert(1f * delay + offset, optionsRect         .DOAnchorPos (verticalMovement,      0.2f).SetEase(ease).SetRelative())
            .Insert(2f * delay + offset, playCanvasGroup     .DOFade      (alpha,                 0.2f))
            .Insert(2f * delay + offset, playRect            .DOAnchorPos (verticalMovement,      0.2f).SetEase(ease).SetRelative())
            .Insert(3f * delay + offset, tutorialCanvasGroup .DOFade      (alpha,                 0.2f))
            .Insert(3f * delay + offset, tutorialRect        .DOAnchorPos (verticalMovement,      0.2f).SetEase(ease).SetRelative());

        animation.InsertCallback(appearing ? 0f : animation.Duration(), () => menuPanel.SetActive(appearing));

        return animation;
    }

    #endregion

    public void SetMenuButtonActiveAndInteractable(bool value)
    {
        menuButton.gameObject.SetActive(value);
        menuButton.interactable = value;
    }
}
