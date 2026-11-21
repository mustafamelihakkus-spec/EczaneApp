using System.Data;
using System.Drawing.Drawing2D;
using Microsoft.Data.SqlClient;

namespace EczaneApp
{
    // Tek pencere: solda menü, sağda seçilen ekran (Dashboard, İlaçlar, Hastalar ...)
    public class AnaForm : Form
    {
        // ---- Renkler ----
        readonly Color arkaPlan = Color.FromArgb(255, 251, 246);
        readonly Color mavi     = Color.FromArgb(134, 99, 235);
        readonly Color yesil    = Color.FromArgb(13, 148, 136);
        readonly Color turuncu  = Color.FromArgb(234, 88, 12);
        readonly Color yaziKoyu = Color.FromArgb(30, 41, 59);
        readonly Color yaziGri  = Color.FromArgb(100, 116, 139);

        // ---- Veritabanı sınıfları ----
        readonly Veritabani vt = new Veritabani();
        readonly IlacIslemleri ilacIslem;
        readonly HastaIslemleri hastaIslem;
        readonly ReceteIslemleri receteIslem;

        // ---- Arayüz parçaları ----
        GradyanPanel menuPanel;
        Panel sayfaPanel;
        Label baslikLabel, altBaslikLabel;
        readonly List<Button> menuButonlari = new List<Button>();
        Button dashboardButon, ilaclarButon, hastaEkleButon, hastalarButon,
               receteKaydetButon, recetelerButon, hastaAraButon, dusukStokButon;

        public AnaForm()
        {
            ilacIslem = new IlacIslemleri(vt);
            hastaIslem = new HastaIslemleri(vt);
            receteIslem = new ReceteIslemleri(vt);

            Text = "Eczane İlaç ve Reçete Takip Sistemi";
            Size = new Size(1250, 760);
            MinimumSize = new Size(1050, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10f);
            BackColor = arkaPlan;

            KeyPreview = true;              // F tuşlarını önce form yakalasın
            KeyDown += AnaForm_KeyDown;

            ArayuzuKur();
            Shown += (s, e) => DashboardGoster();
        }

        // =====================================================
        //  F1 - F7 KLAVYE KISAYOLLARI (switch-case)
        // =====================================================
        private void AnaForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F1:
                    IlaclariGoster();        // İlaçları listele
                    break;
                case Keys.F2:
                    HastaEkleGoster();       // Hasta ekle
                    break;
                case Keys.F3:
                    HastalariGoster();       // Hastaları listele
                    break;
                case Keys.F4:
                    ReceteKaydetGoster();    // Reçete kaydet
                    break;
                case Keys.F5:
                    ReceteleriGoster();      // Reçeteleri listele
                    break;
                case Keys.F6:
                    HastaAraGoster();        // Hasta reçetesi ara (T.C.)
                    break;
                case Keys.F7:
                    DusukStokGoster();       // Düşük stok raporu
                    break;
                case Keys.Escape:
                    CikisYap();              // Çıkış
                    break;
                default:
                    return;                  // Diğer tuşlar normal çalışsın
            }
            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        // =====================================================
        //  ANA DÜZEN (sol menü + sağ içerik)
        // =====================================================
        void ArayuzuKur()
        {
            // Sağ taraf: başlık + sayfa alanı
            var icerikPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(32, 26, 32, 28), BackColor = arkaPlan };
            sayfaPanel = new Panel { Dock = DockStyle.Fill };
            var ustBaslik = new Panel { Dock = DockStyle.Top, Height = 84 };
            baslikLabel = new Label { AutoSize = true, Location = new Point(0, 0), Font = new Font("Segoe UI", 22f), ForeColor = yaziKoyu };
            altBaslikLabel = new Label { AutoSize = true, Location = new Point(3, 46), ForeColor = yaziGri };
            ustBaslik.Controls.Add(baslikLabel);
            ustBaslik.Controls.Add(altBaslikLabel);
            icerikPanel.Controls.Add(sayfaPanel);
            icerikPanel.Controls.Add(ustBaslik);

            // Sol taraf: mavi menü
            menuPanel = new GradyanPanel { Dock = DockStyle.Left, Width = 230 };
            menuPanel.Controls.Add(new Label
            {
                Text = "✚ Eczane",
                Font = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                AutoSize = true,
                Location = new Point(18, 26)
            });

