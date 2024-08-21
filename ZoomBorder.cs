using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;


/*
 *
 * credit for this code goes to Wiesław Šoltés et al. (https://stackoverflow.com/a/6782715)
 * modified by myself
 *
 */


namespace TilesheetIndexGenerator
{
	public class ZoomBorder : Border
	{
		private UIElement? child;
		private Point origin;
		private Point start;

		private static TranslateTransform GetTranslateTransform(UIElement element)
		{
			return (TranslateTransform)((TransformGroup)element.RenderTransform)
			  .Children.First(tr => tr is TranslateTransform);
		}

		private static ScaleTransform GetScaleTransform(UIElement element)
		{
			return (ScaleTransform)((TransformGroup)element.RenderTransform)
			  .Children.First(tr => tr is ScaleTransform);
		}
		
		public override UIElement? Child
		{
			get => base.Child;
			set
			{
				if (value != null && value != Child)
					Initialize(value);
				base.Child = value;
			}
		}

		public void Initialize(UIElement? element)
		{
			child = element;

			if (child == null)
				return;

			TransformGroup group = new();
			ScaleTransform st = new();
			group.Children.Add(st);
			TranslateTransform tt = new();
			group.Children.Add(tt);
			child.RenderTransform = group;
			child.RenderTransformOrigin = new Point(0.0, 0.0);
			
			MouseWheel += child_MouseWheel;
			MouseLeftButtonDown += child_MouseLeftButtonDown;
			MouseLeftButtonUp += child_MouseLeftButtonUp;
			MouseMove += child_MouseMove;
			PreviewMouseRightButtonDown += child_PreviewMouseRightButtonDown;
		}

		public void Reset()
		{
			if (child == null)
				return;

			// reset zoom
			ScaleTransform st = GetScaleTransform(child);
			st.ScaleX = 1.0;
			st.ScaleY = 1.0;

			// reset pan
			TranslateTransform tt = GetTranslateTransform(child);
			tt.X = 0.0;
			tt.Y = 0.0;
		}

		#region Child Events

		private void child_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (child == null)
				return;

			ScaleTransform st = GetScaleTransform(child);
			TranslateTransform tt = GetTranslateTransform(child);

			double zoom = e.Delta > 0 ? .2 : -.2;
			if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
				return;

			Point relative = e.GetPosition(child);

			double absoluteX = relative.X * st.ScaleX + tt.X;
			double absoluteY = relative.Y * st.ScaleY + tt.Y;

			//double zoomCorrected = zoom * st.ScaleX;
			//st.ScaleX += zoomCorrected;
			//st.ScaleY += zoomCorrected;

			st.ScaleX += zoom;
			st.ScaleY += zoom;

			tt.X = absoluteX - relative.X * st.ScaleX;
			tt.Y = absoluteY - relative.Y * st.ScaleY;
		}

		private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (child == null)
				return;

			TranslateTransform tt = GetTranslateTransform(child);
			start = e.GetPosition(this);
			origin = new Point(tt.X, tt.Y);
			Cursor = Cursors.Hand;
			child.CaptureMouse();
		}

		private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (child == null)
				return;

			child.ReleaseMouseCapture();
			Cursor = Cursors.Arrow;
		}

		void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			Reset();
		}

		private void child_MouseMove(object sender, MouseEventArgs e)
		{
			if (child is not { IsMouseCaptured: true })
				return;

			TranslateTransform tt = GetTranslateTransform(child);
			Vector v = start - e.GetPosition(this);
			tt.X = origin.X - v.X;
			tt.Y = origin.Y - v.Y;
		}

		#endregion
	}
}