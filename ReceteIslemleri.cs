using System.Data;
using Microsoft.Data.SqlClient;

namespace EczaneApp
{
    // Recete tablosuyla ilgili işler (F4, F5, Dashboard)
    public class ReceteIslemleri
    {
        private readonly Veritabani vt;

        public ReceteIslemleri(Veritabani vt)
        {
            this.vt = vt;
        }

        // F4 - Reçete kaydet: stok azaltma + reçete ekleme TEK transaction içinde
        public int Kaydet(DateTime tarih, int adet, int doktorId, int hastaId, int eczaneId, int ilacId)
        {
            if (adet <= 0)
                throw new ArgumentOutOfRangeException(nameof(adet));

            using var baglanti = vt.Ac();
            using var islem = baglanti.BeginTransaction();

            // Numarayı kayıt anında ayır ve aynı numaranın eşzamanlı seçilmesini önle.
            using var numara = new SqlCommand(
                "SELECT ISNULL(MAX(ReceteNo), 1000) + 1 FROM Recete WITH (UPDLOCK, HOLDLOCK)", baglanti, islem);
            int no = (int)numara.ExecuteScalar();

            using var stok = new SqlCommand(
                "UPDATE Ilac SET StokMiktari = StokMiktari - @adet " +
                "WHERE IlacID = @ilac AND StokMiktari >= @adet", baglanti, islem);
            stok.Parameters.AddWithValue("@adet", adet);
            stok.Parameters.AddWithValue("@ilac", ilacId);
            if (stok.ExecuteNonQuery() == 0)
                throw new InvalidOperationException("Yeterli stok yok.");

            using var recete = new SqlCommand(
                "INSERT INTO Recete (ReceteNo, Tarih, Adet, DoktorID, HastaID, EczaneID, IlacID) " +
                "VALUES (@no, @tarih, @adet, @doktor, @hasta, @eczane, @ilac)", baglanti, islem);
            recete.Parameters.AddWithValue("@no", no);
            recete.Parameters.Add("@tarih", SqlDbType.Date).Value = tarih.Date;
            recete.Parameters.AddWithValue("@adet", adet);
            recete.Parameters.AddWithValue("@doktor", doktorId);
            recete.Parameters.AddWithValue("@hasta", hastaId);
            recete.Parameters.AddWithValue("@eczane", eczaneId);
            recete.Parameters.AddWithValue("@ilac", ilacId);
            recete.ExecuteNonQuery();

            islem.Commit();
            return no;
        }

        // F5 - Tüm reçeteler (S1 sorgusu)
        public DataTable Listele()
        {
            return vt.Sorgula(
                "SELECT r.ReceteNo AS [Reçete No], r.Tarih, h.Ad + ' ' + h.Soyad AS Hasta, " +
                "d.Ad + ' ' + d.Soyad AS Doktor, i.IlacAdi AS [İlaç], r.Adet, e.EczaneAdi AS Eczane " +
                "FROM Recete r " +
                "JOIN Hasta h ON h.HastaID = r.HastaID " +
                "JOIN Doktor d ON d.DoktorID = r.DoktorID " +
                "JOIN Ilac i ON i.IlacID = r.IlacID " +
                "JOIN Eczane e ON e.EczaneID = r.EczaneID " +
                "ORDER BY r.ReceteNo");
        }

        // Dashboard - son 6 reçete
        public DataTable SonReceteler()
        {
            return vt.Sorgula(
                "SELECT TOP 6 h.Ad + ' ' + h.Soyad AS Hasta, i.IlacAdi AS [İlaç], r.Adet, " +
                "r.Adet * i.Fiyat AS Tutar, r.Tarih " +
                "FROM Recete r " +
                "JOIN Hasta h ON h.HastaID = r.HastaID " +
                "JOIN Ilac i ON i.IlacID = r.IlacID " +
                "ORDER BY r.Tarih DESC, r.ReceteNo DESC");
        }

        public int Sayi()
        {
            return Convert.ToInt32(vt.TekDeger("SELECT COUNT(*) FROM Recete"));
        }

        // Dashboard - toplam satış tutarı
        public decimal ToplamCiro()
        {
            return Convert.ToDecimal(vt.TekDeger(
                "SELECT ISNULL(SUM(r.Adet * i.Fiyat), 0) FROM Recete r JOIN Ilac i ON i.IlacID = r.IlacID"));
        }

        // Yeni reçete numarası = en büyük numara + 1
        public int YeniNo()
        {
            return Convert.ToInt32(vt.TekDeger("SELECT ISNULL(MAX(ReceteNo), 1000) + 1 FROM Recete"));
        }

        public DataTable Doktorlar()
        {
            return vt.Sorgula(
                "SELECT DoktorID AS ID, Ad + ' ' + Soyad + ' (' + Uzmanlik + ')' AS Metin FROM Doktor ORDER BY Ad");
        }

        public DataTable Eczaneler()
        {
            return vt.Sorgula("SELECT EczaneID AS ID, EczaneAdi AS Metin FROM Eczane ORDER BY EczaneAdi");
        }
    }
}
