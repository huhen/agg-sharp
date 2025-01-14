﻿/*
Copyright (c) 2014, Lars Brubaker
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using MatterHackers.Agg;
using MatterHackers.Agg.Image;
using MatterHackers.Agg.PlatformAbstract;
using MatterHackers.Agg.UI;
using MatterHackers.VectorMath;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
#if !__ANDROID__
using System.Drawing.Imaging;
#endif

namespace MatterHackers.GuiAutomation
{
    public abstract class NativeMethods
    {
        public bool LeftButtonDown { get; private set; }

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        public const int MOUSEEVENTF_MIDDLEDOWN = 0x20;
        public const int MOUSEEVENTF_MIDDLEUP = 0x40;

        public abstract ImageBuffer GetCurrentScreen();
        public int GetCurrentScreenHeight()
        {
            Size sz = new Size();// System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;
            return sz.Height;
        }

        public abstract Point2D CurrentMousPosition();

        public abstract void SetCursorPosition(int x, int y);

        public virtual void CreateMouseEvent(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo)
        {
            if (dwFlags == MOUSEEVENTF_LEFTDOWN)
            {
                // send it to the window
                LeftButtonDown = true;
            }
            else if (dwFlags == MOUSEEVENTF_LEFTUP)
            {
                LeftButtonDown = false;
            }
        }

        public abstract void Type(string textToType);
    }

    public class AggInputMethods : NativeMethods
    {
        Point2D currentMousePosition;

        public override ImageBuffer GetCurrentScreen()
        {
            throw new NotImplementedException();
        }

        public override Point2D CurrentMousPosition()
        {
            SystemWindow.AllOpenSystemWindows[0].Invalidate();
            return currentMousePosition;
        }

        SystemWindow currentlyHookedWindow = null;

        public override void SetCursorPosition(int x, int y)
        {
            SystemWindow systemWindow = SystemWindow.AllOpenSystemWindows[SystemWindow.AllOpenSystemWindows.Count - 1];
            if(currentlyHookedWindow != systemWindow)
            {
                if (currentlyHookedWindow != null)
                {
                    currentlyHookedWindow.DrawAfter -= DrawMouse;
                }
                currentlyHookedWindow = systemWindow;
                if (currentlyHookedWindow != null)
                {
                    currentlyHookedWindow.DrawAfter += DrawMouse;
                }
            }
            currentMousePosition = new Point2D(x, y);
            Point2D windowPosition = AutomationRunner.ScreenToSystemWindow(currentMousePosition, systemWindow);
            if (LeftButtonDown)
            {
                MouseEventArgs aggEvent = new MouseEventArgs(MouseButtons.Left, 1, windowPosition.x, windowPosition.y, 0);
                UiThread.RunOnIdle(() => systemWindow.OnMouseMove(aggEvent));
            }
            else
            {
                MouseEventArgs aggEvent = new MouseEventArgs(MouseButtons.None, 1, windowPosition.x, windowPosition.y, 0);
                UiThread.RunOnIdle(() => systemWindow.OnMouseMove(aggEvent));
            }
        }

        private void DrawMouse(GuiWidget drawingWidget, DrawEventArgs e)
        {
            AutomationRunner.RenderMouse(currentlyHookedWindow, e.graphics2D);
        }

        public override void CreateMouseEvent(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo)
        {
            // figure out where this is on our agg windows
            // for now only send mouse events to the top most window
            //foreach (SystemWindow systemWindow in SystemWindow.OpenWindows)
            SystemWindow systemWindow = SystemWindow.AllOpenSystemWindows[SystemWindow.AllOpenSystemWindows.Count - 1];
            {
                Point2D windowPosition = AutomationRunner.ScreenToSystemWindow(currentMousePosition, systemWindow);
                if(systemWindow.LocalBounds.Contains(windowPosition))
                {
                    MouseButtons mouseButtons = MapButtons(cButtons);
                    // create the agg event
                    if (dwFlags == MOUSEEVENTF_LEFTDOWN)
                    {
                        MouseEventArgs aggEvent = new MouseEventArgs(mouseButtons, 1, windowPosition.x, windowPosition.y, 0);
                        // send it to the window
                        if (LeftButtonDown)
                        {
                            UiThread.RunOnIdle(() => systemWindow.OnMouseMove(aggEvent));
                        }
                        else
                        {
                            UiThread.RunOnIdle(() => systemWindow.OnMouseDown(aggEvent));
                        }
                    }
                    else if(dwFlags == MOUSEEVENTF_LEFTUP)
                    {
                        MouseEventArgs aggEvent = new MouseEventArgs(mouseButtons, 0, windowPosition.x, windowPosition.y, 0);
                        // send it to the window
                        UiThread.RunOnIdle(() => systemWindow.OnMouseUp(aggEvent));
                    }
                    else if(dwFlags == MOUSEEVENTF_RIGHTDOWN)
                    {

                    }
                    else if (dwFlags == MOUSEEVENTF_RIGHTUP)
                    {

                    }
                    else if (dwFlags == MOUSEEVENTF_MIDDLEDOWN)
                    {

                    }
                    else if (dwFlags == MOUSEEVENTF_MIDDLEUP)
                    {

                    }
                }
            }

            base.CreateMouseEvent(dwFlags, dx, dy, cButtons, dwExtraInfo);
        }

        private MouseButtons MapButtons(int cButtons)
        {
            switch (cButtons)
            {
                case MOUSEEVENTF_LEFTDOWN:
                case MOUSEEVENTF_LEFTUP:
                    return MouseButtons.Left;

                case MOUSEEVENTF_RIGHTDOWN:
                case MOUSEEVENTF_RIGHTUP:
                    return MouseButtons.Left;

                case MOUSEEVENTF_MIDDLEDOWN:
                case MOUSEEVENTF_MIDDLEUP:
                    return MouseButtons.Left;
            }

            return MouseButtons.Left;
        }

        public override void Type(string textToType)
        {
            SystemWindow systemWindow = SystemWindow.AllOpenSystemWindows[SystemWindow.AllOpenSystemWindows.Count - 1];

            foreach (char character in textToType)
            {
                //UiThread.RunOnIdle(() => systemWindow.OnKeyDown(aggKeyEvent));
                //Keyboard.SetKeyDownState(aggKeyEvent.KeyCode, true);

                KeyPressEventArgs aggKeyPressEvent = new KeyPressEventArgs(character);
                UiThread.RunOnIdle(() => systemWindow.OnKeyPress(aggKeyPressEvent));

                //widgetToSendTo.OnKeyUp(aggKeyEvent);
                //Keyboard.SetKeyDownState(aggKeyEvent.KeyCode, false);
            }
        }
    }

#if !__ANDROID__
    public class WindowsInputMethods : NativeMethods
    {
		// P/Invoke declarations
        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr ptr);

		[DllImport("User32.Dll")]
		public static extern long SetCursorPos(int x, int y); 

		[DllImport("user32.dll")]
		public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public override void CreateMouseEvent(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo)
        {
            mouse_event(dwFlags, dx, dy, cButtons, dwExtraInfo);
        }

        public override void SetCursorPosition(int x, int y)
        {
            SetCursorPos(x, y);
        }

        public override Point2D CurrentMousPosition()
        {
            Point2D mousePos = new Point2D(System.Windows.Forms.Control.MousePosition.X, System.Windows.Forms.Control.MousePosition.Y);
            return mousePos;
        }

        public override ImageBuffer GetCurrentScreen()
		{
			ImageBuffer screenCapture = new ImageBuffer();

			Size sz = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Size;
			IntPtr hDesk = GetDesktopWindow();
			IntPtr hSrce = GetWindowDC(hDesk);
			IntPtr hDest = CreateCompatibleDC(hSrce);
			IntPtr hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
			IntPtr hOldBmp = SelectObject(hDest, hBmp);
			bool b = BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, 0, 0, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
			Bitmap bmpScreenCapture = Bitmap.FromHbitmap(hBmp);
			SelectObject(hDest, hOldBmp);
			DeleteObject(hBmp);
			DeleteDC(hDest);
			ReleaseDC(hDesk, hSrce);

			//bmpScreenCapture.Save("bitmapsave.png");

			screenCapture = new ImageBuffer(bmpScreenCapture.Width, bmpScreenCapture.Height, 32, new BlenderBGRA());
			BitmapData bitmapData = bmpScreenCapture.LockBits(new Rectangle(0, 0, bmpScreenCapture.Width, bmpScreenCapture.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmpScreenCapture.PixelFormat);

			int offset;
			byte[] buffer = screenCapture.GetBuffer(out offset);
			int bitmapDataStride = bitmapData.Stride;
			int backBufferStrideInBytes = screenCapture.StrideInBytes();
			int backBufferHeight = screenCapture.Height;
			int backBufferHeightMinusOne = backBufferHeight - 1;

			unsafe
			{
				byte* bitmapDataScan0 = (byte*)bitmapData.Scan0;
				fixed (byte* pSourceFixed = &buffer[offset])
				{
					byte* pSource = bitmapDataScan0 + bitmapDataStride * backBufferHeightMinusOne;
					byte* pDestBuffer = pSourceFixed;
					for (int y = 0; y < screenCapture.Height; y++)
					{
						int* pSourceInt = (int*)pSource;
						pSourceInt -= (bitmapDataStride * y / 4);

						int* pDestBufferInt = (int*)pDestBuffer;
						pDestBufferInt += (backBufferStrideInBytes * y / 4);

						for (int x = 0; x < screenCapture.Width; x++)
						{
							pDestBufferInt[x] = pSourceInt[x];
						}
					}
				}
			}

			bmpScreenCapture.UnlockBits(bitmapData);

			bmpScreenCapture.Dispose();

			return screenCapture;
		}

        public override void Type(string textToType)
        {
            System.Windows.Forms.SendKeys.SendWait(textToType);
        }
    }
#endif
}