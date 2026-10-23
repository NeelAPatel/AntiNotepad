using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;

namespace AntiQuickPad.Carousel;

/// <summary>Stable caller-facing composition of motion policy and token-driven presentation.</summary>
internal sealed class CarouselController
{
    public const double SlideMs = CarouselMotion.SlideMs;
    private readonly CarouselMotion _motion;
    private readonly CarouselView _view;
    internal event Action? MotionChanged { add => _motion.MotionChanged += value; remove => _motion.MotionChanged -= value; }
    internal readonly record struct FrameSample(double Ms, double Offset);
    internal List<FrameSample> Frames => _motion.Frames;
    internal double Offset => _motion.Offset;
    public Control View => _view.View;
    public bool IsBusy => _motion.IsBusy;

    public CarouselController(TextEditor editor, Look look, ViewModels.MainViewModel viewModel,
        Func<int, Vector> scroll, Action activity, Func<Control, Control>? decoratePage = null, Func<double, double>? pageStride = null)
    {
        _view = new CarouselView(editor, look, i => viewModel.Notes[i].Text, scroll, decoratePage, pageStride);
        _motion = new CarouselMotion(_view, () => viewModel.CurrentIndex, () => viewModel.Notes.Count, viewModel.Select, activity);
        _view.Attach(_motion);
    }
    public void RequestWheel(double delta, long now) => _motion.RequestWheel(delta, now);
    public void BeginDrag(double x, double now) => _motion.BeginDrag(x, now);
    public void MoveDrag(double x, double now) => _motion.MoveDrag(x, now);
    public void EndDrag(double now) => _motion.EndDrag(now);
    public void Cancel() => _motion.Cancel();
    public static int SnapIndex(int index, double offset, double velocity, double width, int count) => CarouselMotion.SnapIndex(index, offset, velocity, width, count);
    internal static double EasedOffset(double from, double to, double progress, bool settling) => CarouselMotion.EasedOffset(from, to, progress, settling);
    public static void RunSmoke() => CarouselMotion.RunSmoke();
}
