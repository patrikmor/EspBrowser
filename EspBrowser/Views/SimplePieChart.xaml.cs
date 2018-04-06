using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EspBrowser.Views
{
  /// <summary>
  /// Interaction logic for SimplePieChart.xaml
  /// </summary>
  [TemplatePart(Name = "PART_PieChart", Type = typeof(SimplePieChart))]
  public partial class SimplePieChart
  {
    private Image _pieChartImage;

    #region Dependency properties
    public static readonly DependencyProperty PercentageProperty =
       DependencyProperty.Register("Percentage", typeof(double), typeof(SimplePieChart), new FrameworkPropertyMetadata(0.5, OnPiePropertyChanged, CoercePercentageCallback));

    public static readonly DependencyProperty SizeProperty =
      DependencyProperty.Register("Size", typeof(double), typeof(SimplePieChart), new PropertyMetadata(100.0, OnPiePropertyChanged));

    public static readonly DependencyProperty InnerPieSliceFillProperty =
       DependencyProperty.Register("InnerPieSliceFill", typeof(Brush), typeof(SimplePieChart), new PropertyMetadata(CreateBrush("#26A0DA"), OnPiePropertyChanged));

    public static readonly DependencyProperty OuterPieSliceFillProperty =
       DependencyProperty.Register("OuterPieSliceFill", typeof(Brush), typeof(SimplePieChart), new PropertyMetadata(CreateBrush("#ACACAC"), OnPiePropertyChanged));

    public double Percentage
    {
      get { return (double)GetValue(PercentageProperty); }
      set { SetValue(PercentageProperty, value); }
    }

    public double Size
    {
      get { return (double)GetValue(SizeProperty); }
      set { SetValue(SizeProperty, value); }
    }

    public Brush InnerPieSliceFill
    {
      get { return (Brush)GetValue(InnerPieSliceFillProperty); }
      set { SetValue(InnerPieSliceFillProperty, value); }
    }

    public Brush OuterPieSliceFill
    {
      get { return (Brush)GetValue(OuterPieSliceFillProperty); }
      set { SetValue(OuterPieSliceFillProperty, value); }
    }
    #endregion

    #region Constructor
    public SimplePieChart()
    {
      InitializeComponent();
    }
    #endregion

    #region Overrides
    public override void OnApplyTemplate()
    {
      _pieChartImage = (Image)Template.FindName("PART_PieChart", this);
      CreatePieChart();
    }
    #endregion

    #region Events
    private static object CoercePercentageCallback(DependencyObject dep, object baseValue)
    {
      var value = (double)baseValue;
      if(value < 0.0)
        value = 0.0;
      else if(value > 1.0)
        value = 1.0;

      return value;
    }

    private static void OnPiePropertyChanged(DependencyObject dep, DependencyPropertyChangedEventArgs ev)
    {
      var chart = (SimplePieChart)dep;

      if(chart.IsInitialized)
      {
        chart.CreatePieChart();
      }
    }
    #endregion

    #region Private methods
    private void CreatePieChart()
    {
      if(_pieChartImage != null)
      {
        if(!double.IsNaN(Size) && !double.IsNaN(Percentage))
        {
          _pieChartImage.Width = _pieChartImage.Height = Width = Height = Size;

          var di = new DrawingImage();
          _pieChartImage.Source = di;

          var dg = new DrawingGroup();
          di.Drawing = dg;

          if(Percentage > 0.0 && Percentage < 1.0)
          {
            var angle = 360 * Percentage;
            var radians = (Math.PI / 180) * angle;
            var endPointX = Math.Sin(radians) * Height / 2 + Height / 2;
            var endPointY = Width / 2 - Math.Cos(radians) * Width / 2;
            var endPoint = new Point(endPointX, endPointY);

            dg.Children.Add(CreatePathGeometry(InnerPieSliceFill, new Point(Width / 2, 0), endPoint, Percentage > 0.5));
            dg.Children.Add(CreatePathGeometry(OuterPieSliceFill, endPoint, new Point(Width / 2, 0), Percentage <= 0.5));
          }
          else
          {
            dg.Children.Add(CreateEllipseGeometry(Math.Abs(Percentage - 0.0) < 0.0001 ? OuterPieSliceFill : InnerPieSliceFill));
          }
        }
        else
        {
          _pieChartImage.Source = null;
        }
      }
    }

    private GeometryDrawing CreatePathGeometry(Brush brush, Point startPoint, Point arcPoint, bool isLargeArc)
    {
      /*
			 * <GeometryDrawing Brush="@Brush">
				  <GeometryDrawing.Geometry>
					 <PathGeometry>
						<PathFigure StartPoint="@Size/2">
						   <PathFigure.Segments>
							  <LineSegment Point="@startPoint"/>
							  <ArcSegment Point="@arcPoint"  SweepDirection="Clockwise" Size="@Size/2"/>
							  <LineSegment Point="@Size/2"/>
						   </PathFigure.Segments>
						</PathFigure>
					 </PathGeometry>
				  </GeometryDrawing.Geometry>
			   </GeometryDrawing>
			 * */

      var midPoint = new Point(Width / 2, Height / 2);

      var drawing = new GeometryDrawing { Brush = brush };
      var pathGeometry = new PathGeometry();
      var pathFigure = new PathFigure { StartPoint = midPoint };

      var ls1 = new LineSegment(startPoint, false);
      var arc = new ArcSegment
      {
        SweepDirection = SweepDirection.Clockwise,
        Size = new Size(Width / 2, Height / 2),
        Point = arcPoint,
        IsLargeArc = isLargeArc
      };
      var ls2 = new LineSegment(midPoint, false);

      drawing.Geometry = pathGeometry;
      pathGeometry.Figures.Add(pathFigure);

      pathFigure.Segments.Add(ls1);
      pathFigure.Segments.Add(arc);
      pathFigure.Segments.Add(ls2);

      return drawing;
    }

    private GeometryDrawing CreateEllipseGeometry(Brush brush)
    {
      var midPoint = new Point(Width / 2, Height / 2);

      var drawing = new GeometryDrawing { Brush = brush };
      var ellipse = new EllipseGeometry(midPoint, Size / 2, Size / 2);

      drawing.Geometry = ellipse;

      return drawing;
    }

    private static SolidColorBrush CreateBrush(string brush)
    {
      var color = ColorConverter.ConvertFromString(brush);
      if(color != null)
        return new SolidColorBrush((Color)color);

      return null;
    }
    #endregion
  }
}
