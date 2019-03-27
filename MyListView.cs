using System.Windows.Forms;

namespace DGServerControllerGUI
{
    public partial class MyListView : ListView
    {
        public MyListView()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }
    }
}