            dashboardButon    = MenuButonu("      Dashboard",          0, DashboardGoster);
            ilaclarButon      = MenuButonu("F1   İlaçlar",             1, IlaclariGoster);
            hastaEkleButon    = MenuButonu("F2   Hasta Ekle",          2, HastaEkleGoster);
            hastalarButon     = MenuButonu("F3   Hastalar",            3, HastalariGoster);
            receteKaydetButon = MenuButonu("F4   Reçete Kaydet",       4, ReceteKaydetGoster);
            recetelerButon    = MenuButonu("F5   Reçeteler",           5, ReceteleriGoster);
            hastaAraButon     = MenuButonu("F6   Hasta Ara (T.C.)",    6, HastaAraGoster);
            dusukStokButon    = MenuButonu("F7   Düşük Stok",          7, DusukStokGoster);
            MenuButonu("ESC  Çıkış", 8, CikisYap);

            menuPanel.Controls.Add(new Label
            {
                Text = "EczaneDB  |  SQL Server",
                Dock = DockStyle.Bottom,
                Height = 44,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(220, 230, 255),
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9f)
            });

            Controls.Add(icerikPanel);
            Controls.Add(menuPanel);
        }

        Button MenuButonu(string metin, int sira, Action tikla)
        {
            var b = new Button
            {
                Text = metin,
                Location = new Point(12, 100 + sira * 46),
                Size = new Size(206, 40),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TabStop = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(35, 255, 255, 255);
            b.FlatAppearance.MouseDownBackColor = Color.FromArgb(70, 255, 255, 255);
            b.Click += (s, e) => tikla();
            menuPanel.Controls.Add(b);
            menuButonlari.Add(b);
            return b;
        }

        // Sağdaki alana yeni ekranı koyar, başlığı ve seçili menüyü günceller
        void SayfaAc(string baslik, string altBaslik, Control icerik, Button aktif)
        {
            foreach (Control eski in sayfaPanel.Controls.Cast<Control>().ToList())
                eski.Dispose();

            icerik.Dock = DockStyle.Fill;
            sayfaPanel.Controls.Add(icerik);
            baslikLabel.Text = baslik;
            altBaslikLabel.Text = altBaslik;

            foreach (var b in menuButonlari) b.BackColor = Color.Transparent;
            aktif.BackColor = Color.FromArgb(60, 255, 255, 255);
        }

        // =====================================================
        //  DASHBOARD (arkadaşının ekranındaki gibi kartlar + 2 tablo)
        // =====================================================
        void DashboardGoster()
        {
            Guvenli(() =>
            {
                var duzen = new TableLayoutPanel { ColumnCount = 1, RowCount = 2 };
                duzen.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
                duzen.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

                var kartlar = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1, Margin = new Padding(0) };
                for (int i = 0; i < 4; i++) kartlar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
                kartlar.Controls.Add(Kart("Toplam ilaç", ilacIslem.Sayi().ToString(), mavi, true), 0, 0);
                kartlar.Controls.Add(Kart("Hastalar", hastaIslem.Sayi().ToString(), mavi, true), 1, 0);
                kartlar.Controls.Add(Kart("Reçeteler", receteIslem.Sayi().ToString(), yesil, true), 2, 0);
                kartlar.Controls.Add(Kart("Toplam satış", receteIslem.ToplamCiro().ToString("N2") + " ₺", turuncu, false), 3, 0);

                var alt = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0, 18, 0, 0) };
                alt.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
                alt.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
                alt.Controls.Add(Kutu("Son reçeteler", Tablo(receteIslem.SonReceteler())), 0, 0);
                var stokKutu = Kutu("Düşük stok (20'nin altı)", Tablo(ilacIslem.DusukStok()));
                stokKutu.Margin = new Padding(18, 0, 0, 0);
                alt.Controls.Add(stokKutu, 1, 0);

                duzen.Controls.Add(kartlar, 0, 0);
                duzen.Controls.Add(alt, 0, 1);
                SayfaAc("Dashboard", "Eczanenin genel görünümü", duzen, dashboardButon);
            });
        }

        // =====================================================
        //  LİSTE EKRANLARI (F1, F3, F5, F7)
        // =====================================================
        void IlaclariGoster()
        {
            Guvenli(() =>
            {
                var dt = ilacIslem.Listele();
                SayfaAc("İlaçlar", "F1  ·  Tüm ilaçlar, fiyat ve stok bilgisi",
                        Kutu($"İlaç listesi ({dt.Rows.Count})", Tablo(dt)), ilaclarButon);
            });
        }

        void HastalariGoster()
        {
            Guvenli(() =>
            {
                var dt = hastaIslem.Listele();
                SayfaAc("Hastalar", "F3  ·  Kayıtlı hastalar",
                        Kutu($"Hasta listesi ({dt.Rows.Count})", Tablo(dt)), hastalarButon);
            });
        }

        void ReceteleriGoster()
        {
            Guvenli(() =>
            {
                var dt = receteIslem.Listele();
                SayfaAc("Reçeteler", "F5  ·  Reçeteler hasta, doktor, ilaç ve eczane bilgileriyle",
                        Kutu($"Reçete listesi ({dt.Rows.Count})", Tablo(dt)), recetelerButon);
            });
        }

        void DusukStokGoster()
        {
            Guvenli(() =>
            {
                var dt = ilacIslem.DusukStok();
                SayfaAc("Düşük Stok", "F7  ·  Stoğu 20'nin altına düşen ilaçlar",
                        Kutu($"Düşük stok raporu ({dt.Rows.Count} ilaç)", Tablo(dt)), dusukStokButon);
            });
        }

        // =====================================================
        //  F2 - HASTA EKLE
        // =====================================================
        void HastaEkleGoster()
        {
            var dis = new Panel();
            var kutu = new Panel { BackColor = Color.White, Location = new Point(0, 0), Size = new Size(580, 320) };

            var tc    = Alan(kutu, "T.C. Kimlik No", 0, new TextBox { Width = 360, MaxLength = 11 });
            var ad    = Alan(kutu, "Ad",             1, new TextBox { Width = 360, MaxLength = 50 });
            var soyad = Alan(kutu, "Soyad",          2, new TextBox { Width = 360, MaxLength = 50 });
            var tel   = Alan(kutu, "Telefon",        3, new TextBox { Width = 360, MaxLength = 20 });

            var kaydet = AnaButon("Kaydet", new Point(170, 240));
            kaydet.Click += (s, e) =>
            {
                string tcNo = tc.Text.Trim();
                if (tcNo.Length != 11 || !tcNo.All(rakam => rakam >= '0' && rakam <= '9'))
                {
                    Uyari("T.C. kimlik numarası 11 haneli olmalı ve sadece rakam içermeli.");
                    return;
                }
                if (ad.Text.Trim() == "" || soyad.Text.Trim() == "")
                {
                    Uyari("Ad ve soyad boş bırakılamaz.");
                    return;
                }
                try
                {
                    hastaIslem.Ekle(tcNo, ad.Text.Trim(), soyad.Text.Trim(), tel.Text.Trim());
                    Bilgi("Hasta kaydedildi.");
                    BeginInvoke(new Action(HastalariGoster));
                }
                catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
                {
                    Uyari("Bu T.C. kimlik numarası zaten kayıtlı.");
                }
                catch (SqlException ex)
                {
                    Hata(ex.Message);
                }
            };
            kutu.Controls.Add(kaydet);
            dis.Controls.Add(kutu);

            SayfaAc("Hasta Ekle", "F2  ·  Yeni hasta kaydı", dis, hastaEkleButon);
            tc.Focus();
        }

        // =====================================================
        //  F4 - REÇETE KAYDET (stok kontrolü + transaction)
        // =====================================================
        void ReceteKaydetGoster()
        {
            Guvenli(() =>
            {
                var dis = new Panel { AutoScroll = true };
                var kutu = new Panel { BackColor = Color.White, Location = new Point(0, 0), Size = new Size(620, 470) };

                var no     = Alan(kutu, "Reçete No", 0, new TextBox { Width = 360, ReadOnly = true, Text = receteIslem.YeniNo().ToString() });
                var tarih  = Alan(kutu, "Tarih",     1, new DateTimePicker { Width = 360, Format = DateTimePickerFormat.Short });
                var doktor = Alan(kutu, "Doktor",    2, Secim(receteIslem.Doktorlar()));
                var hasta  = Alan(kutu, "Hasta",     3, Secim(hastaIslem.Secim()));
                var eczane = Alan(kutu, "Eczane",    4, Secim(receteIslem.Eczaneler()));
                var ilac   = Alan(kutu, "İlaç",      5, Secim(ilacIslem.Secim()));
                var adet   = Alan(kutu, "Adet",      6, new NumericUpDown { Width = 120, Minimum = 1, Maximum = 100, Value = 1 });

                var kaydet = AnaButon("Reçeteyi Kaydet", new Point(170, 384), 200);
                kaydet.Click += (s, e) =>
                {
                    if (doktor.SelectedValue == null || hasta.SelectedValue == null ||
                        eczane.SelectedValue == null || ilac.SelectedValue == null)
                    {
                        Uyari("Lütfen tüm alanları seç.");
                        return;
                    }
                    try
                    {
                        int kaydedilenNo = receteIslem.Kaydet(tarih.Value, (int)adet.Value,
                            (int)doktor.SelectedValue, (int)hasta.SelectedValue,
                            (int)eczane.SelectedValue, (int)ilac.SelectedValue);
                        Bilgi($"Reçete {kaydedilenNo} kaydedildi. İlacın stoğu {adet.Value} adet azaltıldı.");
                        BeginInvoke(new Action(ReceteKaydetGoster));   // ekranı yenile
                    }
                    catch (InvalidOperationException ex)
                    {
                        Uyari(ex.Message + " Reçete kaydedilmedi.");
                    }
                    catch (SqlException ex)
                    {
                        Hata(ex.Message);
                    }
                };
                kutu.Controls.Add(kaydet);
                kutu.Controls.Add(new Label
                {
                    Text = "Kaydederken ilacın stoğu kontrol edilir ve adet kadar düşülür (transaction).",
                    AutoSize = true, ForeColor = yaziGri, Location = new Point(24, 436), Font = new Font("Segoe UI", 9f)
                });
                dis.Controls.Add(kutu);

                SayfaAc("Reçete Kaydet", "F4  ·  Yeni reçete (stok kontrolü ile)", dis, receteKaydetButon);
            });
        }

        // =====================================================
        //  F6 - HASTA REÇETESİ ARA (T.C.)
        // =====================================================
        void HastaAraGoster()
        {
            var dis = new Panel();
            var ust = new Panel { Dock = DockStyle.Top, Height = 84, BackColor = Color.White };
            var tc = new TextBox { Location = new Point(150, 19), Width = 220, MaxLength = 11, PlaceholderText = "10234567890" };
            var ara = AnaButon("Ara", new Point(385, 12), 110);
            var sonuc = new Label { Location = new Point(150, 58), AutoSize = true, ForeColor = yaziGri };
            ust.Controls.Add(new Label { Text = "T.C. Kimlik No:", Location = new Point(18, 22), AutoSize = true, ForeColor = yaziGri });
            ust.Controls.Add(tc);
            ust.Controls.Add(ara);
            ust.Controls.Add(sonuc);

            var tablo = Tablo(null);

            void Ara()
            {
                try
                {
                    var dt = hastaIslem.Receteleri(tc.Text.Trim());
                    tablo.DataSource = dt;
                    sonuc.Text = dt.Rows.Count == 0
                        ? "Bu T.C. numarasına ait reçete bulunamadı."
                        : dt.Rows.Count + " reçete bulundu.";
                }
                catch (SqlException ex)
                {
                    Hata(ex.Message);
                }
            }

            ara.Click += (s, e) => Ara();
            tc.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { Ara(); e.SuppressKeyPress = true; }
            };

            dis.Controls.Add(Kutu("Reçeteler", tablo));
            dis.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 16 });
            dis.Controls.Add(ust);

            SayfaAc("Hasta Ara", "F6  ·  T.C. kimlik numarasıyla hastanın reçeteleri", dis, hastaAraButon);
            tc.Focus();
        }

        // =====================================================
        //  ESC - ÇIKIŞ
        // =====================================================
        void CikisYap()
        {
            if (MessageBox.Show("Programdan çıkılsın mı?", "Çıkış",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                Close();
        }

        // =====================================================
        //  YARDIMCI METOTLAR (kart, kutu, tablo, buton ...)
        // =====================================================
        Panel Kart(string baslik, string deger, Color renk, bool sagBosluk)
        {
            var kart = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Margin = new Padding(0, 0, sagBosluk ? 16 : 0, 0) };
            kart.Controls.Add(new Label { Text = baslik, AutoSize = true, ForeColor = yaziGri, Location = new Point(24, 18) });
            kart.Controls.Add(new Label { Text = deger, AutoSize = true, ForeColor = yaziKoyu, Font = new Font("Segoe UI", 24f), Location = new Point(20, 44) });
            kart.Controls.Add(new Panel { Dock = DockStyle.Left, Width = 5, BackColor = renk });
            return kart;
        }

        Panel Kutu(string baslik, Control icerik)
        {
            var kutu = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18, 14, 18, 14), Margin = new Padding(0) };
            icerik.Dock = DockStyle.Fill;
            kutu.Controls.Add(icerik);
            kutu.Controls.Add(new Label
            {
                Text = baslik, Dock = DockStyle.Top, Height = 34,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold), ForeColor = yaziKoyu
            });
            return kutu;
        }

        DataGridView Tablo(DataTable veri)
        {
            var g = new DataGridView
            {
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                GridColor = Color.FromArgb(226, 232, 240),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                EnableHeadersVisualStyles = false,
                ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
                ColumnHeadersHeight = 36,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing
            };
            g.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.ForeColor = yaziGri;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;
            g.DefaultCellStyle.ForeColor = yaziKoyu;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            g.DefaultCellStyle.SelectionForeColor = yaziKoyu;
            g.RowTemplate.Height = 32;

            // Tarih ve para sütunlarını güzel göster
            g.DataBindingComplete += (s, e) =>
            {
                foreach (DataGridViewColumn c in g.Columns)
                {
                    if (c.ValueType == typeof(DateTime))
                        c.DefaultCellStyle.Format = "dd.MM.yyyy";
                    else if (c.ValueType == typeof(decimal))
                    {
                        c.DefaultCellStyle.Format = "N2";
                        c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    }
                }
                g.ClearSelection();
            };

            if (veri != null) g.DataSource = veri;
            return g;
        }

        T Alan<T>(Panel kutu, string etiket, int sira, T kontrol) where T : Control
        {
            int y = 28 + sira * 50;
            kutu.Controls.Add(new Label { Text = etiket, Location = new Point(24, y + 4), AutoSize = true, ForeColor = yaziGri });
            kontrol.Location = new Point(170, y);
            kutu.Controls.Add(kontrol);
            return kontrol;
        }

        ComboBox Secim(DataTable veri)
        {
            return new ComboBox
            {
                Width = 360,
                DropDownStyle = ComboBoxStyle.DropDownList,
                DisplayMember = "Metin",
                ValueMember = "ID",
                DataSource = veri
            };
        }

        Button AnaButon(string metin, Point konum, int genislik = 170)
        {
            var b = new Button
            {
                Text = metin,
                Location = konum,
                Size = new Size(genislik, 40),
                FlatStyle = FlatStyle.Flat,
                BackColor = mavi,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        // Veritabanı hatası olursa program kapanmasın, mesaj göstersin
        void Guvenli(Action islem)
        {
            try
            {
                islem();
            }
            catch (SqlException ex)
            {
                Hata("Veritabanına bağlanılamadı ya da sorgu hata verdi.\n\n" + ex.Message +
                     "\n\nVeritabani.cs içindeki bağlantı cümlesini (Server=...) kontrol et.");
            }
        }

        void Bilgi(string m) => MessageBox.Show(m, "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        void Uyari(string m) => MessageBox.Show(m, "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        void Hata(string m)  => MessageBox.Show(m, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    // Sol menünün koyu maviden açık maviye geçişli arka planı
    public class GradyanPanel : Panel
    {
        public GradyanPanel()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Width <= 0 || Height <= 0) { base.OnPaintBackground(e); return; }
            using var firca = new LinearGradientBrush(ClientRectangle,
                Color.FromArgb(20, 83, 45), Color.FromArgb(20, 83, 46), LinearGradientMode.Vertical);
            e.Graphics.FillRectangle(firca, ClientRectangle);
        }
    }
}
