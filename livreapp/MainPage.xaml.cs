using Microsoft.Maui.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VersOne.Epub;
using System.IO.Compression;
using IOPath = System.IO.Path;
using Microsoft.Maui.Controls.Shapes;

namespace livreapp
{
    //defini comment sera géré le contenu de l'envoie
    public class BookContent
    {
        public string Type { get; set; }
        public List<byte> Data { get; set; }
    }

    //defini la réponse de la db
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Cover { get; set; }
        public BookContent Content { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
    }

    //class principale, fait fonctionner toute l'application (page principal)
    public partial class MainPage : ContentPage
    {
        static readonly HttpClient client = new HttpClient();
        //initialise la connection
        static MainPage()
        {
            client.Timeout = TimeSpan.FromMinutes(500);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOjQsImlhdCI6MTcwOTU0NjA0MSwiZXhwIjoxNzQxMTAzNjQxfQ.JDEzKyV4xvfmdCWWMh9GQJvHz6KGBTsSvaK076yO4ts");
            client.BaseAddress = new Uri("http://10.0.2.2:3000");
        }

        //fait les liste pour les livre (la partie filtrée fonctionnera avec l'interation du user)
        public List<Book> Books { get; set; }
        private List<Book> FilteredBooks { get; set; }

        public MainPage()
        {
            InitializeComponent();
            LoadBooks();
            SizeChanged += OnSizeChanged;
        }

        //fait des taille en %
        private void OnSizeChanged(object sender, EventArgs e)
        {
            double screenWidth = Width;
            double screenHeight = Height;

            foreach (var view in BooksStackLayout.Children)
            {
                if (view is Border border)
                {
                    border.WidthRequest = screenWidth * 0.8;
                    border.HeightRequest = screenHeight * 0.25;
                }
            }
        }

        //quand on clique sur le boutton cela ouvre le livre voulu
        private async void Button_Clicked(object sender, EventArgs e)
        {
            var button = sender as Button;
            var bookId = (int)button.CommandParameter;
            await OpenBook(bookId);
        }

        //cherche tout les livre (ne récupère pas le contenu du livre)
        private async void LoadBooks()
        {
            try
            {
                //fait la requete, vérification d'erreur en cas de problème dans la réponse. sinon il décortique les data
                HttpResponseMessage response = await client.GetAsync("/api/books");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(jsonResponse);

                    if (jsonObject.ContainsKey("data"))
                    {
                        var booksJson = jsonObject["data"].ToString();
                        Books = JsonConvert.DeserializeObject<List<Book>>(booksJson);
                        FilteredBooks = new List<Book>(Books);

                        DisplayBooks(FilteredBooks);

                        LoadingIndicator.IsVisible = false;
                        LoadingIndicator.IsRunning = false;
                        BooksStackLayout.IsVisible = true;

                        OnSizeChanged(null, null);
                    }
                    else
                    {
                        Debug.WriteLine("Erreur : la réponse JSON ne contient pas de propriété 'data'");
                    }
                }
                else
                {
                    Debug.WriteLine($"Erreur lors du chargement des livres: {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Erreur HTTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur: {ex.Message}");
            }
        }

