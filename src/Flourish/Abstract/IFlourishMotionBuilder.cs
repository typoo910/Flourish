namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishMotionBuilder
{
    IFlourishMotionBuilder SetEnabled(bool enabled = true);

    IFlourishMotionBuilder SetDuration();

    IFlourishMotionBuilder SetDuration(TimeSpan duration);

    IFlourishMotionBuilder SetPageTransition(
        FlourishPageTransition transition = FlourishPageTransition.EntranceFromBottom
    );

    IFlourishMotionBuilder SetNavigationPanelTransition(
        FlourishNavigationPanelTransition transition = FlourishNavigationPanelTransition.Resize
    );

    IFlourishMotionBuilder SetHoverReveal(bool enabled = true);

    IFlourishMotionBuilder RespectSystemReducedMotion(bool enabled = true);
}
