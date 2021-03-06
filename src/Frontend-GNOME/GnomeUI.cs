/*
 * $Id$
 * $URL$
 * $Rev$
 * $Author$
 * $Date$
 *
 * Smuxi - Smart MUltipleXed Irc
 *
 * Copyright (c) 2005-2006 Mirco Bauer <meebey@meebey.net>
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
using System.Reflection;
using SysDiag = System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Globalization; 
using System.ComponentModel;
using Mono.Unix;
using Smuxi.Common;
using Smuxi.Engine;
using Smuxi.Frontend;

namespace Smuxi.Frontend.Gnome
{
    public class GnomeUI : PermanentRemoteObject, IFrontendUI
    {
#if LOG4NET
        private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
        private int _Version = 0;
        private ChatViewManager _ChatViewManager;
        
        public int Version {
            get {
                return _Version;
            }
        }
        
        public GnomeUI(ChatViewManager chatViewManager)
        {
            _ChatViewManager = chatViewManager;
        }
        
        public void AddChat(ChatModel chat)
        {
            TraceRemotingCall(chat);
            
            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, chat);
                
                _ChatViewManager.AddChat(chat);
            });
        }
        
        private void _AddMessageToChat(ChatModel chatModel, MessageModel msg)
        {
            ChatView chatView = _ChatViewManager.GetChat(chatModel);
            if (chatView == null) {
#if LOG4NET
                _Logger.Fatal(
                    String.Format(
                        "_AddMessageToChat(): " +
                        "_ChatViewManager.GetChat(chatModel) " +
                        "chatModel.Name: {0} returned null!",
                        chatModel.Name
                    )
                );
#endif
                return;
            }
            
            DateTime start, stop;
            start = DateTime.UtcNow;
            chatView.AddMessage(msg);
            stop = DateTime.UtcNow;
#if LOG4NET
            _Logger.Debug(
                String.Format(
                    "_AddMessageToChat(): chatView.AddMessage() took: {0:0.00} ms",
                    (stop - start).TotalMilliseconds
                )
            );
#endif
        }
        
        public void AddMessageToChat(ChatModel epage, MessageModel fmsg)
        {
            TraceRemotingCall(epage, fmsg);

            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, epage, fmsg);
                
                _AddMessageToChat(epage, fmsg);
            });
        }
        
        public void RemoveChat(ChatModel chat)
        {
            TraceRemotingCall(chat);

            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, chat);
                
                _ChatViewManager.RemoveChat(chat);
            });
        }
        
        public void EnableChat(ChatModel chat)
        {
            TraceRemotingCall(chat);

            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, chat);
                
                _ChatViewManager.EnableChat(chat);
            });
        }
        
        public void DisableChat(ChatModel chat)
        {
            TraceRemotingCall(chat);
            
            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, chat);
                
                _ChatViewManager.DisableChat(chat);
            });
        }
        
        public void SyncChat(ChatModel chatModel)
        {
            TraceRemotingCall(chatModel);

            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, chatModel);

                ChatView chatView = _ChatViewManager.GetChat(chatModel);
                if (chatView == null) {
#if LOG4NET
                    _Logger.Fatal(
                        String.Format(
                            "SyncChat(): " +
                            "_ChatViewManager.GetChat(chatModel) " +
                            "chatModel.Name: {0} returned null!",
                            chatModel.Name
                        )
                    );
#endif
                    return;
                }

#if LOG4NET
                DateTime syncStart = DateTime.UtcNow;
#endif
                chatView.Sync();
#if LOG4NET
                DateTime syncStop = DateTime.UtcNow;
                double duration = syncStop.Subtract(syncStart).TotalMilliseconds;
                _Logger.Debug("SyncChat() done, syncing took: " + Math.Round(duration) + " ms");
#endif

                // maybe a BUG here? should be tell the FrontendManager before we sync?
                Frontend.FrontendManager.AddSyncedChat(chatModel);

                // BUG: doesn't work?!?
                chatView.ScrollToEnd();
                
                /*
                // this hack is bad for local engine users, and doesn't really
                // make things better for remote engine users, so it stays disabled for now
                // BUG: clearing highlight here is a bad idea, highlight in
                // person chats for the first message go lost here!
                // no better way currently to fix this, see trac bug #50
                chatView.HasHighlight = false;
                */
            });
        }
        
        public void AddPersonToGroupChat(GroupChatModel groupChat, PersonModel person)
        {
            TraceRemotingCall(groupChat, person);

            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, groupChat, person);
                
                GroupChatView groupChatView = (GroupChatView) _ChatViewManager.GetChat(groupChat);
                if (groupChatView == null) {
#if LOG4NET
                    _Logger.Fatal(
                        String.Format(
                            "AddPersonToGroupChat(): " +
                            "_ChatViewManager.GetChat(groupChat) " +
                            "groupChat.Name: {0} returned null!",
                            groupChat.Name
                        )
                    );
#endif
                    return;
                }
                groupChatView.AddPerson(person);
            });
        }
        
        public void UpdatePersonInGroupChat(GroupChatModel groupChat, PersonModel oldPerson, PersonModel newPerson)
        {
            TraceRemotingCall(groupChat, oldPerson, newPerson);

            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, groupChat, oldPerson, newPerson);
                
                GroupChatView groupChatView = (GroupChatView) _ChatViewManager.GetChat(groupChat);
                if (groupChatView == null) {
#if LOG4NET
                    _Logger.Fatal(
                        String.Format(
                            "UpdatePersonInGroupChat(): " +
                            "_ChatViewManager.GetChat(groupChat) " +
                            "groupChat.Name: {0} returned null!",
                            groupChat.Name
                        )
                    );
#endif
                    return;
                }
                groupChatView.UpdatePerson(oldPerson, newPerson);
            });
        }
        
        public void UpdateTopicInGroupChat(GroupChatModel groupChat, MessageModel topic)
        {
            TraceRemotingCall(groupChat, topic);

            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, groupChat, topic);
                
                GroupChatView groupChatView = (GroupChatView)Frontend.MainWindow.Notebook.GetChat(groupChat);
                if (groupChatView == null) {
#if LOG4NET
                    _Logger.Fatal(
                        String.Format(
                            "UpdateTopicInGroupChat(): " +
                            "_ChatViewManager.GetChat(groupChat) " +
                            "groupChat.Name: {0} returned null!",
                            groupChat.Name
                        )
                    );
#endif
                    return;
                }
                groupChatView.Topic = topic;
            });
        }
        
        public void RemovePersonFromGroupChat(GroupChatModel groupChat, PersonModel person)
        {
            TraceRemotingCall(groupChat, person);

            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, groupChat, person);
            
                GroupChatView groupChatView = (GroupChatView) _ChatViewManager.GetChat(groupChat);
                if (groupChatView == null) {
#if LOG4NET
                    _Logger.Fatal(
                        String.Format(
                            "RemovePersonFromGroupChat(): " +
                            "_ChatViewManager.GetChat(groupChat) " +
                            "groupChat.Name: {0} returned null!",
                            groupChat.Name
                        )
                    );
#endif
                    return;
                }
                groupChatView.RemovePerson(person);
            });
        }
        
        public void SetNetworkStatus(string status)
        {
            TraceRemotingCall(status);

            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, status);
                Frontend.MainWindow.NetworkStatusbar.Push(0, status);
                Frontend.MainWindow.UpdateTitle(null, status);
            });
        }
        
        public void SetStatus(string status)
        {
            TraceRemotingCall(status);

            MethodBase mb = Trace.GetMethodBase();
            Gtk.Application.Invoke(delegate {
                TraceRemotingCall(mb, status);
                Frontend.MainWindow.Statusbar.Push(0, status);
            });
        }
        
        private static string _(string msg)
        {
            return Mono.Unix.Catalog.GetString(msg);
        }
        
        [SysDiag.Conditional("REMOTING_TRACE")]
        protected static void TraceRemotingCall(MethodBase mb, params object[] parameters)
        {
            Trace.Call(mb, parameters);
        }
        
        [SysDiag.Conditional("REMOTING_TRACE")]
        protected static void TraceRemotingCall(params object[] parameters)
        {
            TraceRemotingCall(Trace.GetMethodBase(), parameters);
        }
    }
}
