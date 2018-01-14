//
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Chartreuse.Today.App.Controls
{
    [TemplatePart(Name = FullScreenDialog.PartBackButton, Type = typeof(Button))]
    [TemplatePart(Name = FullScreenDialog.PartRootBorder, Type = typeof(Border))]
    [TemplatePart(Name = FullScreenDialog.PartRootGrid, Type = typeof(Grid))]
    [TemplatePart(Name = FullScreenDialog.PartContent, Type = typeof(ContentPresenter))]
    public class FullScreenDialog : ContentControl
    {
        private Grid rootGrid;
        private Border rootBorder;
        private Button backButton;
        private double? width;

        private const string PartRootBorder = "PART_RootBorder";
        private const string PartRootGrid = "PART_RootGrid";
        private const string PartBackButton = "PART_BackButton";
        private const string PartContent = "PART_Content";

        public FullScreenDialog()
        {
            this.DefaultStyleKey = typeof(FullScreenDialog);
        }

        public void ForceWidth(double width)
        {
            this.width = width;
        }

        public void Close()
        {
            if (this.BackButtonClicked != null)
                this.BackButtonClicked(this, new RoutedEventArgs());
        }

        /// <summary>
        /// Invoked whenever application code or internal processes (such as a rebuilding layout pass) 
        /// call ApplyTemplate. In simplest terms, this means the method is called just before a UI
        /// element displays in your app. Override this method to influence the default post-template logic of a class.
        /// </summary>
        protected override void OnApplyTemplate()
        {
            this.rootGrid = (Grid)this.GetTemplateChild(PartRootGrid);
            this.rootBorder = (Border)this.GetTemplateChild(PartRootBorder);
            this.backButton = (Button)this.GetTemplateChild(PartBackButton);

            this.ResizeContainers();

            if (this.backButton != null)
            {
                this.backButton.Click += (bbs, bba) =>
                {
                    if (this.BackButtonClicked != null)
                    {
                        this.BackButtonClicked(bbs, bba);
                    }
                    else
                    {
                        this.IsOpen = false;
                    }
                };
            }

            Window.Current.SizeChanged += this.OnWindowSizeChanged;
            this.Unloaded += this.OnUnloaded;

            base.OnApplyTemplate();
        }

        private void ResizeContainers()
        {
            if (this.rootGrid != null)
            {
                this.rootGrid.Height = Window.Current.Bounds.Height;

                if (this.width.HasValue)
                    this.rootGrid.Width = this.width.Value;
                else
                    this.rootGrid.Width = Window.Current.Bounds.Width;
            }

            if (this.rootBorder != null)
                this.rootBorder.Width = Window.Current.Bounds.Width;
        }

        private void OnWindowSizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            this.ResizeContainers();
        }

        private void OnUnloaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.Unloaded -= this.OnUnloaded;
            Window.Current.SizeChanged -= this.OnWindowSizeChanged;
        }

        #region Events
        /// <summary>
        /// Occurs when the back button was clicked.
        /// </summary>
        public event RoutedEventHandler BackButtonClicked;
        #endregion

        #region Dependency Properties

        /// <summary>
        /// Gets or sets the back button visibility.
        /// </summary>
        public Visibility BackButtonVisibility
        {
            get { return (Visibility)this.GetValue(BackButtonVisibilityProperty); }
            set { this.SetValue(BackButtonVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="BackButtonVisibility"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BackButtonVisibilityProperty =
            DependencyProperty.Register("BackButtonVisibility", typeof(Visibility), typeof(FullScreenDialog), new PropertyMetadata(Visibility.Collapsed));

        /// <summary>
        /// Gets or sets a value indicating whether the dialog is open.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this dialog is open; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpen
        {
            get { return (bool)this.GetValue(IsOpenProperty); }
            set { this.SetValue(IsOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsOpen"/> dependency property
        /// </summary>
        public static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen", typeof(bool), typeof(FullScreenDialog), new PropertyMetadata(false, OnIsOpenPropertyChanged));

        private static void OnIsOpenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                FullScreenDialog dlg = d as FullScreenDialog;
                if (dlg != null)
                {
                    dlg.ApplyTemplate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title
        {
            get { return (string)this.GetValue(TitleProperty); }
            set { this.SetValue(TitleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(FullScreenDialog), null);

        /// <summary>
        /// Gets or sets the back button command.
        /// </summary>
        public ICommand BackButtonCommand
        {
            get { return (ICommand)this.GetValue(BackButtonCommandProperty); }
            set { this.SetValue(BackButtonCommandProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="BackButtonCommand"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BackButtonCommandProperty =
            DependencyProperty.Register("BackButtonCommand", typeof(ICommand), typeof(FullScreenDialog), new PropertyMetadata(DependencyProperty.UnsetValue));

        /// <summary>
        /// Gets or sets the back button command parameter.
        /// </summary>
        public object BackButtonCommandParameter
        {
            get { return (object)this.GetValue(BackButtonCommandParameterProperty); }
            set { this.SetValue(BackButtonCommandParameterProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="BackButtonCommandParameter"/> dependency property
        /// </summary>
        public static readonly DependencyProperty BackButtonCommandParameterProperty =
            DependencyProperty.Register("BackButtonCommandParameter", typeof(object), typeof(FullScreenDialog), new PropertyMetadata(DependencyProperty.UnsetValue));
        #endregion
    }
}