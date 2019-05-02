using System;
using Eto.Drawing;
using Eto.Forms;
using Rune.Monogame.game;

namespace gtkView
{
    class MainWindow : Form
    {
        private readonly MonogameWidget<GameApplication> _monogameWidget;
        
        public MainWindow()
        {
            _monogameWidget = new MonogameWidget<GameApplication>();

            var dummyTree = new TreeGridView() {BackgroundColor = Colors.White, Size = new Size(200, 200)};
            dummyTree.Columns.Add(new GridColumn
            {
                HeaderText = "Content Type",
                DataCell = new TextBoxCell(0)
            });
            var treeData = new TreeGridItemCollection();
            treeData.Add(new TreeGridItem() {Tag = "test"});

            dummyTree.DataStore = treeData;
            
            var layout = new DynamicLayout();
            layout.BeginHorizontal();
            layout.Add(dummyTree);
            layout.Add(_monogameWidget.ToEto(), true);
            layout.EndHorizontal();

            Content = layout;
            WindowState = WindowState.Maximized;
        }

        protected override void OnShown(EventArgs e)
        {
            _monogameWidget.Run();
            base.OnShown(e);}
    }
}
