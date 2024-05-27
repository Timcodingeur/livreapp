using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System;

namespace livreapp
{
    // Page où l'on lit le livre
    public partial class ReaderPage : ContentPage
    {
        private List<string> _chapters;
        private int _currentChapterIndex;

        public ReaderPage(string title, List<string> chapters)
        {
            InitializeComponent();
            TitleLabel.Text = title;
            _chapters = chapters;
            _currentChapterIndex = 0;

            // Afficher le premier chapitre
            DisplayChapter(_currentChapterIndex);
        }

        // Afficher le contenu du chapitre spécifié
        private void DisplayChapter(int chapterIndex)
        {
            if (chapterIndex >= 0 && chapterIndex < _chapters.Count)
            {
                string chapterContent = _chapters[chapterIndex];
                string styledContent = AddCssStyle(chapterContent);
                ContentWebView.Source = new HtmlWebViewSource
                {
                    Html = styledContent
                };
                ChapterLabel.Text = $"Chapitre {chapterIndex + 1}";
            }
        }

        // Ajoute le style CSS au contenu HTML
        private string AddCssStyle(string htmlContent)
        {
            string css = @"
                <style>
                    body {
                        background-color: #385F71;
                        color: white;
                        font-family: Arial, sans-serif;
                        margin: 20px;
                    }
                    img {
                        max-width: 100%;
                        height: auto;
                    }
                </style>";

            return $"<html><head>{css}</head><body>{htmlContent}</body></html>";
        }

        // Gestionnaire de clic pour le bouton précédent
        private void OnPreviousChapterClicked(object sender, EventArgs e)
        {
            if (_currentChapterIndex > 0)
            {
                _currentChapterIndex--;
                DisplayChapter(_currentChapterIndex);
            }
        }

        // Gestionnaire de clic pour le bouton suivant
        private void OnNextChapterClicked(object sender, EventArgs e)
        {
            if (_currentChapterIndex < _chapters.Count - 1)
            {
                _currentChapterIndex++;
                DisplayChapter(_currentChapterIndex);
            }
        }

        // Renvoie à la page principale
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
