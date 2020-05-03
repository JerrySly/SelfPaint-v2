using ColorPickerWPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SelfPaint_v2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static bool _paintMode;
        private static bool _editMode;
        private static bool _penMode;
        private static Shape _selectedShape;
        private static bool _ctrlPressing;
        private static Shape _copyShape;
        private Point _currentPointMouse;
        private Point _centerOfShape;
        private Color _colorShape;
        private Color _colorStroke;
        public MainWindow()
        {
            
            InitializeComponent();
            _paintMode = false;
            _editMode = false;
            Holst.Background = Brushes.Gray;

        }

       private Shape SetShape()
        {
            if (EllipseBtn.IsChecked.HasValue && EllipseBtn.IsChecked.Value) return new Ellipse();
            if (RectangleBtn.IsChecked.HasValue && RectangleBtn.IsChecked.Value) return new Rectangle();
            return new Line();
        }
        private void SetStroke()
        {
            if (CommonStroke.IsChecked.HasValue && CommonStroke.IsChecked.Value) _selectedShape.StrokeDashArray = new DoubleCollection() { 1, 0 };
            if (DashStroke.IsChecked.HasValue && DashStroke.IsChecked.Value) _selectedShape.StrokeDashArray = new DoubleCollection() { 2, 2 };
        }
        
        private void SelectMod()
        {
            if (Mods.SelectedIndex == 0) 
            {
                EllipseBtn.IsEnabled = true;
                EllipseBtn.IsChecked = true; 
                RectangleBtn.IsEnabled = true;
                LineBtn.IsEnabled = true;
                _paintMode = true;
                _editMode = false;
                _penMode = false;
            }
            else if(Mods.SelectedIndex == 1)
            {
                EllipseBtn.IsChecked = false;
                RectangleBtn.IsChecked = false;
                LineBtn.IsChecked = false;
                EllipseBtn.IsEnabled = false;
                RectangleBtn.IsEnabled = false;
                LineBtn.IsEnabled = false;
                _editMode = true;
                _paintMode = false;
                _penMode = false;
            }
            else
            {
                _paintMode = false;
                _editMode = false;
                EllipseBtn.IsChecked = false;
                RectangleBtn.IsChecked = false;
                LineBtn.IsChecked = false;
                EllipseBtn.IsEnabled = false;
                RectangleBtn.IsEnabled = false;
                LineBtn.IsEnabled = false;
                _penMode = true;
            }
        }

        private void Mods_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectMod();
        }

        private void Holst_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source != _selectedShape)
            {
                _selectedShape = e.Source as Shape;
                _ctrlPressing = false;
            }
            if (_editMode && _selectedShape is Shape)
            {

                if (_ctrlPressing)
                {
                    _centerOfShape = new Point(Canvas.GetLeft(_selectedShape) + _selectedShape.Width / 2, Canvas.GetTop(_selectedShape) + _selectedShape.Height / 2);
                    _currentPointMouse = e.GetPosition(Holst);
                    Holst.MouseMove += Holst_MouseMoveEditMode;
                    Holst.MouseLeftButtonUp += Holst_MouseLeftButtonUpEditMode;
                }
                else
                {
                    Holst.MouseMove += Holst_MouseMoveDragMode;
                    Holst.MouseLeftButtonUp += Holst_MouseLeftButtonUpDragMode;
                }
                
            }
            if (_paintMode)
            {
                _selectedShape = SetShape();
                PaintTempShape(e);
                _currentPointMouse = e.GetPosition(Holst);
                if(!(_selectedShape is Line))
                Holst.Children.Add(_selectedShape);
                Holst.MouseMove += Holst_MouseMovePaintMode;
                Holst.MouseLeftButtonUp += Holst_MouseLeftButtonUpPaintMode;
               
            }
            if (_penMode)
            {
                _currentPointMouse = e.GetPosition(Holst);
                Holst.MouseMove += Holst_MouseMovePenMode;
                Holst.MouseLeftButtonUp += Holst_MouseLeftButtonUpPenMode;
            }

        }
        private void Holst_MouseMovePenMode(object sender, MouseEventArgs e)
        {
            Line line = new Line();
            line.X1 = _currentPointMouse.X;
            line.Y1 = _currentPointMouse.Y;
            line.X2 = e.GetPosition(Holst).X;
            line.Y2 = e.GetPosition(Holst).Y;
            _selectedShape = line;
            SetColorStroke();
            line.StrokeThickness = StrokeValue.Value;
            Holst.Children.Add(_selectedShape);
            _currentPointMouse = e.GetPosition(Holst);
        }
        private void Holst_MouseLeftButtonUpPenMode(object sender, MouseButtonEventArgs e)
        {
            Holst.MouseMove -= Holst_MouseMovePenMode;
            Holst.MouseLeftButtonUp -= Holst_MouseLeftButtonUpPenMode;
        }
        private void PaintTempShape(MouseButtonEventArgs e)
        {
            _selectedShape.StrokeDashArray = new DoubleCollection() { 2, 2 };
            _selectedShape.Stroke = Brushes.Black;
            _selectedShape.StrokeThickness = 2;
            
            if(_selectedShape is Line)
            {
                Line line = _selectedShape as Line;
                line.StrokeThickness = 2;
                line.X1 = Mouse.GetPosition(Holst).X;
                line.Y1 = Mouse.GetPosition(Holst).Y;
            }
            else
            {
                _selectedShape.Width = 20;
                _selectedShape.Height = 20;
                Canvas.SetTop(_selectedShape, e.GetPosition(Holst).Y - _selectedShape.Height / 2);
                Canvas.SetLeft(_selectedShape, e.GetPosition(Holst).X - _selectedShape.Width / 2);
            }
        }

       

        private void Holst_MouseMoveDragMode(object sender, MouseEventArgs e)
        {
           
            
                Canvas.SetTop(_selectedShape, e.GetPosition(Holst).Y - _selectedShape.Height / 2);
                Canvas.SetLeft(_selectedShape, e.GetPosition(Holst).X - _selectedShape.Width / 2);
            
        }
        private void Holst_MouseMoveEditMode(object sender, MouseEventArgs e)
        {

            
            if (e.GetPosition(Holst).X > _centerOfShape.X)
            {
                _selectedShape.Width = _selectedShape.Width + (e.GetPosition(Holst).X - _currentPointMouse.X);
            }
            else
            {
                _selectedShape.Width = _selectedShape.Width - (_currentPointMouse.X - e.GetPosition(Holst).X);
            }
            if (e.GetPosition(Holst).Y > _centerOfShape.Y)
            {
                _selectedShape.Height = _selectedShape.Height + (e.GetPosition(Holst).Y - _currentPointMouse.Y);
            }
            else
            {
                _selectedShape.Height = _selectedShape.Height - (_currentPointMouse.Y - e.GetPosition(Holst).Y);
            }
            _currentPointMouse = e.GetPosition(Holst);

        }
        private void Holst_MouseLeftButtonUpEditMode(object sender, MouseButtonEventArgs e)
        {

            Holst.MouseMove -= Holst_MouseMoveEditMode;
            Holst.MouseLeftButtonUp -= Holst_MouseLeftButtonUpEditMode;
        }
        private void Holst_MouseMovePaintMode(object sender, MouseEventArgs e)
        {
            if (_selectedShape is Line)
            {
             
            }
            else
            {

                _selectedShape.Width = _selectedShape.Width + (e.GetPosition(Holst).X - _currentPointMouse.X);
                _selectedShape.Height = _selectedShape.Height + (e.GetPosition(Holst).Y - _currentPointMouse.Y);
            }
           
            _currentPointMouse = e.GetPosition(Holst);


        }
        private void Holst_MouseLeftButtonUpDragMode(object sender, MouseButtonEventArgs e)
        {
           
            Holst.MouseMove -= Holst_MouseMoveDragMode;
            Holst.MouseLeftButtonUp -= Holst_MouseLeftButtonUpDragMode;
        }
        private void Holst_MouseLeftButtonUpPaintMode(object sender, MouseButtonEventArgs e)
        {
            if(_selectedShape is Line)
            {
                Line line = _selectedShape as Line;
                line.X2 = Mouse.GetPosition(Holst).X;
                line.Y2 = Mouse.GetPosition(Holst).Y;
                Holst.Children.Add(_selectedShape);
            }
            SetStroke();
            SetColorShape();
            SetColorStroke();
            
            Holst.MouseMove -= Holst_MouseMovePaintMode;
            Holst.MouseLeftButtonUp -= Holst_MouseLeftButtonUpPaintMode;
        }

        private void Holst_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl) _ctrlPressing = true;
        }

        private void ColorShape_Click(object sender, RoutedEventArgs e)
        {
            bool cp = ColorPickerWindow.ShowDialog(out _colorShape);
            SetColorShape();
        }

        private void ColorStroke_Click(object sender, RoutedEventArgs e)
        {
            bool cp = ColorPickerWindow.ShowDialog(out _colorStroke);
            SetColorStroke();
        }
        private void SetColorShape()
        {

            Brush brush = new SolidColorBrush(_colorShape);
            if (_selectedShape is Shape)
                _selectedShape.Fill = brush;
            
        }
        private void SetColorStroke()
        {

            Brush brush = new SolidColorBrush(_colorStroke);
            if (_selectedShape is Shape)
                _selectedShape.Stroke = brush;

        }

        private void StrokeValue_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(_selectedShape is Shape)
            _selectedShape.StrokeThickness = StrokeValue.Value;
        }
        public static T XamlClone<T>(T source)
        {
            string savedObject = System.Windows.Markup.XamlWriter.Save(source);
            StringReader stringReader = new StringReader(savedObject);
            System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
            T target = (T)System.Windows.Markup.XamlReader.Load(xmlReader);
            return target;
        }
        private void SaveXamlCode(UIElementCollection elementCollection)
        {
            List<string> savedObjects = new List<string>();
            foreach (var el in elementCollection)
            {
                savedObjects.Add(System.Windows.Markup.XamlWriter.Save(el));
            }
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Текстовый документ (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == saveFileDialog.ValidateNames)
            {
                StreamWriter streamWriter = new StreamWriter(saveFileDialog.FileName);
                foreach (string str in savedObjects)
                    streamWriter.WriteLine(str);
                streamWriter.Close();
            }
        }
        private void LoadXamlCode()
        {
            Holst.Children.Clear();

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Текстовый документ (*.txt)|*.txt|Все файлы (*.*)|*.*";
            if (openFileDialog.ShowDialog() == openFileDialog.CheckFileExists)
            {
                List<string> text = new List<string>(File.ReadAllLines(openFileDialog.FileName));
                foreach (string line in text)
                {
                    Holst.Children.Add(System.Windows.Markup.XamlReader.Parse(line) as Shape);
                }

            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            _copyShape = _selectedShape;
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            Shape _newShape = XamlClone(_copyShape);
            Holst.Children.Add(_newShape);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            Holst.Children.Remove(_selectedShape);
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveXamlCode(Holst.Children);
        }

        private void LoadBtn_Click(object sender, RoutedEventArgs e)
        {
            LoadXamlCode();
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            Holst.Children.Clear();
        }
    }
}
