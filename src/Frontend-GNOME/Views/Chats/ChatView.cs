/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * Smuxi - Smart MUltipleXed Irc
 *
 * Copyright (c) 2005-2010 Mirco Bauer <meebey@meebey.net>
 *
 * Full GPL License: <http://www.gnu.org/licenses/gpl.txt>
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
 */

using System;
using System.Drawing;
using System.Collections.Generic;
using Smuxi.Common;
using Smuxi.Engine;
using Smuxi.Frontend;

namespace Smuxi.Frontend.Gnome
{
    // TODO: use Gtk.Bin
    public abstract class ChatView : Gtk.EventBox, IChatView, IDisposable
    {
#if LOG4NET
        private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
        public    string             ID { get; private set; }
        private   string             _Name;
        private   ChatModel          _ChatModel;
        private   bool               _HasHighlight;
        private   bool               _HasActivity;
        private   bool               _HasEvent;
        private   bool               _IsSynced;
        private   Gtk.TextMark       _EndMark;
        private   Gtk.Menu           _TabMenu;
        private   Gtk.Label          _TabLabel;
        private   Gtk.EventBox       _TabEventBox;
        private   Gtk.HBox           _TabHBox;
        private   Gtk.ScrolledWindow _OutputScrolledWindow;
        private   MessageTextView    _OutputMessageTextView;
        private   ThemeSettings      _ThemeSettings;
        private   DateTime           _LastHighlight;
        private   TaskQueue          _LastSeenHighlightQueue;
        
        public ChatModel ChatModel {
            get {
                return _ChatModel;
            }
        }
        
        public bool HasHighlight {
            get {
                return _HasHighlight;
            }
            set {
                _HasHighlight = value;
                
                if (!value) {
                    // clear highlight with "no activity"
                    HasActivity = false;
                    return;
                }

                var color = ColorTools.GetBestTextColor(
                    ColorTools.GetTextColor(_ThemeSettings.HighlightColor),
                    ColorTools.GetTextColor(
                        Gtk.Rc.GetStyle(_TabLabel).Base(Gtk.StateType.Normal)
                    ), ColorContrast.High
                );
                _TabLabel.Markup = String.Format(
                    "<span foreground=\"{0}\">{1}</span>",
                    GLib.Markup.EscapeText(color.ToString()),
                    GLib.Markup.EscapeText(_Name)
                );
            }
        }

        public bool HasActivity {
            get {
                return _HasActivity;
            }
            set {
                _HasActivity = value;

                if (HasHighlight) {
                    // don't show activity if there is a highlight active
                    return;
                }

                Gdk.Color colorValue;
                if (value) {
                    colorValue = _ThemeSettings.ActivityColor;
                } else {
                    colorValue = _ThemeSettings.NoActivityColor;
                }
                var color = ColorTools.GetBestTextColor(
                    ColorTools.GetTextColor(colorValue),
                    ColorTools.GetTextColor(
                        Gtk.Rc.GetStyle(_TabLabel).Base(Gtk.StateType.Normal)
                    ), ColorContrast.High
                );
                _TabLabel.Markup = String.Format(
                    "<span foreground=\"{0}\">{1}</span>",
                    GLib.Markup.EscapeText(color.ToString()),
                    GLib.Markup.EscapeText(_Name)
                );
            }
        }

        public bool HasEvent {
            get {
                return _HasEvent;
            }
            set {
                if (HasHighlight) {
                    return;
                }
                if (HasActivity) {
                    return;
                }
                
                if (!value) {
                    // clear event with "no activity"
                    HasActivity = false;
                    return;
                }

                var color = ColorTools.GetBestTextColor(
                    ColorTools.GetTextColor(_ThemeSettings.EventColor),
                    ColorTools.GetTextColor(
                        Gtk.Rc.GetStyle(_TabLabel).Base(Gtk.StateType.Normal)
                    ), ColorContrast.High
                );
                _TabLabel.Markup = String.Format(
                    "<span foreground=\"{0}\">{1}</span>",
                    GLib.Markup.EscapeText(color.ToString()),
                    GLib.Markup.EscapeText(_Name)
                );
            }
        }
        
        public virtual bool HasSelection {
            get {
                return _OutputMessageTextView.HasTextViewSelection;
            }
        }
        
