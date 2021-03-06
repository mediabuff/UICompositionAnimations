﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Media;
using JetBrains.Annotations;
using UICompositionAnimations.Behaviours;
using UICompositionAnimations.Brushes.Base;
using UICompositionAnimations.Brushes.Effects;
using UICompositionAnimations.Brushes.Effects.Interfaces;
using LuminanceToAlphaEffect = Microsoft.Graphics.Canvas.Effects.LuminanceToAlphaEffect;

namespace UICompositionAnimations.Brushes
{
    /// <summary>
    /// A <see cref="Brush"/> that renders a customizable Composition/Win2D effects pipeline
    /// </summary>
    public sealed class PipelineBrush : XamlCompositionEffectBrushBase
    {
        /// <inheritdoc/>
        protected override PipelineBuilder OnBrushRequested()
        {
            // Starts a new composition pipeline from the given effect
            PipelineBuilder Start(IPipelineEffect effect)
            {
                switch (effect)
                {
                    case BackdropEffect backdrop when backdrop.Source == AcrylicBackgroundSource.Backdrop:
                        return PipelineBuilder.FromBackdropBrush();
                    case BackdropEffect backdrop when backdrop.Source == AcrylicBackgroundSource.HostBackdrop:
                        return PipelineBuilder.FromHostBackdropBrush();
                    case SolidColorEffect color: return PipelineBuilder.FromColor(color.Color);
                    case ImageEffect image: return PipelineBuilder.FromImage(image.Uri, image.DPIMode, image.CacheMode);
                    case TileEffect tile: return PipelineBuilder.FromTiles(tile.Uri, tile.DPIMode, tile.CacheMode);
                    case AcrylicEffect acrylic:
                        switch (acrylic.Source)
                        {
                            case AcrylicBackgroundSource.Backdrop: return PipelineBuilder.FromBackdropAcrylic(acrylic.Tint, (float)acrylic.TintMix, (float)acrylic.BlurAmount, acrylic.TextureUri);
                            case AcrylicBackgroundSource.HostBackdrop: return PipelineBuilder.FromHostBackdropAcrylic(acrylic.Tint, (float)acrylic.TintMix, acrylic.TextureUri);
                            default: throw new ArgumentOutOfRangeException(nameof(acrylic.Source), $"Invalid acrylic source: {acrylic.Source}");
                        }
                    default: throw new ArgumentException($"Invalid initial pipeline effect: {effect.GetType()}");
                }
            }

            // Appends an effect to an existing composition pipeline
            PipelineBuilder Append(IPipelineEffect effect, PipelineBuilder builder)
            {
                switch (effect)
                {
                    case OpacityEffect opacity: return builder.Opacity((float)opacity.Value);
                    case LuminanceEffect _: return builder.Effect(source => new LuminanceToAlphaEffect { Source = source });
                    case TintEffect tint: return builder.Tint(tint.Color, (float)tint.Opacity);
                    case BlurEffect blur: return builder.Blur((float)blur.Value);
                    case SaturationEffect saturation: return builder.Saturation((float)saturation.Value);
                    case BlendEffect blend: return builder.Blend(Build(blend.Input), blend.Mode, blend.Placement);
                    default: throw new ArgumentException($"Invalid pipeline effect: {effect.GetType()}");
                }
            }

            // Builds a new effects pipeline from the input effects sequence
            PipelineBuilder Build(IList<IPipelineEffect> effects)
            {
                if (effects.Count == 0) throw new ArgumentException("An effects pipeline can't be empty");
                return effects.Skip(1).Aggregate(Start(effects[0]), (b, e) => Append(e, b));
            }

            return Build(Effects);
        }

        /// <summary>
        /// Gets or sets the collection of effects to use in the current pipeline
        /// </summary>
        [NotNull, ItemNotNull]
        public IList<IPipelineEffect> Effects { get; set; } = new List<IPipelineEffect>();
    }
}
