using System;
using System.Data.SQLite;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WPF_Bilder_Spielerei
{
    public class SQLite_Image_Save_Load
    {


        //Convert Methode
        internal byte[] ImageToByte(System.Drawing.Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, image.RawFormat);
                byte[] imageBytes = ms.ToArray();
                return imageBytes;
            }
        }

        //Save Image zur Bilder.db
        internal long SaveImage(byte[] imagen, string name)
        {
            long id;
            string cs = @"Data Source = Bilder.db; ";
            SQLiteConnection con = new SQLiteConnection(cs);

            //Create command
            SQLiteCommand cmd = con.CreateCommand();
            cmd.CommandText = string.Format("INSERT INTO bilder (image, image_name) VALUES (@0,@1);");

            //parameter erstellen
            SQLiteParameter paramImage = new SQLiteParameter("@0", System.Data.DbType.Binary);
            SQLiteParameter paramName = new SQLiteParameter("@1", System.Data.DbType.String);


            paramImage.Value = imagen;
            paramName.Value = name;


            cmd.Parameters.Add(paramImage);
            cmd.Parameters.Add(paramName);

            con.Open();

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            id = con.LastInsertRowId;
            con.Close();

            return id;
        }


        //Daten löschen
        public void DeleteImage_BilderDB(int id)
        {
            string cs = @"Data Source = Bilder.db; ";
            SQLiteConnection con = new SQLiteConnection(cs);
            SQLiteCommand cmd;

            cmd = con.CreateCommand();
            cmd.CommandText = string.Format("DELETE FROM bilder WHERE id = (@0);");

            //parameter erstellen
            SQLiteParameter paramID = new SQLiteParameter("@0", System.Data.DbType.Int32)
            {
                Value = id
            };

            cmd.Parameters.Add(paramID);

            con.Open();

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            con.Close();
        }


        //Convert zu BitmapImage
        internal BitmapImage ByteToImage(byte[] imageBytes)
        {
            using (var ms = new System.IO.MemoryStream(imageBytes))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
    }
}