        public virtual new bool HasFocus {
            get {
                return base.HasFocus || _OutputMessageTextView.HasFocus;
            }
            set {
                _OutputMessageTextView.HasFocus = value;
            }
        }
        
        public Gtk.Widget LabelWidget {
            get {
                return _TabEventBox;
            }
        }

        public MessageTextView OutputMessageTextView {
            get {
                return _OutputMessageTextView;
            }
        }
        
        protected Gtk.ScrolledWindow OutputScrolledWindow {
            get {
                return _OutputScrolledWindow;
            }
        }

        protected Gtk.HBox TabHBox {
            get {
                return _TabHBox;
            }
        }

        protected ThemeSettings ThemeSettings {
            get {
                return _ThemeSettings;
            }
        }

        public ChatView(ChatModel chat)
        {
            Trace.Call(chat);
            
            _ChatModel = chat;
            _Name = _ChatModel.Name;
            ID = _ChatModel.ID;
            Name = _Name;
            
            MessageTextView tv = new MessageTextView();
            _EndMark = tv.Buffer.CreateMark("end", tv.Buffer.EndIter, false); 
            tv.ShowTimestamps = true;
            tv.ShowMarkerline = true;
            tv.Editable = false;
            tv.CursorVisible = true;
            tv.WrapMode = Gtk.WrapMode.Char;
            tv.MessageAdded += OnMessageTextViewMessageAdded;
            tv.MessageHighlighted += OnMessageTextViewMessageHighlighted;
            _OutputMessageTextView = tv;
            
            Gtk.ScrolledWindow sw = new Gtk.ScrolledWindow();
            //sw.HscrollbarPolicy = Gtk.PolicyType.Never;
            sw.HscrollbarPolicy = Gtk.PolicyType.Automatic;
            sw.VscrollbarPolicy = Gtk.PolicyType.Always;
            sw.ShadowType = Gtk.ShadowType.In;
            sw.Add(_OutputMessageTextView);
            _OutputScrolledWindow = sw;
            
            // popup menu
            _TabMenu = new Gtk.Menu();
            
            Gtk.ImageMenuItem close_item = new Gtk.ImageMenuItem(Gtk.Stock.Close, null);
            close_item.Activated += new EventHandler(OnTabMenuCloseActivated);  
            _TabMenu.Append(close_item);
            
            //FocusChild = _OutputTextView;
            //CanFocus = false;
            
            _TabLabel = new Gtk.Label();
            _TabLabel.Text = _Name;
            
            _TabHBox = new Gtk.HBox();
            _TabHBox.PackEnd(new Gtk.Fixed(), true, true, 0);
            _TabHBox.PackEnd(_TabLabel, false, false, 0);
            _TabHBox.ShowAll();
            
            _TabEventBox = new Gtk.EventBox();
            _TabEventBox.VisibleWindow = false;
            _TabEventBox.ButtonPressEvent += new Gtk.ButtonPressEventHandler(OnTabButtonPress);
            _TabEventBox.Add(_TabHBox);
            _TabEventBox.ShowAll();

            _ThemeSettings = new ThemeSettings();

            // OPT-TODO: this should use a TaskStack instead of TaskQueue
            _LastSeenHighlightQueue = new TaskQueue("LastSeenHighlightQueue("+_Name+")");
            _LastSeenHighlightQueue.AbortedEvent += OnLastSeenHighlightQueueAbortedEvent;
            _LastSeenHighlightQueue.ExceptionEvent += OnLastSeenHighlightQueueExceptionEvent;
        }
        
        ~ChatView()
        {
            Trace.Call();

            Dispose(false);
        }

        public override void Dispose()
        {
            Trace.Call();

            base.Dispose();

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            Trace.Call(disposing);

            if (disposing) {
                if (_LastSeenHighlightQueue != null) {
                    _LastSeenHighlightQueue.Dispose();
                }
                _LastSeenHighlightQueue = null;
            }
        }

        public virtual void ScrollUp()
        {
            Trace.Call();

            Gtk.Adjustment adj = _OutputScrolledWindow.Vadjustment;
            adj.Value -= adj.PageSize - adj.StepIncrement;
        }
        
