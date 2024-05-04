using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;

namespace Paint_application
{
    /// <summary>
    /// Interaction logic for AddLayerWindow.xaml
    /// </summary>
    public partial class AddLayerWindow : Window
    {
        private ObservableCollection<string> _layerList;
        private ObservableCollection<bool> _layerState;
        int indexToRename;
        public AddLayerWindow(ObservableCollection<string> _layerList, ObservableCollection<bool> _layerState, int indexToRename)
        {
            InitializeComponent();

            this._layerList = _layerList;
            this._layerState = _layerState;
            this.indexToRename = indexToRename;
        }

        private void OkBtn_Click(object sender, RoutedEventArgs e)
        {
            if (indexToRename == -1)
            {
                string text = TextInput.Text;

                if (text.Length == 0)
                    return;

                _layerList.Add(text);
                _layerState.Add(true);

                this.Close();
            }
            else
            {
                string text = TextInput.Text;

                if (text.Length == 0)
                    return;

                _layerList[indexToRename] = text;

                this.Close();
            }
        }
    }
}
