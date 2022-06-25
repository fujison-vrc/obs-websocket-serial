using Prism.Mvvm;

namespace OBSWebsocketSerial.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "OBS Websocket Serial";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {

        }
    }
}
