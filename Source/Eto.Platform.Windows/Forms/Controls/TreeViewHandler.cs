using System;
using swf = System.Windows.Forms;
using sd = System.Drawing;
using Eto.Forms;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;

namespace Eto.Platform.Windows.Forms.Controls
{
	public class TreeViewHandler : WindowsControl<swf.TreeView, TreeView>, ITreeView
	{
		ITreeStore top;
		ContextMenu contextMenu;
		Dictionary<Image, string> images = new Dictionary<Image, string> ();
		static string EmptyName = Guid.NewGuid ().ToString ();
		
		public TreeViewHandler ()
		{
			this.Control = new swf.TreeView ();
			
			this.Control.BeforeExpand += delegate(object sender, System.Windows.Forms.TreeViewCancelEventArgs e) {
				var item = e.Node.Tag as ITreeItem;
				if (e.Node.Nodes.Count == 1 && e.Node.Nodes[0].Name == EmptyName)
				{
					PopulateNodes (e.Node.Nodes, item);
				}
			};
			this.Control.AfterSelect += delegate(object sender, System.Windows.Forms.TreeViewEventArgs e) {
				Widget.OnSelectionChanged (EventArgs.Empty);
			};
		}

		public ITreeStore DataStore {
			get { return top; }
			set {
				top = value;
				this.Control.ImageList = null;
				images.Clear ();
				PopulateNodes (this.Control.Nodes, top);
			}
		}
		
		public override void AttachEvent (string handler)
		{
			switch (handler) {
			case TreeView.ExpandingEvent:
				this.Control.BeforeExpand += (sender, e) => {
					var args = new TreeViewItemCancelEventArgs(e.Node.Tag as ITreeItem);
					Widget.OnExpanding (args);
					e.Cancel = args.Cancel;
				};
				break;
			case TreeView.ExpandedEvent:
				this.Control.AfterExpand += (sender, e) => {
					Widget.OnExpanded (new TreeViewItemEventArgs(e.Node.Tag as ITreeItem));
				};
				break;
			case TreeView.CollapsingEvent:
				this.Control.BeforeCollapse += (sender, e) => {
					var args = new TreeViewItemCancelEventArgs(e.Node.Tag as ITreeItem);
					Widget.OnCollapsing (args);
					e.Cancel = args.Cancel;
				};
				break;
			case TreeView.CollapsedEvent:
				this.Control.AfterCollapse += (sender, e) => {
					Widget.OnCollapsed (new TreeViewItemEventArgs(e.Node.Tag as ITreeItem));
				};
				break;
			case TreeView.ActivatedEvent:
				this.Control.KeyDown += (sender, e) => {
					if (e.KeyData == swf.Keys.Return && this.SelectedItem != null)
					{
						Widget.OnActivated (new TreeViewItemEventArgs(this.SelectedItem));
						e.Handled = true;
					}
				};
				this.Control.DoubleClick += (sender, e) => {
					if (this.SelectedItem != null)
					{
						Widget.OnActivated (new TreeViewItemEventArgs (this.SelectedItem));
					}
				};
				break;
			default:
				base.AttachEvent (handler);
				break;
			}
		}
		
		public ContextMenu ContextMenu {
			get { return contextMenu; }
			set {
				contextMenu = value;
				if (contextMenu != null)
					this.Control.ContextMenuStrip = ((ContextMenuHandler)contextMenu.Handler).Control;
				else
					this.Control.ContextMenuStrip = null;
			}
		}
		
		void PopulateNodes (System.Windows.Forms.TreeNodeCollection nodes, ITreeStore item)
		{
			nodes.Clear ();
			var count = item.Count;
			for (int i=0; i<count; i++) {
				var child = item[i];
				var node = nodes.Add (child.Key, child.Text, GetImageKey (child.Image));
				node.Tag = child;
				
				if (child.Expandable) {
					if (child.Expanded) {
						PopulateNodes (node.Nodes, child);
						node.Expand ();
					} else {
						node.Nodes.Add (EmptyName, string.Empty);
					}
				}
			}
		}
		
		string GetImageKey (Image image)
		{
			if (image == null)
				return null;
			
			if (this.Control.ImageList == null)
				this.Control.ImageList = new System.Windows.Forms.ImageList{ ColorDepth = swf.ColorDepth.Depth32Bit };
			string key;
			if (!images.TryGetValue (image, out key)) {
				key = Guid.NewGuid ().ToString ();
				this.Control.ImageList.AddImage (image, key);
			}
			return key;
		}

		public ITreeItem SelectedItem {
			get {
				var node = this.Control.SelectedNode;
				if (node == null)
					return null;
				return node.Tag as ITreeItem;
			}
			set {
				// TODO: finish this
				var nodes = this.Control.Nodes.Find (value.Key, true);
				if (nodes.Length > 0)
					this.Control.SelectedNode = nodes [0];
			}
		}

		public void RefreshData ()
		{
			this.Control.ImageList = null;
			images.Clear ();
			Control.BeginUpdate ();
			PopulateNodes (this.Control.Nodes, top);
			Control.EndUpdate ();
		}

		public void RefreshItem (ITreeItem item)
		{
			var nodes = Control.Nodes.Find (item.Key, true);
			var node = nodes.FirstOrDefault(r => object.Equals(item, r));
			if (node != null) {
				node.Text = item.Text;
				PopulateNodes (node.Nodes, item);
				if (node.IsExpanded != item.Expanded)
				{
					if (item.Expanded)
						node.Expand ();
					else
						node.Collapse ();
				}
			}
		}
	}
}

