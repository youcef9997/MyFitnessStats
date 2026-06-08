using assessment2526.Models;
using assessment2526.Services;
using System.Collections.ObjectModel;

namespace assessment2526.ViewModels
{
    internal class HistoryViewModel
    {
        public ObservableCollection<SessionStats> Runs => HistoryService.Runs;
    }
}
