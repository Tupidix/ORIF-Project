using UnityEngine;

/// <summary>Positions floating interfaces and presents step/timer/end-of-game feedback.</summary>
public sealed class InterfacePresentationController
{
    private readonly MenuController _menus;
    private readonly GameObject _pause;
    private readonly GameObject _steps;
    private readonly GameObject _timer;
    private readonly GameObject _celebration;
    private readonly GameObject _celebrationTip;

    public InterfacePresentationController(MenuController menus, GameObject pause, GameObject steps,
        GameObject timer, GameObject celebration, GameObject celebrationTip)
    {
        _menus = menus;
        _pause = pause;
        _steps = steps;
        _timer = timer;
        _celebration = celebration;
        _celebrationTip = celebrationTip;
    }

    public void ShowSteps(bool visible) => _steps.SetActive(visible);
    public void ShowTimer(bool visible) => _timer.SetActive(visible);
    public void ShowCelebration(bool visible) => _celebration.SetActive(visible);
    public void ShowCelebrationTip(bool visible) => _celebrationTip.SetActive(visible);

    public void UpdatePose(Camera camera, GameObject notification)
    {
        _menus.Position(camera, 0.55f);
        Helper.PlaceUIElement(_pause, camera, 0.55f, 1.5f);
        Helper.PlaceUIElement(notification, camera, 2.5f, 1.8f);
        _menus.FacePlayer();
        Helper.RotateUIElement(_steps, true);
        Helper.RotateUIElement(notification, true);
        Helper.RotateUIElement(_timer, true, true);
        Helper.RotateUIElement(_pause, true, false, "Pause");
        Helper.RotateUIElement(_celebration, true);
        Helper.RotateUIElement(_celebrationTip, true);
    }
}
