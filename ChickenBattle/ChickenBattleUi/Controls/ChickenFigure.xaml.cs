using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChickenBattleUi.Controls
{
    /// <summary>
    /// Interaction logic for ChickenFigure.xaml
    /// </summary>
    public partial class ChickenFigure : UserControl
    {
        public Brush ChickenBackground
        {
            get { return (Brush)GetValue(ChickenBackgroundProperty); }
            set { SetValue(ChickenBackgroundProperty, value); }
        }
        public static readonly DependencyProperty ChickenBackgroundProperty =
            DependencyProperty.Register("ChickenBackground", typeof(Brush), typeof(ChickenFigure), new FrameworkPropertyMetadata(Brushes.Red) {AffectsRender = true});

        public int Angle
        {
            get { return (int)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }
        public static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(int), typeof(ChickenFigure), new FrameworkPropertyMetadata(0) {AffectsMeasure = true, AffectsRender = true});

        public ChickenFigure()
        {
            InitializeComponent();
        }
    }
}
