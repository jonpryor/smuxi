// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------

namespace Smuxi.Frontend.Gnome {
    
    
    public partial class OpenChatDialog {
        
        private Gtk.Table table1;
        
        private Smuxi.Frontend.Gnome.ChatTypeWidget f_ChatTypeWidget;
        
        private Gtk.Entry f_NameEntry;
        
        private Gtk.Label label1;
        
        private Gtk.Label label2;
        
        private Gtk.Button f_CancelButton;
        
        private Gtk.Button f_OpenButton;
        
        protected virtual void Build() {
            Stetic.Gui.Initialize(this);
            // Widget Smuxi.Frontend.Gnome.OpenChatDialog
            this.Name = "Smuxi.Frontend.Gnome.OpenChatDialog";
            this.Title = Mono.Unix.Catalog.GetString("Smuxi - Open Chat");
            this.WindowPosition = ((Gtk.WindowPosition)(4));
            this.Resizable = false;
            this.AllowGrow = false;
            // Internal child Smuxi.Frontend.Gnome.OpenChatDialog.VBox
            Gtk.VBox w1 = this.VBox;
            w1.Name = "dialog1_VBox";
            w1.BorderWidth = ((uint)(2));
            // Container child dialog1_VBox.Gtk.Box+BoxChild
            this.table1 = new Gtk.Table(((uint)(2)), ((uint)(2)), false);
            this.table1.Name = "table1";
            this.table1.RowSpacing = ((uint)(6));
            this.table1.ColumnSpacing = ((uint)(6));
            this.table1.BorderWidth = ((uint)(5));
            // Container child table1.Gtk.Table+TableChild
            this.f_ChatTypeWidget = new Smuxi.Frontend.Gnome.ChatTypeWidget();
            this.f_ChatTypeWidget.Events = ((Gdk.EventMask)(256));
            this.f_ChatTypeWidget.Name = "f_ChatTypeWidget";
            this.table1.Add(this.f_ChatTypeWidget);
            Gtk.Table.TableChild w2 = ((Gtk.Table.TableChild)(this.table1[this.f_ChatTypeWidget]));
            w2.LeftAttach = ((uint)(1));
            w2.RightAttach = ((uint)(2));
            w2.XOptions = ((Gtk.AttachOptions)(4));
            w2.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.f_NameEntry = new Gtk.Entry();
            this.f_NameEntry.CanFocus = true;
            this.f_NameEntry.Name = "f_NameEntry";
            this.f_NameEntry.IsEditable = true;
            this.f_NameEntry.ActivatesDefault = true;
            this.f_NameEntry.InvisibleChar = '●';
            this.table1.Add(this.f_NameEntry);
            Gtk.Table.TableChild w3 = ((Gtk.Table.TableChild)(this.table1[this.f_NameEntry]));
            w3.TopAttach = ((uint)(1));
            w3.BottomAttach = ((uint)(2));
            w3.LeftAttach = ((uint)(1));
            w3.RightAttach = ((uint)(2));
            w3.XOptions = ((Gtk.AttachOptions)(4));
            w3.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label1 = new Gtk.Label();
            this.label1.Name = "label1";
            this.label1.Xalign = 0F;
            this.label1.LabelProp = Mono.Unix.Catalog.GetString("_Type:");
            this.label1.UseUnderline = true;
            this.table1.Add(this.label1);
            Gtk.Table.TableChild w4 = ((Gtk.Table.TableChild)(this.table1[this.label1]));
            w4.XOptions = ((Gtk.AttachOptions)(4));
            w4.YOptions = ((Gtk.AttachOptions)(4));
            // Container child table1.Gtk.Table+TableChild
            this.label2 = new Gtk.Label();
            this.label2.Name = "label2";
            this.label2.Xalign = 0F;
            this.label2.LabelProp = Mono.Unix.Catalog.GetString("_Name:");
            this.label2.UseUnderline = true;
            this.table1.Add(this.label2);
            Gtk.Table.TableChild w5 = ((Gtk.Table.TableChild)(this.table1[this.label2]));
            w5.TopAttach = ((uint)(1));
            w5.BottomAttach = ((uint)(2));
            w5.XOptions = ((Gtk.AttachOptions)(4));
            w5.YOptions = ((Gtk.AttachOptions)(4));
            w1.Add(this.table1);
            Gtk.Box.BoxChild w6 = ((Gtk.Box.BoxChild)(w1[this.table1]));
            w6.Position = 0;
            w6.Expand = false;
            w6.Fill = false;
            // Internal child Smuxi.Frontend.Gnome.OpenChatDialog.ActionArea
            Gtk.HButtonBox w7 = this.ActionArea;
            w7.Name = "dialog1_ActionArea";
            w7.Spacing = 6;
            w7.BorderWidth = ((uint)(5));
            w7.LayoutStyle = ((Gtk.ButtonBoxStyle)(4));
            // Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.f_CancelButton = new Gtk.Button();
            this.f_CancelButton.CanFocus = true;
            this.f_CancelButton.Name = "f_CancelButton";
            this.f_CancelButton.UseStock = true;
            this.f_CancelButton.UseUnderline = true;
            this.f_CancelButton.Label = "gtk-cancel";
            this.AddActionWidget(this.f_CancelButton, -6);
            Gtk.ButtonBox.ButtonBoxChild w8 = ((Gtk.ButtonBox.ButtonBoxChild)(w7[this.f_CancelButton]));
            w8.Expand = false;
            w8.Fill = false;
            // Container child dialog1_ActionArea.Gtk.ButtonBox+ButtonBoxChild
            this.f_OpenButton = new Gtk.Button();
            this.f_OpenButton.Sensitive = false;
            this.f_OpenButton.CanDefault = true;
            this.f_OpenButton.CanFocus = true;
            this.f_OpenButton.Name = "f_OpenButton";
            this.f_OpenButton.UseStock = true;
            this.f_OpenButton.UseUnderline = true;
            this.f_OpenButton.Label = "gtk-open";
            this.AddActionWidget(this.f_OpenButton, -5);
            Gtk.ButtonBox.ButtonBoxChild w9 = ((Gtk.ButtonBox.ButtonBoxChild)(w7[this.f_OpenButton]));
            w9.Position = 1;
            w9.Expand = false;
            w9.Fill = false;
            if ((this.Child != null)) {
                this.Child.ShowAll();
            }
            this.DefaultWidth = 236;
            this.DefaultHeight = 153;
            this.f_OpenButton.HasDefault = true;
            this.Show();
        }
    }
}
