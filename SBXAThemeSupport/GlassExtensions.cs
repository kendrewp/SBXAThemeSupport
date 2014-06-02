// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlassExtensions.cs" company="Ruf Informatik AG">
//   Copyright © Ruf Informatik AG. All rights reserved.
// </copyright>
// <copyright file="KeyboardBehaviors.cs" company="Ascension Technologies, Inc.">
//   Copyright © Ascension Technologies, Inc. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace SBXAThemeSupport
{
    using System;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    ///     public partial class Window1 : Window
    ///     {
    ///     public Window1()
    ///     {
    ///     InitializeComponent();
    ///     this.GlassBackground();
    ///     }
    ///     http://ciantic.blogspot.com.au/2009/10/wpf-window-with-areo-glass-background-c.html
    /// </summary>
    public static class GlassingExtension
    {
        #region Public Methods and Operators

        /// <summary>
        /// Sets glass background to whole window.
        /// </summary>
        /// <remarks>
        /// Remember to set your WPF Window Background to "Transparent"!
        /// </remarks>
        /// <param name="win">
        /// The Window to apply the glass background.
        /// </param>
        public static void GlassBackground(this Window win)
        {
            // Glass extend WINAPI thingie http://msdn.microsoft.com/en-us/library/aa969512%28VS.85%29.aspx form more details
            // If any of the margins is "-1" the whole window is glass!
            win.GlassBackground(-1, 0, 0, 0);
        }

        /// <summary>
        /// Sets glass background to custom margins in the window.
        /// </summary>
        /// <param name="win">
        /// The Window to apply the glass background.
        /// </param>
        /// <param name="left">
        /// The left.
        /// </param>
        /// <param name="right">
        /// The right.
        /// </param>
        /// <param name="top">
        /// The top.
        /// </param>
        /// <param name="bottom">
        /// The bottom.
        /// </param>
        public static void GlassBackground(this Window win, int left, int right, int top, int bottom)
        {
            // Why would you read the inner workings? Why? If you need to know why...
            // DwmExtendFrameIntoClientArea http://msdn.microsoft.com/en-us/library/aa969512%28VS.85%29.aspx is the magical WINAPI call
            // rest is just crap to get its parameters populated.
            win.Loaded += delegate
                {
                    try
                    {
                        // Obtain the window handle for WPF application
                        IntPtr mainWindowPtr = new WindowInteropHelper(win).Handle;
                        HwndSource mainWindowSrc = HwndSource.FromHwnd(mainWindowPtr);

                        // Transparent shall be glassed!
                        if (mainWindowSrc != null)
                        {
                            if (mainWindowSrc.CompositionTarget != null)
                            {
                                mainWindowSrc.CompositionTarget.BackgroundColor = System.Windows.Media.Colors.Transparent;
                            }

                            // Margin for the DwmExtendFrameIntoClientArea WINAPI call.
                            var margins = new NonClientRegionApi.Margins
                                              {
                                                  CxLeftWidth = left, 
                                                  CxRightWidth = right, 
                                                  CyBottomHeight = bottom, 
                                                  CyTopHeight = top
                                              };

                            // Glass extend WINAPI thingie http://msdn.microsoft.com/en-us/library/aa969512%28VS.85%29.aspx form more details
                            int hr = NonClientRegionApi.DwmExtendFrameIntoClientArea(mainWindowSrc.Handle, ref margins);
                            if (hr < 0)
                            {
                                //DwmExtendFrameIntoClientArea Failed
                            }
                            else
                            {
                                win.Background = System.Windows.Media.Brushes.Transparent;
                            }
                        }
                    }
                    catch (DllNotFoundException)
                    {
                        // If not glassing capabilities (Windows XP...), paint background white.
                        Application.Current.MainWindow.Background = System.Windows.Media.Brushes.White;
                    }
                };
        }

        #endregion

        /// <summary>
        /// The non client region api.
        /// </summary>
        private static class NonClientRegionApi
        {
            #region Public Methods and Operators

            /// <summary>
            /// The dwm extend frame into client area.
            /// </summary>
            /// <param name="hwnd">
            /// The hwnd.
            /// </param>
            /// <param name="pMarInset">
            /// The p mar inset.
            /// </param>
            /// <returns>
            /// The <see cref="int"/>.
            /// </returns>
            [DllImport("DwmApi.dll")]
            public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins pMarInset);

            #endregion

            /// <summary>
            ///     The margins.
            /// </summary>
            [StructLayout(LayoutKind.Sequential)]
            public struct Margins
            {
                /// <summary>
                /// The cx left width.
                /// </summary>
                public int CxLeftWidth; // width of left border that retains its size

                /// <summary>
                /// The cx right width.
                /// </summary>
                public int CxRightWidth; // width of right border that retains its size

                /// <summary>
                /// The cy top height.
                /// </summary>
                public int CyTopHeight; // height of top border that retains its size

                /// <summary>
                /// The cy bottom height.
                /// </summary>
                public int CyBottomHeight; // height of bottom border that retains its size
            }
        }
    }
}