        //affiche les livre, defini le style des zone ou son affiché le boutton, le nom et l'auteur
        private void DisplayBooks(List<Book> books)
        {
            BooksStackLayout.Children.Clear();

            foreach (var book in books)
            {
                var button = new Button
                {
                    Text = "Lire",
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.End,
                    BackgroundColor = Color.FromArgb("#2B4162"),
                    CommandParameter = book.Id
                };
                button.Clicked += Button_Clicked; // Attach event handler

                var border = new Border
                {
                    Stroke = Color.FromArgb("#385F71"),
                    BackgroundColor = Color.FromArgb("#385F71"),
                    MinimumWidthRequest = 90,
                    MinimumHeightRequest = 30,
                    StrokeThickness = 4,
                    StrokeShape = new RoundRectangle
                    {
                        CornerRadius = new CornerRadius(30)
                    },
                    Padding = new Thickness(16, 8),
                    HorizontalOptions = LayoutOptions.Center,
                    Content = new Grid
                    {
                        BackgroundColor = Color.FromArgb("#385F71"),
                        Children =
                        {
                            new Image
                            {
                                Source = book.Cover,
                                Aspect = Aspect.Fill,
                                HorizontalOptions = LayoutOptions.Start,
                                VerticalOptions = LayoutOptions.FillAndExpand
                            },
                            new StackLayout
                            {
                                HorizontalOptions = LayoutOptions.Center,
                                MinimumWidthRequest = 60,
                                Children =
                                {
                                    new Label
                                    {
                                        TextColor = Colors.White,
                                        FontSize = 18,
                                        FontAttributes = FontAttributes.Bold,
                                        Text = book.Title
                                    },
                                    new Label
                                    {
                                        TextColor = Colors.White,
                                        FontSize = 16,
                                        FontAttributes = FontAttributes.Bold,
                                        Text = book.Author
                                    },
                                    button
                                }
                            }
                        }
                    }
                };

                border.WidthRequest = Width * 0.8;
                border.HeightRequest = Height * 0.25;

                BooksStackLayout.Children.Add(border);
            }
        }

        //fait réagire la page avec la saisie de l'utilisateur
        private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = e.NewTextValue.ToLower();

            FilteredBooks = Books.Where(b => b.Title.ToLower().Contains(searchText) || b.Author.ToLower().Contains(searchText)).ToList();
            DisplayBooks(FilteredBooks);
        }

        //fait changrer l'ordre des livre suivant le choix (par auteur ou par livre) dans le tris des livre
        private void OnSortPickerSelectedIndexChanged(object sender, EventArgs e)
        {
            if (SortPicker.SelectedIndex == 0)
            {
                FilteredBooks = FilteredBooks.OrderBy(b => b.Author).ToList();
            }
            else if (SortPicker.SelectedIndex == 1)
            {
                FilteredBooks = FilteredBooks.OrderBy(b => b.Title).ToList();
            }

            DisplayBooks(FilteredBooks);
        }

        //fait la requete pour ouvrir le livre
        private async Task OpenBook(int bookId)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync($"/api/books/{bookId}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jsonObject = JObject.Parse(jsonResponse);

                    if (jsonObject.ContainsKey("data"))
                    {
                        //met les donnée dans une var
                        var bookJson = jsonObject["data"].ToString();
                        var book = JsonConvert.DeserializeObject<Book>(bookJson);

                        if (book.Content?.Data != null && book.Content.Data.Count > 0)
                        {
                            byte[] epubContent = book.Content.Data.ToArray();
                            //vérification si c'est vide
                            if (epubContent.Length == 0)
                            {
                                Debug.WriteLine("Le contenu du fichier EPUB est vide.");
                                return;
                            }

                            await OpenEpubFile(epubContent, book.Title);
                        }
                        else
                        {
                            Debug.WriteLine("Le contenu du livre est vide ou manquant.");
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Erreur : la réponse JSON ne contient pas de propriété 'data'");
                    }
                }
                else
                {
                    Debug.WriteLine($"Erreur lors de la récupération du contenu: {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"Erreur HTTP: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur: {ex.Message}");
            }
        }

        //permet d'ouvrir le livre
        public async Task OpenEpubFile(byte[] epubContent, string title)
        {
            //prend le contenu
            string filePath = IOPath.GetTempFileName();
            await File.WriteAllBytesAsync(filePath, epubContent);

            try
            {
                List<string> chapterList = new List<string>();
                //utilise zip ariche pour ouvrir le livre
                using (ZipArchive archive = ZipFile.OpenRead(filePath))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        if (entry.FullName.EndsWith(".xhtml", StringComparison.OrdinalIgnoreCase) ||
                            entry.FullName.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                        {
                            using (StreamReader reader = new StreamReader(entry.Open()))
                            {
                                string content = await reader.ReadToEndAsync();
                                chapterList.Add(content);
                            }
                        }
                    }
                }
                //vas sur la reader page et affiche le contenu
                await Navigation.PushAsync(new ReaderPage(title, chapterList));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de la lecture du fichier EPUB: {ex.Message}");
            }
            finally
            {
                File.Delete(filePath);
            }
        }
    }
}