        public virtual void ScrollDown()
        {
            Trace.Call();

            // note: Upper - PageSize is the farest scrollable position! 
            Gtk.Adjustment adj = _OutputScrolledWindow.Vadjustment;
            if ((adj.Value + adj.PageSize) <= (adj.Upper - adj.PageSize)) {
                adj.Value += adj.PageSize - adj.StepIncrement;
            } else {
                // there is no page left to scroll, so let's just scroll to the
                // farest position instead
                adj.Value = adj.Upper - adj.PageSize;
            }
        }
        
        public virtual void ScrollToStart()
        {
            Trace.Call();
            
            Gtk.Adjustment adj = _OutputScrolledWindow.Vadjustment;
            adj.Value = adj.Lower;
        }
        
        public virtual void ScrollToEnd()
        {
            Trace.Call();
            
            Gtk.Adjustment adj = _OutputScrolledWindow.Vadjustment;
#if LOG4NET
            _Logger.Debug("ScrollToEnd(): Vadjustment.Value: " + adj.Value +
                          " Vadjustment.Upper: " + adj.Upper +
                          " Vadjustment.PageSize: " + adj.PageSize);
#endif
            
            // BUG? doesn't work always for some reason
            // seems like GTK+ doesn't update the adjustment till we give back control
            //adj.Value = adj.Upper - adj.PageSize;
            
            //_OutputTextView.Buffer.MoveMark(_EndMark, _OutputTextView.Buffer.EndIter);
            //_OutputTextView.ScrollMarkOnscreen(_EndMark);
            //_OutputTextView.ScrollToMark(_EndMark, 0.49, true, 0.0, 0.0);
            
            //_OutputTextView.ScrollMarkOnscreen(_OutputTextView.Buffer.InsertMark);

            //_OutputTextView.ScrollMarkOnscreen(_OutputTextView.Buffer.GetMark("tail"));
            
            System.Reflection.MethodBase mb = Trace.GetMethodBase();
            // WORKAROUND1: scroll after one second delay
            /*
            GLib.Timeout.Add(1000, new GLib.TimeoutHandler(delegate {
                Trace.Call(mb);
                
                _OutputTextView.ScrollMarkOnscreen(_EndMark);
                return false;
            }));
            */
            // WORKAROUND2: scroll when GTK+ mainloop is idle
            GLib.Idle.Add(new GLib.IdleHandler(delegate {
                Trace.Call(mb);
                
                _OutputMessageTextView.ScrollMarkOnscreen(_EndMark);
                return false;
            }));
        }
        
        public virtual void Enable()
        {
            Trace.Call();
        }
        
        public virtual void Disable()
        {
            Trace.Call();

            _IsSynced = false;
        }
        
        public virtual void Sync()
        {
            Trace.Call();
            
#if LOG4NET
            _Logger.Debug("Sync() syncing messages");
#endif
            // sync messages
            // cleanup, be sure the output is empty
            _OutputMessageTextView.Clear();
            IList<MessageModel> messages = _ChatModel.Messages;
            if (messages.Count > 0) {
                foreach (MessageModel msg in messages) {
                    AddMessage(msg);
                }
            }
            // REMOTING CALL
            if (_LastHighlight > _ChatModel.LastSeenHighlight) {
                HasHighlight = true;
            }

            _IsSynced = true;
        }
        
        public virtual void AddMessage(MessageModel msg)
        {
            Trace.Call(msg);
            
            _OutputMessageTextView.AddMessage(msg);
        }
        
        public virtual void Clear()
        {
            Trace.Call();
            
            _OutputMessageTextView.Clear();
        }
        
        public virtual void ApplyConfig(UserConfig config)
        {
            Trace.Call(config);
            
            if (config == null) {
                throw new ArgumentNullException("config");
            }
            
            _ThemeSettings = new ThemeSettings(config);

            _OutputMessageTextView.ApplyConfig(config);
        }
        
        public virtual void Close()
        {
            Trace.Call();

            ChatModel.ProtocolManager.CloseChat(
                Frontend.FrontendManager,
                ChatModel
            );
        }

        protected virtual void OnTabButtonPress(object sender, Gtk.ButtonPressEventArgs e)
        {
            Trace.Call(sender, e);
            
            if (e.Event.Button == 3) {
                _TabMenu.Popup(null, null, null, e.Event.Button, e.Event.Time);
                _TabMenu.ShowAll();
            } else if (e.Event.Button == 2) {
                Close();
            }
        }
        
