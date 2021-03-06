/*
 * $Id: IrcProtocolManager.cs 149 2007-04-11 16:47:52Z meebey $
 * $URL: svn+ssh://svn.qnetp.net/svn/smuxi/smuxi/trunk/src/Engine/IrcProtocolManager.cs $
 * $Rev: 149 $
 * $Author: meebey $
 * $Date: 2007-04-11 18:47:52 +0200 (Wed, 11 Apr 2007) $
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Smuxi.Common;

namespace Smuxi.Engine
{
    public abstract class ProtocolManagerBase : PermanentRemoteObject, IProtocolManager
    {
#if LOG4NET
        private static readonly log4net.ILog _Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#endif
        private static readonly string       _LibraryTextDomain = "smuxi-engine";
        private Session         _Session;
        private string          _Host;
        private int             _Port;
        private bool            _IsConnected;
        
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        
        public virtual string Host {
            get {
                return _Host;
            }
            protected set {
                _Host = value; 
            }
        }
        
        public virtual int Port {
            get {
                return _Port;
            }
            protected set {
                _Port = value; 
            }
        }
        
        public virtual bool IsConnected {
            get {
                return _IsConnected;
            }
            protected set {
                _IsConnected = value; 
            }
        }
        
        public abstract string NetworkID {
            get;
        }
        
        public abstract string Protocol {
            get;
        }
        
        public abstract ChatModel Chat {
            get;
        }
        
        public virtual IList<ChatModel> Chats {
            get {
                IList<ChatModel> chats = new List<ChatModel>();
                lock (_Session.Chats) {
                    foreach (ChatModel chat in _Session.Chats) {
                        if (chat.ProtocolManager == this) {
                            chats.Add(chat);
                        }
                    }
                }
                return chats;
            }
        }
        
        protected Session Session {
            get {
                return _Session;
            }
        }
        
        protected ProtocolManagerBase(Session session)
        {
            Trace.Call(session);
            
            if (session == null) {
                throw new ArgumentNullException("session");
            }
            
            _Session = session;
        }
        
        public virtual void Dispose()
        {
            Trace.Call();
            
            foreach (ChatModel chat in Chats) {
                _Session.RemoveChat(chat);
            }
        }
        
        public abstract bool Command(CommandModel cmd);
        public abstract void Connect(FrontendManager fm,
                                     string hostname, int port,
                                     string username, string password);
        public abstract void Reconnect(FrontendManager fm);
        public abstract void Disconnect(FrontendManager fm);
        
        public abstract IList<GroupChatModel> FindGroupChats(GroupChatModel filter);
        public abstract void OpenChat(FrontendManager fm, ChatModel chat);
        public abstract void CloseChat(FrontendManager fm, ChatModel chat);
        
        protected void NotConnected(CommandModel cmd)
        {
            cmd.FrontendManager.AddTextToChat(
                cmd.Chat,
                String.Format(
                    "-!- {0}",
                    _("Not connected to server")
                )
            );
        }

        protected void NotEnoughParameters(CommandModel cmd)
        {
            cmd.FrontendManager.AddTextToChat(
                cmd.Chat,
                String.Format(
                    "-!- {0}",
                    String.Format(
                        _("Not enough parameters for {0} command"),
                        cmd.Command
                    )
                )
            );
        }
        
        protected virtual void OnConnected(EventArgs e)
        {
            Trace.Call(e);

            Session.AddTextToChat(
                Chat,
                String.Format(
                    "-!- {0}",
                    String.Format(
                        _("Connected to {0}"),
                        NetworkID
                    )
                )
            );

            Session.UpdateNetworkStatus();

            if (Connected != null) {
                Connected(this, e);
            }
        }
        
        protected virtual void OnDisconnected(EventArgs e)
        {
            Trace.Call(e);

            Session.AddTextToChat(
                Chat,
                String.Format(
                    "-!- {0}",
                    String.Format(
                        _("Disconnected from {0}"),
                        NetworkID
                    )
                )
            );

            Session.UpdateNetworkStatus();

            if (Disconnected != null) {
                Disconnected(this, e);
            }
        }
        
        private static string _(string msg)
        {
            return LibraryCatalog.GetString(msg, _LibraryTextDomain);
        }
        
        protected ChatModel GetChat(string id, ChatType chatType)
        {
            return _Session.GetChat(id, chatType, this);
        }
        
        protected void ParseUrls(MessageModel msg)
        {
            string urlRegex;
            //urlRegex = "((([a-zA-Z][0-9a-zA-Z+\\-\\.]*:)?/{0,2}[0-9a-zA-Z;/?:@&=+$\\.\\-_!~*'()%]+)?(#[0-9a-zA-Z;/?:@&=+$\\.\\-_!~*'()%]+)?)");
            // It was constructed according to the BNF grammar given in RFC 2396 (http://www.ietf.org/rfc/rfc2396.txt).
            
            /*
            urlRegex = @"^(?<s1>(?<s0>[^:/\?#]+):)?(?<a1>" + 
                                  @"//(?<a0>[^/\?#]*))?(?<p0>[^\?#]*)" + 
                                  @"(?<q1>\?(?<q0>[^#]*))?" + 
                                  @"(?<f1>#(?<f0>.*))?");
            */ 

            urlRegex = @"(^| )(((https?|ftp):\/\/)|www\.)(([0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|net|org|info|biz|gov|name|edu|[a-zA-Z][a-zA-Z]))(:[0-9]+)?((\/|\?)[^ ""]*[^ ,;\.:"">)])?";
            Regex reg = new Regex(urlRegex, RegexOptions.IgnoreCase);
            // clone MessageParts
            IList<MessagePartModel> parts = new List<MessagePartModel>(msg.MessageParts);
            foreach (MessagePartModel part in parts) {
                if (!(part is TextMessagePartModel)) {
                    continue;
                }
                
                TextMessagePartModel textPart = (TextMessagePartModel) part;
                Match urlMatch = reg.Match(textPart.Text);
                // OPT: fast regex scan
                if (!urlMatch.Success) {
                    // no URLs in this MessagePart, nothing to do
                    continue;
                }
                
                // found URL(s)
                // remove current MessagePartModel as we need to split it
                int idx = msg.MessageParts.IndexOf(part);
                msg.MessageParts.RemoveAt(idx);
                
                string[] textPartParts = textPart.Text.Split(new char[] {' '});
                for (int i = 0; i < textPartParts.Length; i++) {
                    string textPartPart = textPartParts[i];
                    urlMatch = reg.Match(textPartPart);
                    if (urlMatch.Success) {
                        UrlMessagePartModel urlPart = new UrlMessagePartModel(textPartPart);
                        //urlPart.ForegroundColor = new TextColor();
                        msg.MessageParts.Insert(idx++, urlPart);
                        msg.MessageParts.Insert(idx++, new TextMessagePartModel(" "));
                    } else {
                        // FIXME: we put each text part into it's own object, instead of combining them (the smart way)
                        TextMessagePartModel notUrlPart = new TextMessagePartModel(textPartPart + " ");
                        // restore formatting / colors from the original text part
                        notUrlPart.IsHighlight     = textPart.IsHighlight;
                        notUrlPart.ForegroundColor = textPart.ForegroundColor;
                        notUrlPart.BackgroundColor = textPart.BackgroundColor;
                        notUrlPart.Bold            = textPart.Bold;
                        notUrlPart.Italic          = textPart.Italic;
                        notUrlPart.Underline       = textPart.Underline;
                        msg.MessageParts.Insert(idx++, notUrlPart);
                    }
                }
            }
        }
        
        protected void ParseSmileys(MessageModel msg)
        {
            string simleyRegex;
            simleyRegex = @":-?(\(|\))";
            Regex reg = new Regex(simleyRegex);
            // clone MessageParts
            IList<MessagePartModel> parts = new List<MessagePartModel>(msg.MessageParts);
            foreach (MessagePartModel part in parts) {
                if (!(part is TextMessagePartModel)) {
                    continue;
                }
                
                TextMessagePartModel textPart = (TextMessagePartModel) part;
                Match simleyMatch = reg.Match(textPart.Text);
                // OPT: fast regex scan
                if (!simleyMatch.Success) {
                    // no smileys in this MessagePart, nothing to do
                    continue;
                }
                
                // found smiley(s)
                // remove current MessagePartModel as we need to split it
                int idx = msg.MessageParts.IndexOf(part);
                msg.MessageParts.RemoveAt(idx);
                
                string[] textPartParts = textPart.Text.Split(new char[] {' '});
                for (int i = 0; i < textPartParts.Length; i++) {
                    string textPartPart = textPartParts[i];
                    simleyMatch = reg.Match(textPartPart);
                    if (simleyMatch.Success) {
                        string filename = null;
                        if (textPartPart == ":-)") {
                            filename = "smile.png";
                        }
                        ImageMessagePartModel imagePart = new ImageMessagePartModel(
                            filename,
                            textPartPart
                        );
                        msg.MessageParts.Insert(idx++, imagePart);
                        msg.MessageParts.Insert(idx++, new TextMessagePartModel(" "));
                    } else {
                        // FIXME: we put each text part into it's own object, instead of combining them (the smart way)
                        TextMessagePartModel notUrlPart = new TextMessagePartModel(textPartPart + " ");
                        // restore formatting / colors from the original text part
                        notUrlPart.IsHighlight     = textPart.IsHighlight;
                        notUrlPart.ForegroundColor = textPart.ForegroundColor;
                        notUrlPart.BackgroundColor = textPart.BackgroundColor;
                        notUrlPart.Bold            = textPart.Bold;
                        notUrlPart.Italic          = textPart.Italic;
                        notUrlPart.Underline       = textPart.Underline;
                        msg.MessageParts.Insert(idx++, notUrlPart);
                    }
                }
            }
        }

        protected virtual TextColor GetIdentityNameColor(string identityName)
        {
            if (identityName == null) {
                throw new ArgumentNullException("identityName");
            }

            if ((bool) Session.UserConfig["Interface/Notebook/Channel/NickColors"]) {
                return new TextColor(identityName.GetHashCode() & 0xFFFFFF);
            }

            return TextColor.None;
         }

        protected virtual bool ContainsHighlight(string msg)
        {
            Regex regex;
            //go through the user's custom highlight words and check for them.
            foreach (string HighlightString in (string[]) Session.UserConfig["Interface/Chat/HighlightWords"]) {
                if (String.IsNullOrEmpty(HighlightString)) {
                    continue;
                }

                if (HighlightString.StartsWith("/") && HighlightString.EndsWith("/")) {
                    // This is a regex, so just build a regex out of the string.
                    regex = new Regex(HighlightString.Substring(1,HighlightString.Length-2));
                } else {
                    // Plain text - make a regex that matches the word as long as it's separated properly.
                    string regex_string = String.Format("(^|\\W){0}($|\\W)", Regex.Escape(HighlightString));
                    regex = new Regex(regex_string, RegexOptions.IgnoreCase);
                }

                if (regex.Match(msg).Success) {
                    return true;
                }
            }
            
            return false;
        }
    }
}
