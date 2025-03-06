using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovieProjectTest
{
    public partial class FrmMovie : Form
    {
        public FrmMovie()
        {
            InitializeComponent();
        }

        private void FrmMovie_Load(object sender, EventArgs e)
        {
            TestDatabaseConnection();

           

            tbMovieDVDTotal.KeyPress += new KeyPressEventHandler(ShareData.Num_KeyPress);
            tbMovieDVDTotal.TextChanged += new EventHandler(ShareData.Txtzeronum_TextChanged);
            tbMovieDVDPrice.KeyPress += new KeyPressEventHandler(ShareData.textBox_KeyPress);
            tbMovieDVDPrice.TextChanged += new EventHandler(ShareData.Txtzeronum_TextChanged);

        }
        

        private void TestDatabaseConnection()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(ShareData.connStr))
                {
                    conn.Open();
                    MessageBox.Show("เชื่อมต่อฐานข้อมูลสำเร็จ!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            ResetForm();
            LoadMovieCategories();
            LoadMovieList();
        }


        private void LoadMovieCategories()
        {
            using (SqlConnection conn = new SqlConnection(ShareData.connStr))
            {
                conn.Open();
                string query = "SELECT movieTypeId, movieTypeName FROM movie_type_tb";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();

                Dictionary<int, string> movieCategories = new Dictionary<int, string>();
                while (reader.Read())
                {
                    movieCategories.Add(reader.GetInt32(0), reader.GetString(1));
                }

                cbbMovieType.DataSource = new BindingSource(movieCategories, null);
                cbbMovieType.DisplayMember = "Value"; // แสดงชื่อหมวดหมู่
                cbbMovieType.ValueMember = "Key"; // ใช้ ID ของหมวดหมู่
            }
        }

        // โหลดรายการภาพยนตร์ทั้งหมดลง DataGridView
        private void LoadMovieList()
        {
            using (SqlConnection conn = new SqlConnection(ShareData.connStr))
            {
                conn.Open();
                string query = @"SELECT movieId, movieName, movieDetail, movieDateSale, movieTypeName, 
                                movieLengthHour, movieLengthMinute, movieDVDPrice, movieDVDTotal, 
                                movieImg, movieDirImg
                         FROM movie_tb 
                         INNER JOIN movie_type_tb ON movie_tb.movieTypeId = movie_type_tb.movieTypeId";

                SqlDataAdapter adapter = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                // เคลียร์ DataGridView ก่อนโหลดข้อมูลใหม่
                dgvMovieShowAll.DataSource = null;
                dgvMovieShowAll.Rows.Clear();
                dgvMovieShowAll.Columns.Clear();
                dgvMovieShowAll.AutoGenerateColumns = false;

                // เพิ่มคอลัมน์ข้อมูลข้อความ
                dgvMovieShowAll.Columns.Add(new DataGridViewTextBoxColumn { Name = "movieId", HeaderText = "รหัสภาพยนตร์", DataPropertyName = "movieId", Width = 80 });
                dgvMovieShowAll.Columns.Add(new DataGridViewTextBoxColumn { Name = "movieName", HeaderText = "ชื่อภาพยนตร์", DataPropertyName = "movieName", Width = 150 });
                dgvMovieShowAll.Columns.Add(new DataGridViewTextBoxColumn { Name = "movieDetail", HeaderText = "รายละเอียด", DataPropertyName = "movieDetail", Width = 200 });
                dgvMovieShowAll.Columns.Add(new DataGridViewTextBoxColumn { Name = "movieDateSale", HeaderText = "วันที่วางขาย", DataPropertyName = "movieDateSale", Width = 120 });
                dgvMovieShowAll.Columns.Add(new DataGridViewTextBoxColumn { Name = "movieTypeName", HeaderText = "ประเภทภาพยนตร์", DataPropertyName = "movieTypeName", Width = 120 });

                // เพิ่มคอลัมน์สำหรับระยะเวลาภาพยนตร์
                dgvMovieShowAll.Columns.Add(new DataGridViewTextBoxColumn { Name = "movieLengthHour", HeaderText = "ชั่วโมง", DataPropertyName = "movieLengthHour", Width = 50 });
                dgvMovieShowAll.Columns.Add(new DataGridViewTextBoxColumn { Name = "movieLengthMinute", HeaderText = "นาที", DataPropertyName = "movieLengthMinute", Width = 50 });

                // เพิ่มคอลัมน์ราคาขาย DVD และจำนวนที่มี
                dgvMovieShowAll.Columns.Add(new DataGridViewTextBoxColumn { Name = "movieDVDPrice", HeaderText = "ราคาขาย DVD", DataPropertyName = "movieDVDPrice", Width = 80 });
                dgvMovieShowAll.Columns.Add(new DataGridViewTextBoxColumn { Name = "movieDVDTotal", HeaderText = "จำนวน DVD", DataPropertyName = "movieDVDTotal", Width = 80 });

                // เพิ่มคอลัมน์รูปภาพ
                DataGridViewImageColumn imgCol1 = new DataGridViewImageColumn
                {
                    Name = "movieImg",
                    HeaderText = "โปสเตอร์",
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    Width = 100
                };
                dgvMovieShowAll.Columns.Add(imgCol1);

                DataGridViewImageColumn imgCol2 = new DataGridViewImageColumn
                {
                    Name = "movieDirImg",
                    HeaderText = "รูปผู้กำกับ",
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    Width = 100
                };
                dgvMovieShowAll.Columns.Add(imgCol2);

                // เพิ่มข้อมูลแถวแบบกำหนดเอง (ใส่ Image หลังจากดึงข้อมูล)
                foreach (DataRow row in dt.Rows)
                {
                    Image moviePoster = row["movieImg"] != DBNull.Value ? ShareData.byteArrayToImage((byte[])row["movieImg"]) : null;
                    Image directorImg = row["movieDirImg"] != DBNull.Value ? ShareData.byteArrayToImage((byte[])row["movieDirImg"]) : null;

                    dgvMovieShowAll.Rows.Add(row["movieId"], row["movieName"], row["movieDetail"], row["movieDateSale"], row["movieTypeName"],
                                             row["movieLengthHour"], row["movieLengthMinute"], row["movieDVDPrice"], row["movieDVDTotal"],
                                             moviePoster, directorImg);
                }

                // ปรับให้เลือกเฉพาะเซลล์
                dgvMovieShowAll.SelectionMode = DataGridViewSelectionMode.CellSelect;
                dgvMovieShowAll.ClearSelection();
                btAdd.Focus(); // เปลี่ยนโฟกัสไปที่ปุ่มเพิ่ม
            }
        }

        // รีเซ็ตหน้าฟอร์มให้เหมือนตอนโหลดครั้งแรก
        private void ResetForm()
        {
            lbMovieId.Text = "";
            tbMovieName.Text = "";
            tbMovieDetail.Text = "";
            dtpMovieDateSale.Value = DateTime.Now;
            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;
            cbbMovieType.SelectedIndex = -1;
            tbMovieDVDTotal.Text = "";
            tbMovieDVDPrice.Text = "";
            pcbMovieImg.Image = null;
            pcbDirMovie.Image = null;

            tbMovieName.Enabled = false;
            tbMovieDetail.Enabled = false;
            dtpMovieDateSale.Enabled = false;
            nudMovieHour.Enabled = false;
            nudMovieMinute.Enabled = false;
            cbbMovieType.Enabled = false;
            tbMovieDVDTotal.Enabled = false;
            tbMovieDVDPrice.Enabled = false;
            btSelectImg1.Enabled = false;
            btSelectImg2.Enabled = false;

            btAdd.Enabled = true;
            btEdit.Enabled = false;
            btDel.Enabled = false;
            btSaveAddEdit.Enabled = false;

            //ล้างช่องค้นหาและ ListView
            tbMovieSearch.Text = "";
            lsMovieShow.Items.Clear();
            rdMovieId.Checked = true;
            rdMovieName.Checked = false;
        }
        private string GenerateNewMovieId()
        {
            string newId = "mv001"; // ค่าเริ่มต้นถ้ายังไม่มีข้อมูลในฐานข้อมูล

            using (SqlConnection conn = new SqlConnection(ShareData.connStr))
            {
                conn.Open();
                string query = "SELECT TOP 1 movieId FROM movie_tb ORDER BY movieId DESC"; // หารหัสล่าสุด
                SqlCommand cmd = new SqlCommand(query, conn);
                object result = cmd.ExecuteScalar();

                if (result != null) // ถ้ามีค่าในฐานข้อมูล
                {
                    string lastId = result.ToString(); // รหัสล่าสุด เช่น "mv028"
                    int lastNumber = int.Parse(lastId.Substring(2)); // ดึงเฉพาะตัวเลขออกมา (28)
                    newId = $"mv{(lastNumber + 1):D3}"; // สร้างรหัสใหม่ เช่น "mv029"
                }
            }

            return newId;
        }
        private void btAdd_Click(object sender, EventArgs e)
        {
            // ดึงรหัสภาพยนตร์ล่าสุดจากฐานข้อมูล
            string newMovieId = GenerateNewMovieId();
            lbMovieId.Text = newMovieId; // แสดงรหัสใหม่

            // ปลดล็อกให้กรอกข้อมูลได้
            tbMovieName.Enabled = true;
            tbMovieDetail.Enabled = true;
            dtpMovieDateSale.Enabled = true;
            nudMovieHour.Enabled = true;
            nudMovieMinute.Enabled = true;
            cbbMovieType.Enabled = true;
            tbMovieDVDTotal.Enabled = true;
            tbMovieDVDPrice.Enabled = true;
            btSelectImg1.Enabled = true;
            btSelectImg2.Enabled = true;

            //ปิดปุ่ม "เพิ่ม" และเปิดปุ่ม "บันทึก"
            btAdd.Enabled = false;
            btSaveAddEdit.Enabled = true;

            // 4️⃣ ล้างข้อมูลเก่าที่อาจหลงเหลือ
            tbMovieName.Text = "";
            tbMovieDetail.Text = "";
            dtpMovieDateSale.Value = DateTime.Now;
            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;
            cbbMovieType.SelectedIndex = -1;
            tbMovieDVDTotal.Text = "";
            tbMovieDVDPrice.Text = "";
            pcbMovieImg.Image = null;
            pcbDirMovie.Image = null;
        }

        private void btSaveAddEdit_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(ShareData.connStr))
            {
                conn.Open();

                // ตรวจสอบว่า movieId มีอยู่แล้วหรือไม่
                string checkQuery = "SELECT COUNT(*) FROM movie_tb WHERE movieId = @movieId";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@movieId", lbMovieId.Text);
                int count = (int)checkCmd.ExecuteScalar();

                string query;

                if (count > 0)
                {
                    // ถ้ามีอยู่แล้ว -> UPDATE
                    query = @"UPDATE movie_tb 
                      SET movieName = @movieName, movieDetail = @movieDetail, movieDateSale = @movieDateSale, 
                          movieTypeId = @movieTypeId, movieDVDTotal = @movieDVDTotal, movieDVDPrice = @movieDVDPrice, 
                          movieLengthHour = @movieLengthHour, movieLengthMinute = @movieLengthMinute, 
                          movieImg = @movieImg, movieDirImg = @movieDirImg
                      WHERE movieId = @movieId";
                }
                else
                {
                    // ถ้ายังไม่มี -> INSERT
                    query = @"INSERT INTO movie_tb 
                      (movieId, movieName, movieDetail, movieDateSale, movieTypeId, 
                       movieDVDTotal, movieDVDPrice, movieLengthHour, movieLengthMinute, movieImg, movieDirImg) 
                      VALUES 
                      (@movieId, @movieName, @movieDetail, @movieDateSale, @movieTypeId, 
                       @movieDVDTotal, @movieDVDPrice, @movieLengthHour, @movieLengthMinute, @movieImg, @movieDirImg)";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // ตรวจสอบค่าห้ามว่าง
                    if (string.IsNullOrWhiteSpace(tbMovieName.Text) || string.IsNullOrWhiteSpace(tbMovieDetail.Text))
                    {
                        MessageBox.Show("กรุณากรอกชื่อและรายละเอียดของภาพยนตร์", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // ตรวจสอบค่าที่เป็นตัวเลข
                    if (!int.TryParse(tbMovieDVDTotal.Text, out int dvdTotal) || dvdTotal < 0)
                    {
                        MessageBox.Show("กรุณากรอกจำนวน DVD เป็นตัวเลขที่ถูกต้อง", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    if (!decimal.TryParse(tbMovieDVDPrice.Text, out decimal dvdPrice) || dvdPrice < 0)
                    {
                        MessageBox.Show("กรุณากรอกราคาของ DVD เป็นตัวเลขที่ถูกต้อง", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // ตรวจสอบว่าผู้ใช้เลือกประเภทภาพยนตร์หรือไม่
                    if (cbbMovieType.SelectedValue == null)
                    {
                        MessageBox.Show("กรุณาเลือกประเภทของภาพยนตร์", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    object movieTypeId = cbbMovieType.SelectedValue;

                    // ตรวจสอบว่าผู้ใช้เลือกรูปภาพหรือไม่
                    if (pcbMovieImg.Image == null || pcbDirMovie.Image == null)
                    {
                        MessageBox.Show("กรุณาเลือกรูปภาพของภาพยนตร์และผู้กำกับ", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // แปลงรูปภาพเป็น byte[]
                    byte[] movieImg = ShareData.imageToByteArray(pcbMovieImg.Image);
                    byte[] movieDirImg = ShareData.imageToByteArray(pcbDirMovie.Image);

                    // ใส่ค่า Parameter
                    cmd.Parameters.AddWithValue("@movieId", lbMovieId.Text);
                    cmd.Parameters.AddWithValue("@movieName", tbMovieName.Text.Trim());
                    cmd.Parameters.AddWithValue("@movieDetail", tbMovieDetail.Text.Trim());
                    cmd.Parameters.AddWithValue("@movieDateSale", dtpMovieDateSale.Value);
                    cmd.Parameters.AddWithValue("@movieTypeId", movieTypeId);
                    cmd.Parameters.AddWithValue("@movieDVDTotal", dvdTotal);
                    cmd.Parameters.AddWithValue("@movieDVDPrice", dvdPrice);
                    cmd.Parameters.AddWithValue("@movieLengthHour", nudMovieHour.Value);
                    cmd.Parameters.AddWithValue("@movieLengthMinute", nudMovieMinute.Value);

                    // บันทึกภาพเป็น varbinary(MAX) ห้ามเป็น NULL
                    cmd.Parameters.Add("@movieImg", SqlDbType.VarBinary, -1).Value = movieImg;
                    cmd.Parameters.Add("@movieDirImg", SqlDbType.VarBinary, -1).Value = movieDirImg;

                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show(count > 0 ? "อัปเดตข้อมูลเรียบร้อยแล้ว!" : "เพิ่มข้อมูลภาพยนตร์เรียบร้อยแล้ว!",
                                "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            // โหลดรายการใหม่ และรีเซ็ตฟอร์ม
            LoadMovieList();
            ResetForm();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            //ล้างช่องค้นหา
            tbMovieSearch.Text = "";

            //ล้าง ListView ค้นหา
            lsMovieShow.Items.Clear();

            // รีเซ็ตค่า RadioButton
            rdMovieId.Checked = true;
            rdMovieName.Checked = false;

            // รีเซ็ตฟอร์มเป็นค่าเริ่มต้น
            ResetForm();
            
        }

        private void btExit_Click(object sender, EventArgs e)
        {
            //แสดงกล่องยืนยันก่อนออกจากโปรแกรม
            DialogResult result = MessageBox.Show("คุณต้องการออกจากโปรแกรมหรือไม่?",
                                                  "ยืนยันการออก",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit(); //ปิดโปรแกรม
            }
        }

        private void btSelectImg1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pcbMovieImg.Image = Image.FromFile(ofd.FileName);
            }
        }

        private void btSelectImg2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pcbDirMovie.Image = Image.FromFile(ofd.FileName);
            }
        }

        private void btMovieSearch_Click(object sender, EventArgs e)
        {
            // ตรวจสอบว่าผู้ใช้ป้อนข้อมูลค้นหาหรือยัง
            if (string.IsNullOrWhiteSpace(tbMovieSearch.Text))
            {
                MessageBox.Show("กรุณาป้อนรหัสภาพยนตร์หรือชื่อภาพยนตร์", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection conn = new SqlConnection(ShareData.connStr))
            {
                conn.Open();
                string query = "";

                if (rdMovieId.Checked) // ค้นหาด้วยรหัสภาพยนตร์
                {
                    query = @"SELECT movieId, movieName FROM movie_tb WHERE movieId = @searchText";
                }
                else if (rdMovieName.Checked) // ค้นหาด้วยชื่อภาพยนตร์
                {
                    query = @"SELECT movieId, movieName FROM movie_tb WHERE movieName LIKE @searchText";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    if (rdMovieId.Checked)
                    {
                        cmd.Parameters.AddWithValue("@searchText", tbMovieSearch.Text.Trim());
                    }
                    else if (rdMovieName.Checked)
                    {
                        cmd.Parameters.AddWithValue("@searchText", "%" + tbMovieSearch.Text.Trim() + "%");
                    }

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    //เคลียร์รายการเก่าก่อนเพิ่มใหม่
                    lsMovieShow.Items.Clear();

                    if (dt.Rows.Count > 0)
                    {
                        int index = 1;
                        foreach (DataRow row in dt.Rows)
                        {
                            ListViewItem item = new ListViewItem(index.ToString()); // ลำดับที่
                            item.SubItems.Add(row["movieName"].ToString()); // ชื่อภาพยนตร์
                            item.Tag = row["movieId"].ToString(); // เก็บค่า movieId ไว้ใน Tag
                            lsMovieShow.Items.Add(item);
                            index++;
                        }
                    }
                    else
                    {
                        MessageBox.Show("ไม่พบข้อมูลที่ค้นหา", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void LoadMovieDetail(string movieId)
        {
            using (SqlConnection conn = new SqlConnection(ShareData.connStr))
            {
                conn.Open();
                string query = @"SELECT * FROM movie_tb WHERE movieId = @movieId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@movieId", movieId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        lbMovieId.Text = reader["movieId"].ToString();
                        tbMovieName.Text = reader["movieName"].ToString();
                        tbMovieDetail.Text = reader["movieDetail"].ToString();
                        dtpMovieDateSale.Value = Convert.ToDateTime(reader["movieDateSale"]);
                        nudMovieHour.Value = Convert.ToInt32(reader["movieLengthHour"]);
                        nudMovieMinute.Value = Convert.ToInt32(reader["movieLengthMinute"]);
                        tbMovieDVDTotal.Text = reader["movieDVDTotal"].ToString();
                        tbMovieDVDPrice.Text = reader["movieDVDPrice"].ToString();

                        // ✅ โหลดหมวดหมู่จาก movieTypeId
                        cbbMovieType.SelectedValue = reader["movieTypeId"];

                        // ✅ โหลดรูปภาพ
                        if (reader["movieImg"] != DBNull.Value)
                        {
                            byte[] imageBytes = (byte[])reader["movieImg"];
                            pcbMovieImg.Image = ShareData.byteArrayToImage(imageBytes);
                        }
                        else
                        {
                            pcbMovieImg.Image = null;
                        }

                        if (reader["movieDirImg"] != DBNull.Value)
                        {
                            byte[] imageBytes = (byte[])reader["movieDirImg"];
                            pcbDirMovie.Image = ShareData.byteArrayToImage(imageBytes);
                        }
                        else
                        {
                            pcbDirMovie.Image = null;
                        }
                    }
                }
            }
        }


        private void lsMovieShow_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                string selectedMovieId = e.Item.Tag.ToString();
                LoadMovieDetail(selectedMovieId);

                // ปิดปุ่มเพิ่ม
                btAdd.Enabled = false;

                // เปิดปุ่มแก้ไข และลบ
                btEdit.Enabled = true;
                btDel.Enabled = true;
            }
        }

        private void btEdit_Click(object sender, EventArgs e)
        {
            //เปิดให้แก้ไขข้อมูลได้
            tbMovieName.Enabled = true;
            tbMovieDetail.Enabled = true;
            dtpMovieDateSale.Enabled = true;
            nudMovieHour.Enabled = true;
            nudMovieMinute.Enabled = true;
            cbbMovieType.Enabled = true;
            tbMovieDVDTotal.Enabled = true;
            tbMovieDVDPrice.Enabled = true;
            btSelectImg1.Enabled = true;
            btSelectImg2.Enabled = true;

            //ปิดปุ่มแก้ไข และเปิดปุ่มบันทึก
            btEdit.Enabled = false;
            btSaveAddEdit.Enabled = true;
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            //ตรวจสอบว่ามีภาพยนตร์ที่ถูกเลือกหรือไม่
            if (string.IsNullOrWhiteSpace(lbMovieId.Text))
            {
                MessageBox.Show("กรุณาเลือกภาพยนตร์ที่ต้องการลบ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //ยืนยันการลบข้อมูล
            DialogResult result = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบภาพยนตร์นี้?",
                                                  "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(ShareData.connStr))
                {
                    conn.Open();
                    string query = "DELETE FROM movie_tb WHERE movieId = @movieId";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@movieId", lbMovieId.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("ลบข้อมูลภาพยนตร์เรียบร้อยแล้ว!", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            //รีเฟรช DataGridView
                            LoadMovieList();

                            //รีเซ็ตฟอร์ม
                            ResetForm();
                        }
                        else
                        {
                            MessageBox.Show("เกิดข้อผิดพลาด! ไม่พบข้อมูลที่ต้องการลบ", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
    }
}
