using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Конвертер_курса_валют
{
    public partial class Form1 : Form
    {
        SQLiteConnection connection = null;
        SQLiteDataAdapter adapter = null;
        string currency1 = null;
        string currency2 = null;
        string file = null;
        DataTable dtable = null;
        DataTable pic = null;
        string[] tables = null;
        string n1;
        string n2;
        string x;
        string y;
        public Form1()
        {
            InitializeComponent();
            file = @"D:\\Study\\Сопровождение ПО\\Валюты.db";
            connection = new SQLiteConnection("Data Source=" + file);
            connection.Open();
            adapter = new SQLiteDataAdapter("SELECT Валюта FROM Валюты", connection);
            dtable = new DataTable();
            adapter.Fill(dtable);
            tables = new string[dtable.Rows.Count];

            for (int i = 0; i < tables.Length; i++)
            {
                tables[i] = dtable.Rows[i].ItemArray[0].ToString();
            }
            comboBox1.Items.AddRange(tables);
            comboBox2.Items.AddRange(tables);

            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;

            label1.Visible = false;
            label2.Visible = false;
            label1.BackColor = Color.Transparent;
            label2.BackColor = Color.Transparent;
            label1.ForeColor = Color.White;
            label2.ForeColor = Color.White;
            pictureBox1.BackColor = Color.Transparent;
            pictureBox2.BackColor = Color.Transparent;
            textBox1.ReadOnly = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            currency1 = comboBox1.SelectedItem.ToString();
            adapter = new SQLiteDataAdapter($"SELECT Флаг FROM Валюты where Валюта = '{currency1}'", connection);
            pic = new DataTable();
            adapter.Fill(pic);
            var im = new MemoryStream((byte[])pic.Rows[0].ItemArray[0]);
            pictureBox1.Image = Image.FromStream(im);
            adapter = new SQLiteDataAdapter($"SELECT Название FROM Валюты where Валюта = '{currency1}'", connection);
            dtable = new DataTable();
            adapter.Fill(dtable);
            label1.Visible = true;
            label1.Text = Convert.ToString(dtable.Rows[0].ItemArray[0]);
            button1.Enabled = true;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            currency2 = comboBox2.SelectedItem.ToString();
            adapter = new SQLiteDataAdapter($"SELECT Флаг FROM Валюты where Валюта = '{currency2}'", connection);
            pic = new DataTable();
            adapter.Fill(pic);
            var im = new MemoryStream((byte[])pic.Rows[0].ItemArray[0]);
            pictureBox2.Image = Image.FromStream(im);
            adapter = new SQLiteDataAdapter($"SELECT Название FROM Валюты where Валюта = '{currency2}'", connection);
            dtable = new DataTable();
            adapter.Fill(dtable);
            label2.Visible = true;
            label2.Text = Convert.ToString(dtable.Rows[0].ItemArray[0]);
            button1.Enabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (comboBox1.SelectedIndex == -1 || comboBox2.SelectedIndex == -1)
            {
                MessageBox.Show(
                           "Валюты не выбраны",
                           "Информация",
                           MessageBoxButtons.OKCancel,
                           MessageBoxIcon.Information);
                money.Text = "";
            }

            else
            {
                if (comboBox1.SelectedIndex == comboBox2.SelectedIndex)
                {
                    MessageBox.Show(
                       "Выбрана одна и та же валюта",
                       "Информация",
                       MessageBoxButtons.OKCancel,
                       MessageBoxIcon.Information);

                    money.Text = "";
                    textBox1.Text = "";
                    button1.Enabled = false;
                }

                else
                {
                    if (money.Text == "")
                    {
                        MessageBox.Show(
                           "Вы не ввели конвертируемое число",
                           "Информация",
                           MessageBoxButtons.OKCancel,
                           MessageBoxIcon.Information);
                    }

                    else
                    {
                        try
                        {
                            Convert.ToInt32(money.Text);
                            double value = Convert.ToDouble(money.Text);

                            if (value <=0)
                            {
                                MessageBox.Show(
                                        "Нельзя конвертировать отрицательное число",
                                        "Информация",
                                        MessageBoxButtons.OKCancel,
                                        MessageBoxIcon.Information);
                                money.Text = "";
                                textBox1.Text = "";
                            }

                            else
                            {
                                var url = "http://www.cbr.ru/scripts/XML_daily.asp";

                                var request = WebRequest.Create(url);
                                request.Method = "GET";

                                var webResponse = request.GetResponse();
                                var webStream = webResponse.GetResponseStream();

                                var reader = new StreamReader(webStream, Encoding.Default);
                                var data = reader.ReadToEnd();

                                XElement source = XElement.Parse(data);
                                var valutes = source.Descendants("Valute");

                                foreach (XElement valute in valutes)
                                {
                                    if (currency1 == "RUB")
                                    {
                                        x = Convert.ToString(1);
                                        n1 = Convert.ToString(1);

                                    }
                                    if (currency2 == "RUB")
                                    {
                                        y = Convert.ToString(1);
                                        n2 = Convert.ToString(1);

                                    }
                                    if (valute.Element("CharCode").Value == currency1)
                                    {
                                        x = (string)valute.Element("Value");
                                        n1 = (string)valute.Element("Nominal");
                                    }
                                    if (valute.Element("CharCode").Value == currency2)
                                    {

                                        y = (string)valute.Element("Value");
                                        n2 = (string)valute.Element("Nominal");
                                    }


                                }

                                double z = ((value * Convert.ToDouble(x)) / (Convert.ToDouble(y) * Convert.ToDouble(n1))) * Convert.ToDouble(n2);
                                textBox1.Text = Math.Round(z, 4).ToString();
                            }
                        }

                            
                        catch (FormatException)
                        {
                            MessageBox.Show("Введено не число!",
                                "Информация",
                               MessageBoxButtons.OKCancel,
                               MessageBoxIcon.Information);
                            money.Text = "";
                        }
                    }
                }
            }
        }
    }
}