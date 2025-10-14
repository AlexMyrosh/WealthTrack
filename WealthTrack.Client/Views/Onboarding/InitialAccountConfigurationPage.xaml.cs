using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WealthTrack.Client.ViewModels.Onboarding;

namespace WealthTrack.Client.Views.Onboarding;

public partial class InitialAccountConfigurationPage
{
    public InitialAccountConfigurationPage()
    {
        InitializeComponent();
        BindingContext = new InitialAccountConfigurationViewModel();
    }
}