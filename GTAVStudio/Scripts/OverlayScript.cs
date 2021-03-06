﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using GTA;
using GTA.UI;
using GTAVStudio.Common;
using GTAVStudio.Forms;

namespace GTAVStudio.Scripts
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class OverlayScript : Script
    {
        public static readonly OverlayForm Overlay = new OverlayForm();
        private static readonly IntPtr MainWindowHandle = Process.GetCurrentProcess().MainWindowHandle;
        private static bool _threadStarted;
        private static bool _threadStarting;
        private static bool _overlayToggle;
        private static Thread _thread;
        public static bool ToggleOverlayNextFrame;
        public static bool ReloadSettingsNextFrame;

        public static bool IsGameActive => User32.GetForegroundWindow() == MainWindowHandle && !Game.IsPaused;

        public OverlayScript()
        {
            KeyUp += OnKeyUp;
            Tick += OnTick;
        }

        private static void OnTick(object sender, EventArgs e)
        {
            if (ReloadSettingsNextFrame)
            {
                ReloadSettingsNextFrame = false;
                
                ReloadSettings();
            }
            
            if (ToggleOverlayNextFrame)
            {
                ToggleOverlayNextFrame = false;

                ToggleOverlay();
            }

            if (_overlayToggle)
            {
                Hud.ShowCursorThisFrame();
            }
        }

        private static void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && _overlayToggle)
            {
                ToggleOverlay();
            }
            else if (e.KeyCode == StudioSettings.GetValue(Constants.Settings.Overlay, "ToggleKey", Keys.F12))
            {
                ToggleOverlay();
            }
        }

        internal static void ReloadSettings()
        {
            StudioSettings.Reload();
            StudioTranslations.Reload();
            Overlay.Reload();
        }

        internal static void ToggleOverlay()
        {
            if (_threadStarting) return;

            _overlayToggle = !_overlayToggle;

            if (!_threadStarted)
            {
                _threadStarting = true;

                _thread = new Thread(ShowForm);
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Start();
            }
            else
            {
                Overlay.Visible = _overlayToggle;
                if (Overlay.Visible)
                {
                    User32.SetForegroundWindow(Overlay.Handle);
                }
                else
                {
                    User32.SetForegroundWindow(MainWindowHandle);
                }
            }

            Game.Pause(_overlayToggle);
        }

        private static void ShowForm()
        {
            _threadStarted = true;
            _threadStarting = false;
            Application.EnableVisualStyles();
            Application.Run(Overlay);
            Overlay.Focus();
        }
    }
}