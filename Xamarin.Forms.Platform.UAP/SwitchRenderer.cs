﻿using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

namespace Xamarin.Forms.Platform.UWP
{
	public class SwitchRenderer : ViewRenderer<Switch, ToggleSwitch>
	{
		Brush _originalOnHoverColor;
		Brush _originalOnColorBrush;
		Brush _originalThumbOnBrush;

		protected override void OnElementChanged(ElementChangedEventArgs<Switch> e)
		{
			base.OnElementChanged(e);
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var control = new ToggleSwitch();
					control.Toggled += OnNativeToggled;
					control.Loaded += OnControlLoaded;
					control.ClearValue(ToggleSwitch.OnContentProperty);
					control.ClearValue(ToggleSwitch.OffContentProperty);

					SetNativeControl(control);
				}

				Control.IsOn = Element.IsToggled;

				UpdateFlowDirection();
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Switch.IsToggledProperty.PropertyName)
			{
				Control.IsOn = Element.IsToggled;
			}
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
			{
				UpdateFlowDirection();
			}
			else if (e.PropertyName == Switch.OnColorProperty.PropertyName)
				UpdateOnColor();
			else if (e.PropertyName == Switch.ThumbColorProperty.PropertyName)
				UpdateThumbColor();
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnControlLoaded(object sender, RoutedEventArgs e)
		{
			UpdateOnColor();
			UpdateThumbColor();
			Control.Loaded -= OnControlLoaded;
		}

		void OnNativeToggled(object sender, RoutedEventArgs routedEventArgs)
		{
			((IElementController)Element).SetValueFromRenderer(Switch.IsToggledProperty, Control.IsOn);
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}

		void UpdateOnColor()
		{
			if (Control == null)
				return;

			var grid = Control.GetFirstDescendant<Windows.UI.Xaml.Controls.Grid>();
			if (grid == null)
				return;

			var groups = Windows.UI.Xaml.VisualStateManager.GetVisualStateGroups(grid);
			foreach (var group in groups)
			{
				if (group.Name != "CommonStates")
					continue;

				foreach (var state in group.States)
				{
					if (state.Name != "PointerOver")
						continue;

					foreach (var timeline in state.Storyboard.Children.OfType<ObjectAnimationUsingKeyFrames>())
					{
						var property = Windows.UI.Xaml.Media.Animation.Storyboard.GetTargetProperty(timeline);
						var target = Windows.UI.Xaml.Media.Animation.Storyboard.GetTargetName(timeline);
						if (target == "SwitchKnobBounds" && property == "Fill")
						{
							var frame = timeline.KeyFrames.First();

							if (_originalOnHoverColor == null)
								_originalOnHoverColor = (Brush)frame.Value;

							if (!Element.OnColor.IsDefault)
								frame.Value = new SolidColorBrush(Element.OnColor.ToWindowsColor()) { Opacity = _originalOnHoverColor.Opacity };
							else
								frame.Value = _originalOnHoverColor;
							break;
						}
					}
				}
			}

			var rect = Control.GetDescendantsByName<Windows.UI.Xaml.Shapes.Rectangle>("SwitchKnobBounds").First();

			if (_originalOnColorBrush == null)
				_originalOnColorBrush = rect.Fill;

			if (!Element.OnColor.IsDefault)
				rect.Fill = new SolidColorBrush(Element.OnColor.ToWindowsColor());
			else
				rect.Fill = _originalOnColorBrush;
		}

		void UpdateThumbColor()
		{
			if (Control == null)
				return;

			var grid = Control.GetFirstDescendant<Windows.UI.Xaml.Controls.Grid>();
			if (grid == null)
				return;

			ObjectKeyFrame frame = Windows.UI.Xaml.VisualStateManager.GetVisualStateGroups(grid)
				.First(g => g.Name == "CommonStates")
				.States.First(s => s.Name == "PointerOver")
				.Storyboard.Children.OfType<ObjectAnimationUsingKeyFrames>().First(
					t => Windows.UI.Xaml.Media.Animation.Storyboard.GetTargetName(t) == "SwitchKnobOn" &&
						 Windows.UI.Xaml.Media.Animation.Storyboard.GetTargetProperty(t) == "Fill")
				.KeyFrames.First();

			if (_originalThumbOnBrush == null)
				_originalThumbOnBrush = (Brush)frame.Value;

			if (!Element.ThumbColor.IsDefault)
				frame.Value = new SolidColorBrush(Element.ThumbColor.ToWindowsColor())
				{
					Opacity = _originalThumbOnBrush.Opacity
				};
			else
				frame.Value = _originalThumbOnBrush;

			var thumb = (Ellipse)grid.FindName("SwitchKnobOn");
			if (_originalThumbOnBrush == null)
				_originalThumbOnBrush = thumb.Fill;

			if (!Element.ThumbColor.IsDefault)
				thumb.Fill = new SolidColorBrush(Element.ThumbColor.ToWindowsColor());
			else
				thumb.Fill = _originalThumbOnBrush;
		}
	}
}
