using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
        //liste erstellen
        public List<ImageDaten> bilder = new List<ImageDaten>();

        int id = 0;

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

            //Klassen Instanz
            save_Load = new SQLite_Image_Save_Load();
        }

        public void List()
        {
            foreach (ImageDaten daten in bilder)
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
                bilder.Add(daten);

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
                datein = bilder.ElementAt(index);
                bilder.RemoveAt(index);
                view_List.Items.Remove(view_List.SelectedItem);
                save_Load.DeleteImage_BilderDB(datein.Id);
            }
        }

        //Erstellen der Datenbank Wen nicht vorhanden
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            {
                if (File.Exists("Bilder.db"))
                {
                    Loadimage();
                    return;
                }

                SQLiteConnection con = new SQLiteConnection();
                SQLiteCommand cmd;

                con.ConnectionString = "Data Source=Bilder.db;";
                cmd = con.CreateCommand();

                try
                {
                    con.Open();
                    cmd.CommandText = "CREATE TABLE bilder (id INTEGER PRIMARY KEY AUTOINCREMENT, image BLOB, image_name TEXT)";
                    cmd.ExecuteNonQuery();
                    con.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        //load images
        internal void Loadimage()
        {

            string cs = @"Data Source = Bilder.db; ";
            SQLiteConnection con = new SQLiteConnection(cs);
            SQLiteCommand cmd;

            cmd = con.CreateCommand();
            cmd.CommandText = string.Format("SELECT * FROM bilder WHERE ROWID;");

            con.Open();
            try
            {
                IDataReader reader = cmd.ExecuteReader();
                try
                {
                    while (reader.Read())
                    {
                        int idd = Convert.ToInt32((Int64)(reader[0]));
                        byte[] a = (System.Byte[])reader[1];
                        

                        //Convert und zuweisen
                        ImageDaten daten = new ImageDaten();
                        System.Windows.Controls.Image pdf = new System.Windows.Controls.Image()
                        {
                            Stretch = Stretch.Fill,
                            MaxHeight = 100,
                            MaxWidth = 100

                        };

                        pdf.Source = save_Load.ByteToImage(a);
                        daten.Images = pdf;
                        daten.Id = idd;
                        
                        bilder.Add(daten);
                    }
                }
                catch (Exception exc)
                {
                    MessageBox.Show(exc.Message);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
            con.Close();
            List();
        }
    }
}
