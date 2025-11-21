using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ElementUI.WPF.Component
{
    public class Button : ButtonBase
    {
        #region 1. 构造器

        static Button()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Button), new FrameworkPropertyMetadata(typeof(Button)));
        }

        #endregion

        #region 2. 注册依赖属性

        public static readonly DependencyProperty ButtonTypeProperty = DependencyProperty.Register(nameof(ButtonType), typeof(ButtonType), typeof(Button), new PropertyMetadata(ButtonType.RightIconAndText));

        public static readonly DependencyProperty ButtonWidthProperty = DependencyProperty.Register(nameof(ButtonWidth), typeof(string), typeof(Button), new PropertyMetadata("100"));
        public static readonly DependencyProperty ButtonHeightProperty = DependencyProperty.Register(nameof(ButtonHeight), typeof(string), typeof(Button), new PropertyMetadata("45"));
        public static readonly DependencyProperty ButtonIsAutoSizeProperty = DependencyProperty.Register(nameof(ButtonIsAutoSize), typeof(bool), typeof(Button), new PropertyMetadata(false));
        public static readonly DependencyProperty ButtonIconAndTextSpaceProperty = DependencyProperty.Register(nameof(ButtonIconAndTextSpace), typeof(int), typeof(Button), new PropertyMetadata(10));

        public static readonly DependencyProperty ButtonBorderSizeProperty = DependencyProperty.Register(nameof(ButtonBorderSize), typeof(int), typeof(Button), new PropertyMetadata(5));
        public static readonly DependencyProperty ButtonBorderRadiusProperty = DependencyProperty.Register(nameof(ButtonBorderRadius), typeof(int), typeof(Button), new PropertyMetadata(5));
        public static readonly DependencyProperty ButtonBorderColorProperty = DependencyProperty.Register(nameof(ButtonBorderColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Black));
        public static readonly DependencyProperty MouseEnterButtonBorderColorProperty = DependencyProperty.Register(nameof(MouseEnterButtonBorderColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty MouseClickButtonBorderColorProperty = DependencyProperty.Register(nameof(MouseClickButtonBorderColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Green));

        public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register(nameof(ButtonText), typeof(string), typeof(Button), new PropertyMetadata("这是按钮"));
        public static readonly DependencyProperty ButtonTextSizeProperty = DependencyProperty.Register(nameof(ButtonTextSize), typeof(int), typeof(Button), new PropertyMetadata(14));
        public static readonly DependencyProperty ButtonTextWeightProperty = DependencyProperty.Register(nameof(ButtonTextWeight), typeof(FontWeight), typeof(Button), new PropertyMetadata(FontWeights.Normal));
        public static readonly DependencyProperty ButtonTextColorProperty = DependencyProperty.Register(nameof(ButtonTextColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Black));
        public static readonly DependencyProperty MouseEnterButtonTextColorProperty = DependencyProperty.Register(nameof(MouseEnterButtonTextColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty MouseClickButtonTextColorProperty = DependencyProperty.Register(nameof(MouseClickButtonTextColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Green));

        public static readonly DependencyProperty ButtonIconCodeProperty = DependencyProperty.Register(nameof(ButtonIconCode), typeof(string), typeof(Button), new PropertyMetadata("〇"));
        public static readonly DependencyProperty ButtonIconSizeProperty = DependencyProperty.Register(nameof(ButtonIconSize), typeof(int), typeof(Button), new PropertyMetadata(18));
        public static readonly DependencyProperty IconColorProperty = DependencyProperty.Register(nameof(IconColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Black));
        public static readonly DependencyProperty MouseEnterIconColorProperty = DependencyProperty.Register(nameof(MouseEnterIconColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty MouseClickIconColorProperty = DependencyProperty.Register(nameof(MouseClickIconColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Green));

        public static readonly DependencyProperty ButtonBackgroundColorProperty = DependencyProperty.Register(nameof(ButtonBackgroundColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Red));
        public static readonly DependencyProperty MouseEnterButtonBackgroundColorProperty = DependencyProperty.Register(nameof(MouseEnterButtonBackgroundColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.SkyBlue));
        public static readonly DependencyProperty MouseClickButtonBackgroundColorProperty = DependencyProperty.Register(nameof(MouseClickButtonBackgroundColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Orange));

        public static readonly DependencyProperty NoButtonTextColorProperty = DependencyProperty.Register(nameof(NoButtonTextColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Gray));
        public static readonly DependencyProperty NoMouseEnterButtonTextColorProperty = DependencyProperty.Register(nameof(NoMouseEnterButtonTextColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Gray));
        public static readonly DependencyProperty NoButtonIconColorProperty = DependencyProperty.Register(nameof(NoButtonIconColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Gray));
        public static readonly DependencyProperty NoMouseEnterIconColorProperty = DependencyProperty.Register(nameof(NoMouseEnterIconColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty NoButtonBorderColorProperty = DependencyProperty.Register(nameof(NoButtonBorderColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty NoMouseEnterButtonBorderColorProperty = DependencyProperty.Register(nameof(NoMouseEnterButtonBorderColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty NoButtonBackgroundColorProperty = DependencyProperty.Register(nameof(NoButtonBackgroundColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty NoMouseEnterButtonBackgroundColorProperty = DependencyProperty.Register(nameof(NoMouseEnterButtonBackgroundColor), typeof(Brush), typeof(Button), new PropertyMetadata(Brushes.White));


        #endregion

        #region 3. 依赖属性的包装器

        public int ButtonIconAndTextSpace
        {
            get => (int)GetValue(ButtonIconAndTextSpaceProperty);
            set => SetValue(ButtonIconAndTextSpaceProperty, value);
        }

        public bool ButtonIsAutoSize
        {
            get => (bool)GetValue(ButtonIsAutoSizeProperty);
            set => SetValue(ButtonIsAutoSizeProperty, value);
        }

        public FontWeight ButtonTextWeight
        {
            get => (FontWeight)GetValue(ButtonTextWeightProperty);
            set => SetValue(ButtonTextWeightProperty, value);
        }
        public Brush MouseClickButtonBorderColor
        {
            get => (Brush)GetValue(MouseClickButtonBorderColorProperty);
            set => SetValue(MouseClickButtonBorderColorProperty, value);
        }
        public Brush MouseClickIconColor
        {
            get => (Brush)GetValue(MouseClickIconColorProperty);
            set => SetValue(MouseClickIconColorProperty, value);
        }
        public Brush MouseClickButtonTextColor
        {
            get => (Brush)GetValue(MouseClickButtonTextColorProperty);
            set => SetValue(MouseClickButtonTextColorProperty, value);
        }
        public Brush MouseClickButtonBackgroundColor
        {
            get => (Brush)GetValue(MouseClickButtonBackgroundColorProperty);
            set => SetValue(MouseClickButtonBackgroundColorProperty, value);
        }
        public Brush NoButtonTextColor
        {
            get => (Brush)GetValue(NoButtonTextColorProperty);
            set => SetValue(NoButtonTextColorProperty, value);
        }

        public Brush NoMouseEnterButtonTextColor
        {
            get => (Brush)GetValue(NoMouseEnterButtonTextColorProperty);
            set => SetValue(NoMouseEnterButtonTextColorProperty, value);
        }

        public Brush NoButtonIconColor
        {
            get => (Brush)GetValue(NoButtonIconColorProperty);
            set => SetValue(NoButtonIconColorProperty, value);
        }

        public Brush NoMouseEnterIconColor
        {
            get => (Brush)GetValue(NoMouseEnterIconColorProperty);
            set => SetValue(NoMouseEnterIconColorProperty, value);
        }

        public Brush NoButtonBorderColor
        {
            get => (Brush)GetValue(NoButtonBorderColorProperty);
            set => SetValue(NoButtonBorderColorProperty, value);
        }

        public Brush NoMouseEnterButtonBorderColor
        {
            get => (Brush)GetValue(NoMouseEnterButtonBorderColorProperty);
            set => SetValue(NoMouseEnterButtonBorderColorProperty, value);
        }

        public Brush NoButtonBackgroundColor
        {
            get => (Brush)GetValue(NoButtonBackgroundColorProperty);
            set => SetValue(NoButtonBackgroundColorProperty, value);
        }

        public Brush NoMouseEnterButtonBackgroundColor
        {
            get => (Brush)GetValue(NoMouseEnterButtonBackgroundColorProperty);
            set => SetValue(NoMouseEnterButtonBackgroundColorProperty, value);
        }


        public Brush ButtonTextColor
        {
            get => (Brush)GetValue(ButtonTextColorProperty);
            set => SetValue(ButtonTextColorProperty, value);
        }

        public Brush MouseEnterButtonTextColor
        {
            get => (Brush)GetValue(MouseEnterButtonTextColorProperty);
            set => SetValue(MouseEnterButtonTextColorProperty, value);
        }

        public Brush IconColor
        {
            get => (Brush)GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        public Brush MouseEnterIconColor
        {
            get => (Brush)GetValue(MouseEnterIconColorProperty);
            set => SetValue(MouseEnterIconColorProperty, value);
        }

        public Brush ButtonBorderColor
        {
            get => (Brush)GetValue(ButtonBorderColorProperty);
            set => SetValue(ButtonBorderColorProperty, value);
        }

        public Brush MouseEnterButtonBorderColor
        {
            get => (Brush)GetValue(MouseEnterButtonBorderColorProperty);
            set => SetValue(MouseEnterButtonBorderColorProperty, value);
        }

        public Brush ButtonBackgroundColor
        {
            get => (Brush)GetValue(ButtonBackgroundColorProperty);
            set => SetValue(ButtonBackgroundColorProperty, value);
        }

        public Brush MouseEnterButtonBackgroundColor
        {
            get => (Brush)GetValue(MouseEnterButtonBackgroundColorProperty);
            set => SetValue(MouseEnterButtonBackgroundColorProperty, value);
        }

        public ButtonType ButtonType
        {
            get => (ButtonType)GetValue(ButtonTypeProperty);
            set => SetValue(ButtonTypeProperty, value);
        }

        public string ButtonText
        {
            get => (string)GetValue(ButtonTextProperty);
            set => SetValue(ButtonTextProperty, value);
        }

        public string ButtonWidth
        {
            get => (string)GetValue(ButtonWidthProperty);
            set => SetValue(ButtonWidthProperty, value);
        }

        public string ButtonHeight
        {
            get => (string)GetValue(ButtonHeightProperty);
            set => SetValue(ButtonHeightProperty, value);
        }

        public int ButtonTextSize
        {
            get => (int)GetValue(ButtonTextSizeProperty);
            set => SetValue(ButtonTextSizeProperty, value);
        }

        public int ButtonBorderSize
        {
            get => (int)GetValue(ButtonBorderSizeProperty);
            set => SetValue(ButtonBorderSizeProperty, value);
        }

        public int ButtonBorderRadius
        {
            get => (int)GetValue(ButtonBorderRadiusProperty);
            set => SetValue(ButtonBorderRadiusProperty, value);
        }

        public string ButtonIconCode
        {
            get => (string)GetValue(ButtonIconCodeProperty);
            set => SetValue(ButtonIconCodeProperty, value);
        }

        public int ButtonIconSize
        {
            get => (int)GetValue(ButtonIconSizeProperty);
            set => SetValue(ButtonIconSizeProperty, value);
        }

        #endregion
    }
}