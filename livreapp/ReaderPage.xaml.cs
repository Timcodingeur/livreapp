using Microsoft.Maui.Controls;

namespace livreapp
{
    //page ou l'on lit le livre
    public partial class ReaderPage : ContentPage
    {
        public ReaderPage(string title, string content)
        {
            InitializeComponent();
            TitleLabel.Text = title;
            //transforme le contenu qui  été envoyer en html en contenu brut
            ContentWebView.Source = new HtmlWebViewSource
            {
                Html = content
            };
        }

        //renvoie a la page principale
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
