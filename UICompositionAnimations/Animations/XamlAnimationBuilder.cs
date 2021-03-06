﻿using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using JetBrains.Annotations;
using UICompositionAnimations.Animations.Abstract;
using UICompositionAnimations.Animations.Interfaces;
using UICompositionAnimations.Enums;

namespace UICompositionAnimations.Animations
{
    /// <summary>
    /// A <see langword="class"/> that implements the <see cref="IAnimationBuilder"/> <see langword="interface"/> using composition APIs
    /// </summary>
    internal sealed class XamlAnimationBuilder : AnimationBuilderBase<Func<TimeSpan, Timeline>>
    {
        /// <summary>
        /// The target <see cref="CompositeTransform"/> to animate
        /// </summary>
        [NotNull]
        private readonly CompositeTransform TargetTransform;

        public XamlAnimationBuilder([NotNull] UIElement target) : base(target)
        {
            TargetTransform = target.GetTransform<CompositeTransform>(false);
            target.RenderTransformOrigin = new Point(0.5, 0.5);
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnOpacity(double? from, double to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetElement.CreateDoubleAnimation(nameof(UIElement.Opacity), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(Axis axis, double? from, double to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation($"Translate{axis}", from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnTranslation(Vector2? from, Vector2 to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.TranslateX), from?.X, to.X, duration, ease));
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.TranslateY), from?.Y, to.Y, duration, ease));

            return this;
        }

        protected override IAnimationBuilder OnOffset(Axis axis, double? from, double to, Easing ease) => throw new NotSupportedException("Can't animate the offset property from XAML");

        protected override IAnimationBuilder OnOffset(Vector2? from, Vector2 to, Easing ease) => throw new NotSupportedException("Can't animate the offset property from XAML");

        /// <inheritdoc/>
        protected override IAnimationBuilder OnScale(double? from, double to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.ScaleX), from, to, duration, ease));
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.ScaleY), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnRotate(double? from, double to, Easing ease)
        {
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(CompositeTransform.Rotation), from, to, duration, ease));

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnClip(Side side, double? from, double to, Easing ease) => throw new NotSupportedException("Can't animate the clip property from XAML");

        /// <inheritdoc/>
        protected override IAnimationBuilder OnSize(Axis axis, double? from, double to, Easing ease)
        {
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("Can't animate the size of an item that's not a framework element");
            AnimationFactories.Add(duration =>
            {
                switch (axis)
                {
                    case Axis.X: return TargetElement.CreateDoubleAnimation(nameof(FrameworkElement.Width), double.IsNaN(element.Width) ? element.ActualWidth : from, to, duration, ease, true);
                    case Axis.Y: return TargetElement.CreateDoubleAnimation(nameof(FrameworkElement.Height), double.IsNaN(element.Height) ? element.ActualHeight : from, to, duration, ease, true);
                    default: throw new ArgumentException($"Invalid axis: {axis}", nameof(axis));
                }
            });

            return this;
        }

        /// <inheritdoc/>
        protected override IAnimationBuilder OnSize(Vector2? from, Vector2 to, Easing ease = Easing.Linear)
        {
            if (!(TargetElement is FrameworkElement element)) throw new InvalidOperationException("Can't animate the size of an item that's not a framework element");
            double?
                fromX = double.IsNaN(element.Width) ? (double?)element.ActualWidth : from?.X,
                fromY = double.IsNaN(element.Height) ? (double?)element.ActualHeight : from?.Y;
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(FrameworkElement.Width), fromX, to.X, duration, ease, true));
            AnimationFactories.Add(duration => TargetTransform.CreateDoubleAnimation(nameof(FrameworkElement.Height), fromY, to.Y, duration, ease, true));

            return this;
        }

        /// <summary>
        /// Gets the <see cref="Windows.UI.Xaml.Media.Animation.Storyboard"/> represented by the current instance
        /// </summary>
        [NotNull]
        private Storyboard Storyboard => AnimationFactories.Select(f => f(DurationInterval)).ToStoryboard();

        /// <inheritdoc/>
        protected override void OnStart() => Storyboard.Begin();

        /// <inheritdoc/>
        protected override Task OnStartAsync() => Storyboard.BeginAsync();
    }
}
