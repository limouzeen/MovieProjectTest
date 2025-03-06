using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Drawing;

namespace MovieProjectTest
{
    internal class ShareData
    {

        //เชื่อมต่อฐานข้อมูล Microsoft SQL Server
        public static string connStr = @"Server=DESKTOP-HVGLT4A\SQLEXPRESS;Database=movie_record_db;Integrated Security=True;";


        //แสดงข้อความแจ้งเตือน (Error)
        public static void ShowErrorMSG(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        //แสดงข้อความแจ้งเตือน (Info)
        public static void ShowInfoMSG(string message)
        {
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        //แสดงข้อความแจ้งเตือน (Warning)
        public static void ShowWarningMSG(string message)
        {
            MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        //แปลง byte[] เป็น Image (ใช้สำหรับแสดงรูปภาพจากฐานข้อมูล)
        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0) return null;
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                return Image.FromStream(ms);
            }
        }

        //แปลง Image เป็น byte[] (ใช้สำหรับบันทึกภาพลงฐานข้อมูล)
        public static byte[] imageToByteArray(Image image)
        {
            if (image == null) return null;
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }


        public static void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // อนุญาตให้กดปุ่มควบคุม เช่น Backspace
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // อนุญาตให้กดตัวเลข (0-9)
            if (char.IsDigit(e.KeyChar))
            {
                return;
            }

            // อนุญาตให้กด '.' แต่ต้องไม่มีจุดซ้ำ
            if (e.KeyChar == '.' && !((sender as TextBox).Text.Contains(".")))
            {
                return;
            }

            // หากไม่ตรงตามเงื่อนไขด้านบน ยกเลิกการพิมพ์
            e.Handled = true;
        }


        public static void textBox_KeyPressNum(object sender, KeyPressEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            // Allow control keys like Backspace
            if (char.IsControl(e.KeyChar))
            {
                return;
            }

            // Allow only digits (0-9)
            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Block other characters
                return;
            }

            // Prevent leading zeros
            if (e.KeyChar == '0' && textBox.Text.Length == 0)
            {
                e.Handled = true; // Block leading zero
                return;
            }

            // Prevent multiple leading zeros (e.g., "000001")
            if (textBox.Text == "0")
            {
                textBox.Text = e.KeyChar.ToString(); // Replace "0" with the new number
                textBox.SelectionStart = textBox.Text.Length; // Move cursor to end
                e.Handled = true; // Block input since we replaced it manually
            }
        }



        public static void Num_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // ไม่ให้พิมพ์อักขระที่ไม่ใช่ตัวเลข
            }
        }





        public static void Txtzeronum_TextChanged(object sender, EventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt == null) return;

            // ลบ 0 ที่นำหน้าตัวเลข (แต่ให้คงไว้หากพิมพ์เพียง 0 ตัวเดียว)
            if (txt.Text.Length > 1 && txt.Text.StartsWith("0"))
            {
                txt.Text = txt.Text.TrimStart('0');
                txt.SelectionStart = txt.Text.Length; // คงตำแหน่งเคอร์เซอร์ไว้ที่ท้ายข้อความ
            }
        }

    }
}