        protected virtual void OnTabMenuCloseActivated(object sender, EventArgs e)
        {
            Trace.Call(sender, e);
            
            Close();
        }
        
        protected virtual void OnMessageTextViewMessageAdded(object sender, MessageTextViewMessageAddedEventArgs e)
        {
            Trace.Call(sender, e);
            
            // HACK: out of scope?
            // probably we should use the ChatViewManager instead?
            if (Frontend.MainWindow.Notebook.CurrentChatView != this) {
                switch (e.Message.MessageType) {
                    case MessageType.Normal:
                        HasActivity = true;
                        break;
                    case MessageType.Event:
                        HasEvent = true;
                        break;
                }
            }

            Gtk.ScrolledWindow sw = _OutputScrolledWindow;
            Gtk.TextView tv = _OutputMessageTextView;

            if (sw.Vadjustment.Upper == (sw.Vadjustment.Value + sw.Vadjustment.PageSize)) {
                // the scrollbar is way at the end, lets autoscroll
                Gtk.TextIter endit = tv.Buffer.EndIter;
                tv.Buffer.PlaceCursor(endit);
                tv.Buffer.MoveMark(tv.Buffer.InsertMark, endit);
                tv.ScrollMarkOnscreen(tv.Buffer.InsertMark);
            }

            // update the end mark
            tv.Buffer.MoveMark(_EndMark, tv.Buffer.EndIter);
        }
        
        protected virtual void OnMessageTextViewMessageHighlighted(object sender, MessageTextViewMessageHighlightedEventArgs e)
        {
            Trace.Call(sender, e);
            
            // HACK: out of scope?
            // only beep if the main windows has no focus (the user is
            // elsewhere) and the chat is was already synced, as during sync we
            // would get insane from all beeping caused by the old highlights
            if (!Frontend.MainWindow.HasToplevelFocus &&
                _IsSynced &&
                Frontend.UserConfig["Sound/BeepOnHighlight"] != null &&
                (bool) Frontend.UserConfig["Sound/BeepOnHighlight"]) {
#if LOG4NET
                _Logger.Debug("OnMessageTextViewMessageHighlighted(): BEEP!");
#endif
                Display.Beep();
            }

            if (_IsSynced) {
                bool isActiveChat =
                    Frontend.MainWindow.HasToplevelFocus &&
                    Object.ReferenceEquals(
                        Frontend.MainWindow.Notebook.CurrentChatView,
                        this
                    );

                var method = Trace.GetMethodBase();
                // update last seen highlight
                // OPT-TODO: we should use a TaskStack here OR at least a
                // timeout approach that will only sync once per 30 seconds!
                _LastSeenHighlightQueue.Queue(delegate {
                    Trace.Call(method, null, null);

                    // unhandled exception here would kill the syncer thread
                    try {
                        if (isActiveChat) {
                            // REMOTING CALL 1
                            _ChatModel.LastSeenHighlight = e.Message.TimeStamp;
                        } else {
                            // REMOTING CALL 1
                            if (_ChatModel.LastSeenHighlight < e.Message.TimeStamp) {
                                Gtk.Application.Invoke(delegate {
                                    HasHighlight = true;
                                });
                            }
                        }
                    } catch (Exception ex) {
#if LOG4NET
                        _Logger.Error("OnMessageTextViewMessageHighlighted(): Exception: ", ex);
#endif
                    }
                });
            } else {
                _LastHighlight = e.Message.TimeStamp;
            }
        }

        protected virtual void OnLastSeenHighlightQueueExceptionEvent(object sender, TaskQueueExceptionEventArgs e)
        {
            Trace.Call(sender, e);

#if LOG4NET
            _Logger.Error("Exception in TaskQueue: ", e.Exception);
            _Logger.Error("Inner-Exception: ", e.Exception.InnerException);
#endif
            Frontend.ShowException(e.Exception);
        }

        protected virtual void OnLastSeenHighlightQueueAbortedEvent(object sender, EventArgs e)
        {
            Trace.Call(sender, e);

#if LOG4NET
            _Logger.Debug("OnLastSeenHighlightQueueAbortedEvent(): task queue aborted!");
#endif
        }

        private static string _(string msg)
        {
            return Mono.Unix.Catalog.GetString(msg);
        }
    }
}
