using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ElementUI.WPF.Component
{
    public class Icon : Control
    {
        #region 1. 构造器

        static Icon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Icon), new FrameworkPropertyMetadata(typeof(Icon)));
        }

        #endregion

        #region 2. 注册依赖属性

        /// <summary>
        /// 默认图标边长
        /// </summary>
        private const int DefaultIconSide = 18;

        /// <summary>
        /// 默认图标Unicode代码
        /// </summary>
        private const string DefaultIconCode = "〇";

        /// <summary>
        /// 默认图标颜色
        /// </summary>
        private static readonly Brush DefaultIconColor = Brushes.Black;

        public static readonly DependencyProperty IconCodeProperty = DependencyProperty.Register(nameof(IconCode), typeof(string), typeof(Icon), new PropertyMetadata(DefaultIconCode));
        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(nameof(IconSize), typeof(int), typeof(Icon), new PropertyMetadata(DefaultIconSide));
        public static readonly DependencyProperty IconColorProperty = DependencyProperty.Register(nameof(IconColor), typeof(Brush), typeof(Icon), new PropertyMetadata(DefaultIconColor));

        #endregion

        #region 3. 依赖属性的包装器

        public string IconCode
        {
            get => (string)GetValue(IconCodeProperty);
            set => SetValue(IconCodeProperty, value);
        }

        public int IconSize
        {
            get => (int)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public Brush IconColor
        {
            get => (Brush)GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        #endregion
    }
}