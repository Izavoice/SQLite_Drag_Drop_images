using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPF_Bilder_Spielerrei;

namespace WPF_Bilder_Spielerei
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        private int id = 0;

        //Felder
        private readonly ContextMenu cm = new ContextMenu
        {
            Focusable = false,
        };
        private readonly MenuItem delete = new MenuItem
        {
            Header = "Löschen",
            Focusable = false,
        };

        //Klassen
        private readonly SQLite_Image_Save_Load save_Load;

        public MainWindow()
        {
            InitializeComponent();
            //Delete button
            _ = cm.Items.Add(delete);

            //Instanz
            save_Load = new SQLite_Image_Save_Load();
        }

        public void List()
        {
            foreach (ImageDaten daten in save_Load.bilder)
            {
                StackPanel panel = new StackPanel();
                panel.Children.Add(daten.Images);
                panel.Tag = daten.Id;
                view_List.Items.Add(panel);
            }
        }

        //Drop methode
        private void Picture_panel_Drop(object sender, DragEventArgs e)
        {
            string[] dropped = (string[])e.Data.GetData(DataFormats.FileDrop, true);

            dropped.ToList().ForEach((i) =>
            {
                ImageDaten daten = new ImageDaten();
                //drop Datei info
                FileInfo file = new FileInfo(i);

                //Datei für die Datenabank
                System.Drawing.Image SQLimage = new Bitmap(file.FullName);
                System.Windows.Controls.Image images = new System.Windows.Controls.Image
                {
                    Source = new BitmapImage(new Uri(file.FullName, UriKind.RelativeOrAbsolute)),

                    Stretch = Stretch.Fill,
                    MaxHeight = 100,
                    MaxWidth = 100
                };

                //Convert zu Byte
                byte[] pic = save_Load.ImageToByte(SQLimage);

                //save Image in DB
                id = Convert.ToInt32(save_Load.SaveImage(pic, file.Name));

                //liste daten
                daten.Id = id;
                daten.Images = images;
                save_Load.bilder.Add(daten);

                StackPanel panel = new StackPanel();
                panel.Children.Add(daten.Images);
                panel.Tag = daten.Id;
                view_List.Items.Add(panel);
            });
        }

        //Rechtsklick Menü
        private void View_List_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            cm.IsOpen = true;
            delete.Click += new RoutedEventHandler(Btn_delete);

        }
        //Methode zum löschen
        private void Btn_delete(object sender, EventArgs e)
        {
            ImageDaten datein;
            int index = view_List.SelectedIndex;
            if (index >= 0)
            {
                datein = save_Load.bilder.ElementAt(index);
                save_Load.bilder.RemoveAt(index);
                view_List.Items.Remove(view_List.SelectedItem);
                save_Load.DeleteImage_BilderDB(datein.Id);
            }
        }

        //Erstellen der Datenbank Wen nicht vorhanden
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (File.Exists("Bilder.db"))
            {
                save_Load.Loadimages();
                List();
                return;
            }
            else
            {
                save_Load.Datenbank();
            }
        }
    }
}
